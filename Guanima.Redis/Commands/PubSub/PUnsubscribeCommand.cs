using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public sealed class PUnsubscribeCommand : RedisCommand
    {
        public PUnsubscribeCommand(IEnumerable<string> patterns)
        {
            if (patterns == null)
                throw new ArgumentNullException("patterns");
            Elements = CommandUtils.ConstructParameters(Name, patterns);
        }
    }
}
