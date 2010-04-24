using System;

namespace Guanima.Redis
{
    public enum RedisReplyType
    {
        Status,
        String,
        Int,
        Bulk,
        Float,
        MultiBulk,
        Dictionary,
        Error,
        Object,
        List
    }

    public interface IRedisReply
    {
        object Value { get;} 
        RedisReplyType ReplyType { get; }
    }

    public interface IRedisReply<T> : IRedisReply
    {
        new T Value { get; }
    }

    public class RedisReply<T> : IRedisReply<T>
    {
        public T Value { get; protected set; }

        object IRedisReply.Value
        {
            get { return this.Value; }
        }

        public virtual RedisReplyType ReplyType
        {
            get; private set;
        }

        public RedisReply(T value, RedisReplyType replyType)
        {
            this.Value = value;
            ReplyType = replyType;
        }

        public static IRedisReply<ReplyT> Create<ReplyT>(ReplyT value, RedisReplyType replyType)
        {
            return new RedisReply<ReplyT>(value, replyType);
        }
    }

  
    [Serializable]
    public class ErrorReply : RedisReply<String>
    {

        public ErrorReply(string message) 
            : base(message, RedisReplyType.Error)
        {}

        public String Message
        {
            get { return Value; }
        }
    }
  
 
 
 
}
