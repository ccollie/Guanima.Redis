using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZScoreCommand : KeyValueCommand
    {
        public ZScoreCommand(string key, RedisValue element) 
            : base(key, element)
        {
        }
    }
}
