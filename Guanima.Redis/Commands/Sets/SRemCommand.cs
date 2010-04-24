using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SRemCommand : KeyValueCommand
    {
        public SRemCommand(String key, RedisValue value) :
            base(key, value)
        {
        }
     }
}