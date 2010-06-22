using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Guanima.Redis.Commands;
using Guanima.Redis.Commands.PubSub;
using Guanima.Redis.Extensions;
using Guanima.Redis.Utils;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Client
{
    public class SubscriptionState : Disposable
    {
        private readonly object _lockObject = new object();
        private PooledSocket _socket;
        private RedisReplyParser _parser;
        private ArraySegment<byte>? _readBuffer;        
        private readonly IRedisNode _node;
        private readonly CommandBuffer _commandBuffer = new CommandBuffer();
        private readonly List<string> _activeChannels = new List<string>();
        public RedisSubscription Subscription;
        public IRedisNode Node { get { return _node; } }
        
        public PooledSocket Socket
        {
            get { return (_socket ?? (_socket = Node.Acquire())); }
        }

        public Socket RawSocket { get { return Socket.Socket; } }

        public CommandBuffer CommandBuffer { get { return _commandBuffer; } }
        
        public ArraySegment<byte> ReadBuffer 
        {
            get
            {
                if (_readBuffer == null)
                {
                    int dummy;
                    _readBuffer = Subscription.GetBuffer(out dummy); 
                }
                return _readBuffer.Value;
            }
        }

        public void FreeReadBuffer()
        {
            if (_readBuffer != null)
            {
                Subscription.FreeBuffer(_readBuffer.Value);
                _readBuffer = null;
            }            
        }

        public ManualResetEvent NoMoreSubscriptions;
        public ManualResetEvent SendDone;

        public List<String> Channels { get { return _activeChannels; } }

        public int SubscriptionCount
        {
            get
            {
                return (_activeChannels == null) ? 0 : _activeChannels.Count;
            }
        }

       
        public SubscriptionState(IRedisNode node)
        {
            _node = node;
            NoMoreSubscriptions = new ManualResetEvent(false);
            SendDone = new ManualResetEvent(false);
        }

        public void BufferCommand(byte[] name, IEnumerable<String> keys)
        {
            if (keys == null || keys.Count() == 0)
            {
                CommandBuffer.Append(new byte[][] {name});
            } 
            else
            {
                var args = CommandUtils.ConstructParameters(name, keys);
                CommandBuffer.Append(args);
            }
        }

        public void BufferCommand(RedisCommand command)
        {
            CommandBuffer.Append(command);    
        }

        public RedisReplyParser ReplyParser
        {
            get { return _parser ?? (_parser = new RedisReplyParser(ReplyReceived, null));}
        }

        private void ReplyReceived(object data, RedisValue value)
        {
            //
            if (value.Type == RedisValueType.Error)
            {
                // set
                throw new RedisClientException(value.Text);
            }
            Subscription.HandleMessage(this, value.MultiBulkValues);
        }


        public void DoLock(Action action)
        {
            if (action != null)
            {
                lock (_lockObject)
                {
                    action();
                }
            }
        }

        protected override void Release()
        {
            if (Socket != null)
            {
                ((IDisposable)Socket).Dispose();
            }
            FreeReadBuffer();
        }
    }


    public class RedisSubscription : Disposable, IRedisSubscription
    {
        // TODO: Make this configurable
        public const int ReadBufferSize = 2048;

        private readonly RedisClient _redisClient;
        private readonly List<IRedisNode> _nodes = new List<IRedisNode>();
        private readonly IDictionary<IRedisNode, SubscriptionState> _states = new Dictionary<IRedisNode, SubscriptionState>();
        private List<string> _activeChannels;
        private readonly ManualResetEvent _shutdownCompleted = new ManualResetEvent(false);
        private readonly ManualResetEvent _messageReceived = new ManualResetEvent(false);
        private readonly ManualResetEvent _noSubscriptions = new ManualResetEvent(false);

        private readonly BufferManager _bufferManager;

        private int _subscriptionCount = 0;
        private int _shutdown = 0;

        private static readonly byte[] SubscribeWord = "subscribe".ToUtf8ByteArray();
        private static readonly byte[] UnSubscribeWord = "unsubscribe".ToUtf8ByteArray();
        private static readonly byte[] MessageWord = "message".ToUtf8ByteArray();
        private readonly object _lockObject = new object();

        public RedisSubscription(RedisClient client)
        {
            _redisClient = client;
            SubscriptionCount = 0;
            _activeChannels = new List<string>();
            _nodes.AddRange(client.GetNodes());
            _bufferManager = new BufferManager(10, ReadBufferSize);
        }

        public RedisSubscription(RedisClient client, params IRedisNode[] nodes)
        {
            _redisClient = client;
            SubscriptionCount = 0;
            _activeChannels = new List<string>();
            _nodes.AddRange(nodes);
        }

        public Action<string> OnSubscribe { get; set; }
        public Action<string, byte[]> OnMessage { get; set; }
        public Action<string> OnUnSubscribe { get; set; }
        public Action<Exception> OnException { get; set; }

        public int SubscriptionCount
        {
            get { return _subscriptionCount;}
            private set { _subscriptionCount = value; }
        }

        public void Subscribe(params string[] channels)
        {
            Subscribe((IEnumerable<string>)channels);
        }

        public void Subscribe(IEnumerable<string> channels)
        {
            var chans = new List<string>();
            var patterns = new List<string>();
            SplitChannels(channels, chans, patterns);

            int count = chans.Count + patterns.Count;
            if (count > 0)
            {
                foreach (var node in _nodes)
                {
                    var state = GetSubscriptionState(node);
                    if (patterns.Count > 0)
                    {
                        state.BufferCommand(Command.PSubscribe, patterns);
                    }
                    if (chans.Count > 0)
                    {
                        state.BufferCommand(Command.Subscribe, chans);
                    }
                    BeginSend(state);
                }
            }
        }


        public void UnSubscribe(params string[] channels)
        {
            if (channels.Length == 0)
            {
                UnSubscribeAll();
                return;
            }
            UnSubscribe((IEnumerable<string>)channels);
        }

        public void UnSubscribe(IEnumerable<string> channels)
        {
            var chans = new List<string>();
            var patterns = new List<string>();
            SplitChannels(channels, chans, patterns);

            var handles = new List<ManualResetEvent>();
            int count = chans.Count + patterns.Count;
            if (count > 0)
            {
                foreach (var node in _nodes)
                {
                    var state = GetSubscriptionState(node);
                    state.CommandBuffer.Reset();

                    if (patterns.Count > 0)
                    {
                        state.BufferCommand(Command.PUnSubscribe, patterns);
                    }
                    if (chans.Count > 0)
                    {
                        state.BufferCommand(Command.UnSubscribe, chans);
                    }

                    BeginSend(state);
                }
            }
        }

        internal void CallSubscribeHandler(int delta)
        {
            
        }

        public void UnSubscribeAll()
        {
            //if (_activeChannels.Count == 0) return;
            if (HasSubscriptions)
            {
                foreach (var node in _nodes)
                {
                    var state = GetSubscriptionState(node);
                    if (state.Channels.Count > 0)
                    {
                        state.BufferCommand( new UnsubscribeCommand());
                        BeginSend(state);
                    }
                }
                _noSubscriptions.WaitOne();
            }
            _noSubscriptions.Reset();
            _activeChannels = new List<string>();
        }


        protected override void Release()
        {
            UnSubscribeAll();
        }

        #region Utils

        internal void HandleException(Exception ex)
        {
            if (ex is ThreadAbortException)
            {
                Console.WriteLine("Thread abort");
                throw ex;
            }
            if (OnException != null)
            {
                OnException(ex);
                return;
            }
            throw ex;
        }


        private bool HasSubscriptions
        {
            get
            {
                return (0 != Interlocked.CompareExchange(ref _subscriptionCount, _subscriptionCount, 0));
            }    
        }

        public bool IsShutDown
        {
            get 
            {
                return (1 == Interlocked.CompareExchange(ref _shutdown, _shutdown, 1));
            }
        }

        public void Shutdown()
        {
            UnSubscribeAll();
            Interlocked.Exchange(ref _shutdown, 1);
            // _noSubscriptions.WaitOne();
        }


        internal ArraySegment<byte> GetBuffer(out int bufferSize)
        {
            bufferSize = _bufferManager.SegmentSize;
            return _bufferManager.CheckOut();
        }

        internal void FreeBuffer(ArraySegment<byte> buffer)
        {
            _bufferManager.CheckIn(buffer);
        }

        public SubscriptionState GetSubscriptionState(IRedisNode node)
        {
            lock(_lockObject)
            {
                SubscriptionState result;
                if (!_states.TryGetValue(node, out result))
                {
                    result = new SubscriptionState(node) {Subscription = this};
                    _states.Add(node, result);
                }
                return result;
            }    
        }


        void DoLock(Action action)
        {
            if (action != null)
            {
                lock (_lockObject)
                {
                    action();
                }
            }
        }

        private static void SplitChannels(IEnumerable<string> original, ICollection<string> channels, ICollection<string> patterns)
        {
            foreach (var item in original)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    if (item.Contains("*"))
                        patterns.Add(item);
                    else
                        channels.Add(item);
                }
            }
        }


        static void BeginSend(SubscriptionState state)
        {
            var buffer = state.CommandBuffer.Data;
            var buflen = state.CommandBuffer.Size;

            var client = state.RawSocket;

            client.BeginSend(buffer, 0, buflen, SocketFlags.None, new AsyncCallback(EndSend), state);
            state.SendDone.WaitOne();
            state.SendDone.Reset();
        }


        static void EndSend(IAsyncResult ar)
        {
            var state = (SubscriptionState)ar.AsyncState;
            state.RawSocket.EndSend(ar);
            state.CommandBuffer.Reset();
            state.SendDone.Set();
            BeginReadMessages(state);

            //var self = state.Subscription;
            //self._messageReceived.WaitOne();
            //self._messageReceived.Reset();
        }


        static void BeginReadMessages(SubscriptionState state)
        {
            var self = state.Subscription;
            
            if (self.IsShutDown)
                return;

            var client = state.RawSocket;

            int buflen = RedisSubscription.ReadBufferSize;
            var buffer = state.ReadBuffer.Array;
            client.BeginReceive(buffer, 0, buflen, SocketFlags.None, 
                new AsyncCallback(EndReadMessages), state);
        }

        static void EndReadMessages(IAsyncResult ar)
        {
            var state = (SubscriptionState)ar.AsyncState;
            var socket = state.RawSocket;
            var self = state.Subscription;
            try
            {
                SocketError errorCode;
                int bytesRead = socket.EndReceive(ar, out errorCode);

                if (bytesRead == 0)
                {
                    state.DoLock(() =>
                    {
                        if (state.SubscriptionCount > 0 && !self.IsShutDown)
                        {
                            BeginReadMessages(state);
                            return;
                        }
                    });

                    state.FreeReadBuffer();
                    return;
                }

                if (errorCode != SocketError.Success)
                {
                    state.FreeReadBuffer();
                    throw new RedisClientException("Socket error reading reply : " +
                                             Enum.GetName(typeof (SocketError), errorCode));
                }

                var buffer = state.ReadBuffer.Array;
                state.ReplyParser.Update(buffer, 0, bytesRead);
                state.FreeReadBuffer();

                state.DoLock(() =>
                {
                    if (state.SubscriptionCount > 0 && !self.IsShutDown)
                        BeginReadMessages(state);
                });
            }
            catch(ThreadAbortException)
            {
                Console.WriteLine("Thread aborted");
                throw;
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine("EndRecieve. Exception = '{0}'", msg);
                state.Subscription.HandleException(ex);
            }
        }


        internal void HandleMessage(SubscriptionState state, RedisValue[] multiBytes)
        {
            try
            {
                for (var i = 0; i < multiBytes.Length; i += 3)
                {
                    var messageType = multiBytes[i];
                    var channel = (string)multiBytes[i + 1];

                    if (MessageWord.IsEqualTo(messageType))
                    {
                        if (OnMessage != null)
                            OnMessage(channel, multiBytes[i + 2]);
                    }
                    else if (SubscribeWord.IsEqualTo(messageType))
                    {
                        var count = (int)multiBytes[i + 2];

                        state.DoLock(() =>
                        {
                            int delta = (count - state.SubscriptionCount);
                            Interlocked.Add(ref _subscriptionCount, delta);
                            state.Channels.Add(channel);
                            // if (count != state.Channels.Count) { throw }
                        });

                        if (OnSubscribe != null)
                            OnSubscribe(channel);

                        if (HasSubscriptions)
                            _noSubscriptions.Reset();

                    }
                    else if (UnSubscribeWord.IsEqualTo(messageType))
                    {
                        var count = (int)multiBytes[i + 2];

                        state.DoLock(() =>
                        {
                            int delta = (count - state.SubscriptionCount);
                            Interlocked.Add(ref _subscriptionCount, delta);
                            state.Channels.Remove(channel);
                            // if (state.Channels.Count != count) -- error
                        });

                        if (OnUnSubscribe != null)
                            OnUnSubscribe(channel);

                        if (!HasSubscriptions)
                            _noSubscriptions.Set();

                    }
                    else
                    {
                        throw new RedisException(
                            "Invalid state. Expected [subscribe|unsubscribe|message] got: " + messageType);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion
    }
}