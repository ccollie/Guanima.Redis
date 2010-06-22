using System.Collections.Generic;

namespace Guanima.Redis.Commands.SortedSets
{
    public abstract class BaseZRangeByScoreCommand : RedisCommand
    {
        protected BaseZRangeByScoreCommand(string key, double min, double max)
            :this(key, min, max, false)
        {
        }

        protected BaseZRangeByScoreCommand(string key, double min, double max, int offset, int count)
            : this(key, min, max, offset, count, null)
        {
        }

        protected BaseZRangeByScoreCommand(string key, double start, double end, bool withScores) 
            : this(key, start, end, null, null, withScores)
        {
        }

        protected BaseZRangeByScoreCommand(string key, double min, double max, int? offset, int? count, bool? withScores)
        {
            ValidateKey(key);
            var parms = new List<RedisValue>();
            parms.Add(Name);
            parms.Add(key);
            parms.Add(min);
            parms.Add(max);
         
            if (offset.HasValue || count.HasValue)
            {
                var offs = (offset.HasValue ? offset.Value : 0);
                parms.Add("LIMIT");
                parms.Add(offs);
                parms.Add( (count.HasValue ? count.Value : 1) );
            }
            if (withScores.HasValue && withScores.Value)
            {
                parms.Add("WITHSCORES");
            }
            Arguments = parms.ToArray();
        }

    }

    public class ZRangeByScoreCommand :BaseZRangeByScoreCommand
    {
        public ZRangeByScoreCommand(string key, double min, double max) : base(key, min, max)
        {
        }

        public ZRangeByScoreCommand(string key, double min, double max, int offset, int count) : base(key, min, max, offset, count)
        {
        }

        public ZRangeByScoreCommand(string key, double start, double end, bool withScores) : base(key, start, end, withScores)
        {
        }

        public ZRangeByScoreCommand(string key, double min, double max, int offset, int count, bool withScores) : base(key, min, max, offset, count, withScores)
        {
        }

    }
}