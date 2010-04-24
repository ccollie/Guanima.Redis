using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class RPushCommand : KeyValueCommand
    {
        public RPushCommand(String key, RedisValue value) :
            base(key, value)
        {
        }
     }
}
