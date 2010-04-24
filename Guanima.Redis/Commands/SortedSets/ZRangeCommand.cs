using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRangeCommand : RedisCommand
    {
        public ZRangeCommand(string key, int start, int end)
            : this(key, start, end, false)
        {
        }

        public ZRangeCommand(string key, int start, int end, bool withScores)
        {
            if (key == null)
                throw new ArgumentNullException();

            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Empty key value");
            if (withScores)
                SetParameters(key, start, end, "WITHSCORES");
            else
            {
                SetParameters(key, start, end);
            }
        }

    }
}