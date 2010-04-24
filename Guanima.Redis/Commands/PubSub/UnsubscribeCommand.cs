using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.PubSub
{
    public class UnsubscribeCommand : RedisCommand
    {
        public UnsubscribeCommand(IEnumerable<string> channels)
        {
            if (channels == null)
                throw new ArgumentNullException("channels");
            Elements = CommandUtils.ConstructParameters(Name, channels);
        }
    }
}
