using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Guanima.Redis.Commands;
using Guanima.Redis.Commands.Connection;
using Guanima.Redis.Commands.Generic;
using Guanima.Redis.Commands.Transactions;
using Guanima.Redis.Extensions;
using Guanima.Redis.Utils;

namespace Guanima.Redis.Client
{
    public class RedisCommandQueue : Disposable
    {
        private readonly byte[] CrLf = new[] { (byte)'\r', (byte)'\n' };
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(RedisCommandQueue));

        private readonly Queue<RedisCommand> _sendQueue;
		private readonly Queue<RedisCommand> _receiveQueue;
        private readonly ManualResetEvent _sendMre = new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveMre = new ManualResetEvent(false);
        private CommandBuffer _commandBuffer;
        private PooledSocket _socket;
		private readonly IRedisNode _node;

        private bool _sendAllOnRead;
        private bool _transactional;
        private int _currentDB;
        private int _multiCount;

        public RedisCommandQueue(IRedisNode node)
            :this(node, false)
        {
        }

        public RedisCommandQueue(IRedisNode node, bool transactional)
        {
            _transactional = transactional;
            _node = node;
            _sendQueue = new Queue<RedisCommand>();
            _receiveQueue = new Queue<RedisCommand>();
            _commandBuffer = new CommandBuffer();
        }

		public IRedisNode Node 
		{
			get { return _node; }
		}
		
		public PooledSocket Socket 
        {
			get 
			{
				_socket = _socket ?? AcquireSocket();
				return _socket;
			}
		}

        public bool IsTransactional
        {
            get { return _transactional; }
            set { SetTransactional(value); }
        }
		
        public int CurrentDB
        {
            get { return _currentDB; }
            set { SetCurrentDB(value); }
        }

        private void SetCurrentDB(int db)
        {
            if (_currentDB != db)
            {
                _currentDB = db;
                Flush();
            }
        }

        private void SetTransactional(bool value)
        {
            if (value != _transactional)
            {
                // flush what we have now
                Flush();
                _transactional = value;
            }    
        }

        public bool SendAllOnRead
        {
            get { return _sendAllOnRead; }
            set { _sendAllOnRead = value; }
        }

        public int Length
        {
            get { return _sendQueue.Count + _receiveQueue.Count; }
        }

        public RedisValue ExecValue(RedisCommand command)
        {
            Enqueue(command);
            ReadResultForCommand(command);
            return command.Value;
        }

		public RedisCommand EnqueueQueue(params RedisValue[] elements)
        {
            return Enqueue( new AdHocCommand(elements) );
        }

	
		public RedisCommand Enqueue(RedisCommand command)
        {
		    var empty = _sendQueue.Count == 0;
            if (empty)
            {
                var socket = Socket;
                RedisCommand cmd = null;

                if (!socket.IsAuthorized && !String.IsNullOrEmpty(Node.Password) && !IsAuth(command))
                {
                    cmd = new AuthCommand(Node.Password);
                    _sendQueue.Enqueue( cmd );
                }

                // Select proper db if specified in config or (socket.CurrentDB <> currentDB)
                // Read the comments on Select to get some background on the following.
                // Im not sure i like this.
                if (socket.CurrentDb != CurrentDB && !IsSelect(command))
                {
                    cmd = new SelectCommand(CurrentDB);              
                    _sendQueue.Enqueue(cmd);
                }
                if (cmd != null)
                    cmd.Queue = this;
            }
            if (empty && IsTransactional)
            {
                _sendQueue.Enqueue(new MultiCommand());
                _multiCount++;
            }
            if (IsMulti(command))
            {
                if (IsTransactional)
                    throw new RedisClientException("Cannot nest transactions");
                _multiCount++;
            }
            if (IsExec(command))
            {
                //if (_multiCount == 1)
            }
            _sendQueue.Enqueue(command);
		    command.Queue = this;
            return command;
        }

        public void SendAllCommands()
        {
            if (_sendQueue.Count == 0)
                return;
            
            if (IsTransactional)
            {
                _sendQueue.Enqueue(new ExecCommand());
            }
            while (_sendQueue.Count != 0) 
            {
                var command = _sendQueue.Dequeue();
                BufferOutput(command);
                _receiveQueue.Enqueue(command);
            }
            FlushCommandBuffer();
        }

        public void ReadAllResults()
        {
            SendAllCommands();
            BeginReadReplies();
        }

        void SendCommand(RedisCommand command)
        {
            if (_sendQueue.Contains(command)) 
            {
                RedisCommand dequeued;
                do 
                {
                    dequeued = _sendQueue.Dequeue();
                    BufferOutput(dequeued);
                    _receiveQueue.Enqueue(dequeued);
                } while (command != dequeued);
                FlushCommandBuffer();
            }
        }

        internal void ReadResultForCommand(RedisCommand command)
        {
            if (_sendQueue.Contains(command)) 
            {
                if (_sendAllOnRead)
                    SendAllCommands();
                else
                    SendCommand(command);
            }
            if (_receiveQueue.Contains(command)) 
            {
                BeginReadReplies();
            }
        }


        public void Clear() 
		{
			// Drain pending responses
            BeginReadReplies();
			// kill unsent commands 
			_sendQueue.Clear();
		}

        public void Flush()
        {
            SendAllCommands();
            ReadAllResults();
        }


        protected override void Release()
        {
            try
            {
                Flush();
            }
            finally 
            {
                DisposeSocket(_socket);
            }
        }


        static void DisposeSocket(IDisposable socket)
        {
            if (socket != null)
                socket.Dispose();
        }


        PooledSocket AcquireSocket()
        {
            var socket = Node.Acquire();
            if (socket == null)
                throw new RedisClientException("Unable to acquire socket for node : '" + Node.EndPoint + "'");
            return socket;
        }

        #region Utils

        static bool CommandNameIs(RedisCommand command, string name)
        {
            var cmdName = command.Arguments[0].Text;
            return cmdName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        static bool CommandNameIs(RedisCommand command, byte[] name)
        {
            var cmdName = command.Arguments[0];
            if (name.IsEqualTo(cmdName)) 
                return true;
            var temp = Encoding.UTF8.GetString(name);
            return CommandNameIs(command, temp);
        }

        static bool IsMulti(RedisCommand command)
        {
            return (command is MultiCommand) || CommandNameIs(command, Command.Multi); 
        }

        static bool IsExec(RedisCommand command)
        {
            return (command is ExecCommand) || CommandNameIs(command, Command.Exec);
        }

        static bool IsAuth(RedisCommand command)
        {
            return (command is AuthCommand) || CommandNameIs(command, Command.Auth);    
        }

        static bool IsSelect(RedisCommand command)
        {
            return (command is SelectCommand) || CommandNameIs(command, Command.Select);    
        }


        /// <summary>
        /// Command to set multiple binary safe arguments
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        void BufferOutput(RedisCommand command)
        {
            _commandBuffer.Append(command);
        }

        private void FlushCommandBuffer()
        {
            if (_commandBuffer.Size > 0)
            {
                CheckDisposed();
                var state = new ClientAsyncWriteState
                                {
                                    WorkSocket = Socket.Socket,
                                    Buffer = _commandBuffer.Data,
                                    CallbackArg = this
                                };
   
                state.WorkSocket.BeginSend(state.Buffer, 0, _commandBuffer.Size,
                    SocketFlags.None,
                    new AsyncCallback(EndSendCommandBuffer), state);

                _commandBuffer.Reset(); // important that this DOES NOT deallocate buffer
                _sendMre.WaitOne();
                _sendMre.Reset();
            }
        }

        static void EndSendCommandBuffer(IAsyncResult ar)
        {
            var state = (ClientAsyncWriteState)ar.AsyncState;
            var client = state.WorkSocket;
            client.EndSend(ar);
            var self = (RedisCommandQueue) state.CallbackArg;
            self._sendMre.Set();
        }


        private ClientAsyncReadState CreateAsyncReadState()
        {
            int expectedReplies = _receiveQueue.Count;
            var state = new ClientAsyncReadState
            {
                WorkSocket = Socket.Socket,
                ExpectedReplies = expectedReplies,
                CallbackArg = this
            };

            bool execExpected = false;
            state.ValueReceived += (stateObject, data, value) => HandleValueReceived(stateObject, data, value, ref execExpected);

            return state;
        }

        private static void HandleValueReceived(ClientAsyncReadState state, object data, RedisValue value, ref bool execExpected)
        {
            var self = (RedisCommandQueue)data;

            var dequeued = self._receiveQueue.Peek();

            if (value.Type == RedisValueType.Error)
            {
                if (IsAuth(dequeued))
                {
                    self._receiveQueue.Dequeue();
                    throw new RedisAuthenticationException(value.Text);
                }
                throw new RedisClientException(value.Text);
            }

            var socket = self.Socket;

            if (IsMulti(dequeued))
            {
                execExpected = true;
                // value here should be "OK"
                dequeued.Value = value;
                return;
            }

            if (IsExec(dequeued))
            {
                if (!execExpected)
                {
                    // throw
                    throw new RedisClientException("EXEC found without a matching MULTI");
                }
                execExpected = false;
                self._multiCount--;
            }
            else if (execExpected)
            {
                // value should be "QUEUED" for each command between multi and exec
                return;
            }

            self._receiveQueue.Dequeue();

            if (!socket.IsAuthorized && IsAuth(dequeued))
            {
                if (value.Text != "OK")
                    throw new RedisAuthenticationException("Invalid credentials for node : " + self.Socket);
                socket.IsAuthorized = true;
                dequeued.Value = value;
            }
            else if (socket.CurrentDb != self.CurrentDB && IsSelect(dequeued))
            {
                socket.CurrentDb = self.CurrentDB;
            }

            dequeued.Value = value;            
        }


        void BeginReadReplies()
        {
            if (_receiveQueue.Count > 0)
            {
                var state = CreateAsyncReadState();
                BeginReadReplies(state);
            }
        }

        static void BeginReadReplies(ClientAsyncReadState state)
        {
            var client = state.WorkSocket;

            client.BeginReceive(state.Buffer, 0, ClientAsyncReadState.BufferSize, SocketFlags.None,
              new AsyncCallback(EndReadReplies), state);

            var self = (RedisCommandQueue)state.CallbackArg;
            self._receiveMre.WaitOne();
            self._receiveMre.Reset();
        }

        static void EndReadReplies(IAsyncResult ar)
        {
            var state = (ClientAsyncReadState) ar.AsyncState;
            var self = (RedisCommandQueue)state.CallbackArg;
            SocketError errorCode;
            int bytesRead = state.WorkSocket.EndReceive(ar, out errorCode);
            
            if (errorCode != SocketError.Success)
            {      
                throw new RedisClientException("Socket error reading reply : " + Enum.GetName(typeof(SocketError), errorCode));
            }

            if (bytesRead > 0 && self._receiveQueue.Count > 0)
            {
                state.ReplyParser.Update(state.Buffer, 0, bytesRead);
                if (self._receiveQueue.Count > 0)
                    BeginReadReplies(state);
                else
                {
                    // TODO: Do we need to see if there is more data pending ?
                    self._receiveMre.Set();
                }
            } 
            else
            {
                self._receiveMre.Set();   
                if (self._receiveQueue.Count > 0)
                {
                    throw new RedisClientException(
                       String.Format("Invalid number of bulk responses. Expected {0}, Got {1}", 
                        state.ExpectedReplies, state.ReceivedReplies));
                }
                self._multiCount = 0;
            }
        }


        private void CheckDisposed()
        {
            if (Socket == null || Socket.Socket == null)
                throw new ObjectDisposedException("PooledSocket");
        }


        #endregion
    }
}
