using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LRemCommand : KeyCommand
    {
        public LRemCommand(String key, int count, RedisValue value) :
            base(key, count, value)
        {
            // todo: do we need to validate ?
        }
    }
}
