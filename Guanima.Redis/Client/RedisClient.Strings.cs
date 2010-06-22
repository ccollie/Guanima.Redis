using System;
using System.Linq;
using System.Collections.Generic;
using Guanima.Redis.Commands;
using Guanima.Redis.Commands.Generic;
using Guanima.Redis.Commands.Strings;
using Guanima.Redis.Commands.Transactions;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        #region Strings

        public RedisValue this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public int Decr(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new DecrCommand(transformedKey));
        }

        public int Decr(string key, int delta)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new DecrByCommand(transformedKey, delta));
        }

        public RedisValue Get(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new GetCommand(transformedKey));
        }

        public IList<RedisValue> Get(IEnumerable<string> keys)
        {
            return MGet(keys);
        }

        public string GetString(String key)
        {
            return ToString(Get(key));
        }

        public RedisValue GetSet(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new GetSetCommand(transformedKey, value));
        }


        public IList<String> GetKeys(String pattern)
        {
            var dict = new Dictionary<string, bool>();
            var result = new List<string>();
            foreach (var server in _serverPool.GetServers())
            {
                if (!server.IsAlive)
                    continue;

                var temp = ExecValue(server, new KeysCommand(pattern));
                foreach (var s in temp)
                {
                    if (!dict.ContainsKey(s))
                    {
                        dict[s] = true;
                        result.Add(s);
                    }
                }        
            }
            return result;
        }

        public long Incr(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new IncrCommand(transformedKey));
        }

        public long Incr(string key, long delta)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new IncrByCommand(transformedKey, delta));
        }

        public IList<RedisValue> MGet(params string[] keys)
        {
            IEnumerable<string> keyList = new List<string>(keys);
            return MGet(keyList);
        }



        public IList<RedisValue> MGet(IEnumerable<string> keys)
        {
            var result = new Dictionary<String, RedisValue>();

            var realToHashed = new Dictionary<string, string>();
            var hashedToReal = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                var hashed = TransformKey(key);
                hashedToReal[hashed] = key;
                realToHashed[key] = hashed;
            }
           
            // map each key to the appropriate server in the pool
            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(realToHashed.Values);

            // send a 'MGet' to each server
            foreach (var de in splitKeys)
            {
                var node = de.Key;
                var command = new MGetCommand(de.Value);

                RedisValue items = ExecValue(node, command);
                if (!items.IsEmpty)
                {
                    int index = 0;
                    foreach(var item in items)
                    {
                        var transformedKey = de.Value[index++];
                        var originalKey = hashedToReal[transformedKey];
                        result[originalKey] = item;
                    }
                }
            }

            var values = new List<RedisValue>();
            foreach(var key in keys)
            {
                RedisValue val;
                if (!result.TryGetValue(key, out val))
                {
                    values.Add(RedisValue.Empty);
                } 
                else
                {
                    values.Add(val);
                }
            }
            return values;
        }


        public RedisClient Set(string key, RedisValue value)
        {
            return Set(key, value, null, null);
        }

        public RedisClient Set(string key, RedisValue value, TimeSpan expiration)
        {
            return Set(key, value, expiration, null);
        }

        public RedisClient Set(string key, RedisValue value, DateTime expiresAt)
        {
            return Set(key, value, null, expiresAt);
        }


        private RedisClient Set(string key, RedisValue value, TimeSpan? expiresIn, DateTime? expiresAt)
        {
            var transformedKey = TransformKey(key);
            RedisCommand command;

            if (expiresAt != null || expiresIn != null)
            {
                if (expiresAt != null)
                {
                    command = new SetExCommand(transformedKey, value, expiresAt.Value);
                }
                else
                {
                    command = new SetExCommand(transformedKey, value, expiresIn.Value);
                }
            } 
            else
            {
                command = new SetCommand(transformedKey, value);
            }
            Execute(transformedKey, command);
            return this;
        }


        public bool SetNX(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new SetNXCommand(transformedKey, value));
        }

        #region MSet

        public RedisClient MSet(IDictionary<String,RedisValue> values)
        {
            return MSetInternal(values, false);    
        }

        public RedisClient MSet(IDictionary<String, string> values)
        {
            return MSetInternal(values, false);
        }

        public RedisClient MSetNX(IDictionary<String, RedisValue> values)
        {
            return MSetInternal(values, true);
        }

        public RedisClient MSetNX(IDictionary<String, string> values)
        {
            return MSetInternal(values, true);
        }

        protected RedisClient MSetInternal(IDictionary<string,string> values, bool checkNotExists)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            
            var convertedValues = values.ToDictionary(k => k.Key, v => (RedisValue)(v.Value));
            return MSetInternal(convertedValues, checkNotExists);
        }

        protected RedisClient MSetInternal(IDictionary<string, RedisValue> values, bool checkNotExists)
        {
            if (values == null)
                throw new ArgumentNullException("values");
                 
            var realToHashed = new Dictionary<string, string>();
            var hashedToReal = new Dictionary<string, string>();

            foreach (var key in values.Keys)
            {
                var hashed = TransformKey(key);
                hashedToReal[hashed] = key;
                realToHashed[key] = hashed;
            }

            // map each key to the appropriate server in the pool based on transformed key
            IDictionary<IRedisNode, List<string>> nodeKeys = SplitKeys(realToHashed.Values);

            if (nodeKeys.Count > 0)
            {
                CannotCluster( (checkNotExists) ? "MSETNX" : "MSET" );
            }

            // send a 'MSet' to each server
            foreach (var nodeKeyItem in nodeKeys)
            {
                var destNode = nodeKeyItem.Key;
                var destKeys = nodeKeyItem.Value;

                // collect the list of items for this node
                var itemsForNode = new Dictionary<string, RedisValue>(destKeys.Count);
                foreach (var destKey in destKeys)
                {
                    // we have to map from transformed key to original value
                    var originalKey = hashedToReal[destKey];
                    itemsForNode[destKey] = values[originalKey];
                }
                if (checkNotExists)
                {
                    Execute(destNode, new MSetNXCommand(itemsForNode));
                } 
                else
                {
                    Execute(destNode, new MSetCommand(itemsForNode));                    
                }
            }

            return this;
        }

        #endregion

        public int Append(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new AppendCommand(transformedKey, value));            
        }

        public RedisValue Substr(string key, int start, int end)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new SubstrCommand(transformedKey, start, end));
        }
        #endregion
    }
}
