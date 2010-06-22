using System;
using System.Collections.Generic;
using System.Linq;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public class SubscribeCommand : RedisCommand
    {
        private readonly int _channelCount;
        
        protected SubscribeCommand()
        {
        }

        public SubscribeCommand(IEnumerable<string> channels)
        {
            if (channels == null)
                throw new ArgumentNullException("channels");
            _channelCount = channels.Count();
            if (_channelCount == 0)
                throw new ArgumentException("At least 1 channel must be specified", "channels");
            Arguments = CommandUtils.ConstructParameters(Name, channels);
        }

        public override void ReadFrom(PooledSocket socket)
        {
            var values = new List<RedisValue>();
            int subscriptionCount = 0;
            for (var k = 0; k < _channelCount; k++)
            {
                string action = "", channel = "";
                socket.ParseSubscriptionResponse(ref action, ref channel, ref subscriptionCount);
                values.Add( channel );
            }
            values.Add( subscriptionCount );
            Value = new RedisValue()
                         {
                             MultiBulkValues = values.ToArray()
                         };
        }
    }
}
