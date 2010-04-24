using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public sealed class SubscribeCommand : RedisCommand
    {
        public SubscribeCommand(IEnumerable<string> channels)
        {
            if (channels == null)
                throw new ArgumentNullException("channels");
            if (channels.Count() == 0)
                throw new ArgumentException("At least 1 channel must be specified", "channels");
            Elements = CommandUtils.ConstructParameters(Name, channels);
        }
    }
}
