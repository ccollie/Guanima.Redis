using System;
using System.Net.Sockets;
using Guanima.Redis.Commands;
using Guanima.Redis.Protocol;

namespace Guanima.Redis
{
    public class ClientAsyncState
    {
        private PooledSocket Socket;
        public IRedisNode Node;
        // Client socket.
        public Socket WorkSocket = null;
    }

    public class ClientAsyncWriteState : ClientAsyncState
    {
        // Size of receive buffer.
        public const int BufferSize = 256;

        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
 
        public delegate void CommandSentHandler(object args, RedisCommand command);

        // An event that clients can use to be notified whenever a
        // value is retrieved from the socket.
        public event CommandSentHandler CommandSent;
        
        public object CallbackArg { get; set; }

        internal void OnCommandWritten(RedisCommand command)
        {
            if (CommandSent != null)
                CommandSent.Invoke(CallbackArg, command);
        }
    }


    public class ClientAsyncReadState : ClientAsyncState
    {
        private RedisReplyParser _parser;
        private int _expectedReplies = 1;
        private int _receivedReplies;

        public object CallbackArg { get; set; }

        // Size of receive buffer.
        public const int BufferSize = 1024;
            
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
            
        public delegate void ValueReceivedHandler(ClientAsyncReadState state, object args, RedisValue value);

        // An event that clients can use to be notified whenever a
        // value is retrieved from the socket.
        public event ValueReceivedHandler ValueReceived;

        public int ExpectedReplies
        {
            get { return _expectedReplies; }
            set { _expectedReplies = value; }
        }

        public int ReceivedReplies
        {
            get { return _receivedReplies; }
        }

        public bool IsComplete
        {
            get { return ReceivedReplies == ExpectedReplies; }
        }

        public RedisReplyParser ReplyParser
        {
            get
            {
                return _parser ?? (_parser = new RedisReplyParser(ReplyReceived, CallbackArg));    
            }
            set
            {
                _parser = value;
            }
        }

        private void ReplyReceived(object data, RedisValue value)
        {
            _receivedReplies++;
            if (ValueReceived != null)
                ValueReceived.Invoke(this, data, value);
        }
    }
}