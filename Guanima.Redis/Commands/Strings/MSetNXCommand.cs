using System.Collections.Generic;
using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class MSetNXCommand : RedisCommand
    {
        public MSetNXCommand(ICollection<KeyValuePair<string, RedisValue>> values)
        {
            Arguments = CommandUtils.ConstructParameters(Name, values);
        }
    }
}
