using System.Collections.Generic;
using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class MSetNXCommand : RedisCommand
    {
        public MSetNXCommand(ICollection<KeyValuePair<string, RedisValue>> values)
        {
            Elements = CommandUtils.ConstructParameters(Name, values);
        }
    }
}
