using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.Hashes;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        #region Hashes

        public bool HDel(string key, string field)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new HDelCommand(transformedKey, field));
        }

        public bool HExists(string key, string field)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new HExistsCommand(transformedKey, field));
        }

        public RedisValue HGet(string key, string field)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new HGetCommand(transformedKey, field));
        }

        public long HIncrBy(string key, string field, int delta)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new HIncrByCommand(transformedKey, field, delta));
        }

        public Dictionary<string, RedisValue> HGetAll(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new HGetAllCommand(transformedKey));
        }

        public long HLen(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new HLenCommand(transformedKey));
        }


        public bool HSet(string key, string field, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new HSetCommand(transformedKey, field, value));
        }

        public bool HSetNx(string key, string field, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new HSetNXCommand(transformedKey, field, value));
        }


        public RedisValue[] HMGet(string key, params string[] fields)
        {
            return HMGet(key, (IEnumerable<String>) fields);
        }

        public RedisValue[] HMGet(string key, IEnumerable<String> fields)
        {
            var transformedKey = TransformKey(key);
            var val = ExecValue(transformedKey, new HMGetCommand(transformedKey, fields));
            return val.MultiBulkValues;
        }

        public RedisClient HMSet(string key, ICollection<KeyValuePair<string, RedisValue>> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            var transformedKey = TransformKey(key);
            Execute(transformedKey, new HMSetCommand(transformedKey, values));
            return this;
        }

        public IList<String> HKeys(string key)
        {
            var transformedKey = TransformKey(key);
            var items = ExecValue(transformedKey, new HKeysCommand(transformedKey));
            var result = new List<string>();
            foreach (var item in items)
            {
                result.Add(item);
            }
            return result;
        }

        public RedisValue[] HVals(string key)
        {
            var transformedKey = TransformKey(key);
            var vals = ExecValue(transformedKey, new HValsCommand(transformedKey));
            return vals.MultiBulkValues;
        }

        #endregion
    }
}
