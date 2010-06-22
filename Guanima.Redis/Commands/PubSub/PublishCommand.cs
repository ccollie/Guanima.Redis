using System;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public sealed class PublishCommand : RedisCommand
    {
        public PublishCommand(String channelName, RedisValue message) 
        {
            if (channelName == null) 
                throw new ArgumentNullException("channelName", "Class name must be specified.");
            if (String.IsNullOrEmpty(channelName))
                throw  new ArgumentException("Class name must have a value","channelName");

            SetParameters(channelName, message);
        }

        public override void ReadFrom(PooledSocket socket)
        {
            this.Value = socket.ExpectIntegerReply();
        }
    }
}