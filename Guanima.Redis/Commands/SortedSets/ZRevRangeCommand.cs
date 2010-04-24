using System;
namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZRevRangeCommand : RedisCommand
    {
        public ZRevRangeCommand(string key, int start, int end)
            :this(key, start, end, false)
        {
        }

        public ZRevRangeCommand(string key, int start, int end, bool withScores) 
        {
            if (key == null)
                throw new ArgumentNullException();

            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Empty key value");
            if (withScores)
                SetParameters(start, end, "WITHSCORES");
            else
            {
                SetParameters(start, end);
            }
        }
    }
}