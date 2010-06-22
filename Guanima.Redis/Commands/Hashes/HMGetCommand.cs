using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HMGetCommand : RedisCommand
    {
        public HMGetCommand(string key, IEnumerable<string> fields)
        {
            Arguments = CommandUtils.ConstructParameters(Name, key, fields);
        }
    }
}