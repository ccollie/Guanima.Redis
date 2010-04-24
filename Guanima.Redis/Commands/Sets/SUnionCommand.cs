using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SUnionCommand : RedisCommand
    {
        public SUnionCommand(IEnumerable<String> keys) 
        {
            Elements = CommandUtils.ConstructParameters(Name, keys);
        }
    }
}
