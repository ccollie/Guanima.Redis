using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.SortedSets;

namespace Guanima.Redis
{
    public partial class RedisClient 
    {

        public int ZAdd(string key, Double score, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZAddCommand(transformedKey, score, value));
        }

        public int ZCard(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZCardCommand(transformedKey));
        }

        public int ZRem(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZRemCommand(transformedKey, value));
        }

        public int ZRemRangeByRank(string key, int min, int max)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZRemRangeByRankCommand(transformedKey, min, max));
        }

        public int ZRemRangeByScore(string key, double min, double max)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZRemRangeByScoreCommand(transformedKey, min, max));
        }

        public double ZIncrBy(string key, double delta, RedisValue member)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new ZIncrByCommand(transformedKey, delta, member));
        }

        public RedisValue ZRange(string key, int start, int end)
        {
            return ZRange(key, start, end, false);
        }

        public RedisValue ZRange(string key, int start, int end, bool withScores)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new ZRangeCommand(transformedKey, start, end, withScores));
        }

        public RedisValue ZRevRange(string key, int start, int end, bool withScores)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new ZRangeCommand(transformedKey, start, end, withScores));
        }

        public RedisValue ZRangeByScore(string key, double min, double max)
        {
            return ZRangeByScore(key, min, max, false);
        }

        public RedisValue ZRangeByScore(string key, double min, double max, bool withScores)
        {
            var transformedKey = TransformKey(key);
            var cmd = new ZRangeByScoreCommand(transformedKey, min, max, withScores);
            return ExecValue(transformedKey, cmd);
        }

        public RedisValue ZRangeByScore(string key, double min, double max, int offset, int count)
        {
            return ZRangeByScore(key, min, max, offset, count, false);
        }

        public RedisValue ZRangeByScore(string key, double min, double max, int offset, int count, bool withScores)
        {
            var transformedKey = TransformKey(key);
            var cmd = new ZRangeByScoreCommand(transformedKey, min, max, offset, count, withScores);
            return ExecValue(transformedKey, cmd);
        }


        public int ZRank(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZRankCommand(transformedKey, value));
        }


        public int ZRevRank(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new ZRevRankCommand(transformedKey, value));
        }


        public double ZScore(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new ZScoreCommand(transformedKey, value));
        }

  
        public int ZInter(string destKey, IEnumerable<string> keys)
        {
            var transformedKey = TransformKey(destKey);
            var transformedKeys = TransformKeys(keys);

            EnsureNotClustered("ZINTER", transformedKey, transformedKeys);

            return ExecuteInt(transformedKey,
                new ZInterCommand(transformedKey, transformedKeys));
        }

        public int ZInter(string destKey, IEnumerable<string> keys, IEnumerable<double> weights, AggregateType aggregateType)
        {
            var transformedKey = TransformKey(destKey);
            var transformedKeys = TransformKeys(keys);

            EnsureNotClustered("ZINTER", transformedKey, transformedKeys);

            return ExecuteInt(transformedKey,
                new ZInterCommand(transformedKey, transformedKeys, weights, aggregateType));
        }

        public int ZUnion(string destKey, IEnumerable<string> keys)
        {
            var transformedKey = TransformKey(destKey);
            var transformedKeys = TransformKeys(keys);

            EnsureNotClustered("ZUNION", transformedKey, transformedKeys);

            return ExecuteInt(transformedKey, new ZUnionCommand(transformedKey, transformedKeys));
        }
        
        public int ZUnion(string destKey, IEnumerable<string> keys, IEnumerable<double> weights, AggregateType aggregateType)
        {
            var transformedKey = TransformKey(destKey);
            var transformedKeys = TransformKeys(keys);

            EnsureNotClustered("ZUNION", transformedKey, transformedKeys);

            return ExecuteInt(destKey,
                new ZUnionCommand(transformedKey, transformedKeys, weights, aggregateType));
        }
    }
}