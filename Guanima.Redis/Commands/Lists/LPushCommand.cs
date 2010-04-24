using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LPushCommand : KeyValueCommand
    {
        public LPushCommand(String key, RedisValue value) :
            base(key, value)
        {
        }
    }
}
