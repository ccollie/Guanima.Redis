using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanima.Redis.Commands
{
    public static class CommandUtils
    {
        public static RedisValue[] ConstructParameters(IEnumerable<string> keys)
        {
            var parameters = new RedisValue[keys.Count()];
            int i = 0;
            foreach (var key in keys)
            {
                if (key == null)
                    throw new ArgumentNullException("keys", "Null value found for key");
                parameters[i++] = key;
            }
            return parameters;
        }


        public static RedisValue[] ConstructParameters(string name, IEnumerable<string> keys)
        {
            return ConstructParameters(name, null, keys);
        }

        public static RedisValue[] ConstructParameters(byte[] name, IEnumerable<string> keys)
        {
            return ConstructParameters(name, null, keys);
        }

        public static RedisValue[] ConstructParameters(byte[] name, string dstKey, IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            var count = keys.Count();
            if (count == 0)
                throw new ArgumentException("At least 1 source key must be specified.");

            bool hasDest = dstKey != null;
            var parameters = new RedisValue[count + (hasDest ? 2 : 1)];
            int i = 1;
            parameters[0] = name;
            if (hasDest)
            {
                parameters[1] = dstKey;
                i++;
            }
            foreach (var key in keys)
            {
                if (key == null)
                    throw new ArgumentNullException("keys", "Null value found for key");
                parameters[i++] = key;
            }
            return parameters;
        }

        public static RedisValue[] ConstructParameters(string name, string dstKey, IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            var count = keys.Count();
            if (count == 0)
                throw new ArgumentException("At least 1 source key must be specified.");

            bool hasDest = dstKey != null; 
            var parameters = new RedisValue[count + (hasDest ? 2 : 1)];
            int i = 1;
            parameters[0] = name;
            if (hasDest)
            {
                parameters[1] = dstKey;
                i++;
            }
            foreach (var key in keys)
            {
                if (key == null)
                    throw new ArgumentNullException("keys", "Null value found for key");
                parameters[i++] = key;
            }
            return parameters;
        }

        public static RedisValue[] ConstructParameters(ICollection<KeyValuePair<string, RedisValue>> values)
        {
            return ConstructParameters(null, values);
        }

        public static RedisValue[] ConstructParameters(string name, ICollection<KeyValuePair<string, RedisValue>> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Count == 0)
                throw new ArgumentException("At least one value expected", "values");
            
            var parameters = new RedisValue[values.Count * 2 + ((name != null) ? 1 : 0)];
            
            int i = 0;
            if (name != null)
            {
                parameters[0] = name;
                i = 1;
            }
            
            foreach (var kvp in values)
            {
                if (kvp.Key == null)
                    throw new ArgumentNullException("values", "Null value found for key");
                parameters[i++] = kvp.Key;
                parameters[i++] = kvp.Value;
            }
            return parameters;
        }       

    }
}
