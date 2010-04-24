using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LSetCommand : KeyCommand
    {
        public LSetCommand(string key, int index, RedisValue value)
            : base(key, index, value)
        {
        }
    }
}
