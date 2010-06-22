using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.PubSub
{
    [Serializable]
    public sealed class PUnsubscribeCommand : UnsubscribeCommand
    {
        public PUnsubscribeCommand(IEnumerable<string> patterns)
        {
            if (patterns == null)
                throw new ArgumentNullException("patterns");
            Arguments = CommandUtils.ConstructParameters(Name, patterns);
        }
    }
}
