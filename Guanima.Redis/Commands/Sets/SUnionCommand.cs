using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SUnionCommand : RedisCommand
    {
        public SUnionCommand(IEnumerable<String> keys) 
        {
            Arguments = CommandUtils.ConstructParameters(Name, keys);
        }
    }
}
