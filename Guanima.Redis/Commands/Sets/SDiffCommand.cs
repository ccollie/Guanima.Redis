using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SDiffCommand : RedisCommand
    {
        public SDiffCommand(IEnumerable<String> keys) 
        {
            Arguments = CommandUtils.ConstructParameters(Name, keys);
        }
    }
}
