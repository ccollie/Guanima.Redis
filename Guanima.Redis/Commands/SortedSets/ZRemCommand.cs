using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRemCommand : KeyValueCommand
    {
        public ZRemCommand(string key, RedisValue member) 
            : base(key, member)
        {
        }
    }
}
