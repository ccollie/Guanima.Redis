using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZIncrByCommand : KeyCommand
    {
        public ZIncrByCommand(string key, double delta, RedisValue value) 
            : base(key, delta, value)
        {
        }
    }
}
