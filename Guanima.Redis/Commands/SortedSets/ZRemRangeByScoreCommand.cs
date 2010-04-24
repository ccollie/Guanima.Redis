using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRemRangeByScoreCommand : KeyCommand
    {
        public ZRemRangeByScoreCommand(string key, double start, double end) 
            : base(key, start, end)
        {
        }

    }
}