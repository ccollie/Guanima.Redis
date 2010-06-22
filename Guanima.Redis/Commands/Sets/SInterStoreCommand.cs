using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SInterStoreCommand : RedisCommand
    {
        public SInterStoreCommand(string dstKey, IEnumerable<String> keys) 
        {
            ValidateKey(dstKey);
            Arguments = CommandUtils.ConstructParameters(Name, dstKey, keys);
        }
    }
}
