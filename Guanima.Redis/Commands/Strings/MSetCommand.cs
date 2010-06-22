using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class MSetCommand : RedisCommand
    {
        public MSetCommand(ICollection<KeyValuePair<string, RedisValue>> values)
        {
            Arguments = CommandUtils.ConstructParameters(Name, values);
        }
    }
}
