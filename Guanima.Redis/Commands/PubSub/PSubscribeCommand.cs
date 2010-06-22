using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public sealed class PSubscribeCommand : SubscribeCommand
    {
        public PSubscribeCommand(IEnumerable<string> patterns)
        {
            if (patterns == null)
                throw new ArgumentNullException("patterns");
            if (patterns.Count() == 0)
                throw new ArgumentException("At least 1 channel must be specified", "patterns");
            Arguments = CommandUtils.ConstructParameters(Name, patterns);
        }
    }
}
