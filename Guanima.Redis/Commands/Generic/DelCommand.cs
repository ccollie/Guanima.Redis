using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class DelCommand : RedisCommand
    {
        public DelCommand(IEnumerable<string> keys)
        {
            Arguments = CommandUtils.ConstructParameters(Command.Del,keys);
        }

        public DelCommand(string key) 
            : this(new string[]{key})
        {            
        }
    }
}
