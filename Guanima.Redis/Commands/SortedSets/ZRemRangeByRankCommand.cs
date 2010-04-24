using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRemRangeByRankCommand : KeyCommand
    {
        public ZRemRangeByRankCommand(string key, int start, int end) 
            : base(key, start, end)
        {
        }
    }
}