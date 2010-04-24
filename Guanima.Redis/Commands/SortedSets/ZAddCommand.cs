using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZAddCommand : KeyCommand
    {
        public ZAddCommand(string key, double score, RedisValue value) 
            : base(key, score, value)
        {
        }
    }
}
