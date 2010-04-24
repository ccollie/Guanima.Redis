using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public class ZRankCommand : KeyValueCommand
    {
        public ZRankCommand(string key, RedisValue value) 
            : base(key, value)
        {
        }
    }
}
