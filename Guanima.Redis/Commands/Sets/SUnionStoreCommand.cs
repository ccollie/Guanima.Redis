using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SUnionStoreCommand : RedisCommand
    {
        public SUnionStoreCommand(string dstKey, IEnumerable<String> keys) 
        {
            ValidateKey(dstKey);
            Arguments = CommandUtils.ConstructParameters(Name, dstKey, keys);
        }
    }
}
