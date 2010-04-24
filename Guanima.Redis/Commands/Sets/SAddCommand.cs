using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public class SAddCommand : KeyValueCommand
    {
        public SAddCommand(String key, RedisValue value) :
            base(key, value)
        {
        }
    }
}