using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRevRankCommand : KeyValueCommand
    {
        public ZRevRankCommand(string key, RedisValue value) 
            : base(key, value)
        {
        }
    }
}
