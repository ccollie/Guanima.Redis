using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SInterCommand : RedisCommand
    {
        public SInterCommand(IEnumerable<String> keys) 
        {
            Arguments = CommandUtils.ConstructParameters(Name, keys);
        }
    }
}
