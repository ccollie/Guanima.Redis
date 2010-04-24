using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HMSetCommand : RedisCommand
    {
        public HMSetCommand(string key, ICollection<KeyValuePair<string, RedisValue>> values)
        {
            ValidateKey(key);
            if (values == null)
                throw new ArgumentNullException("values");

            var parms = new RedisValue[(values.Count * 2) + 2];
            parms[0] = Name;
            parms[1] = key;
            int i = 2;
            foreach (var kvp in values)
            {
                if (String.IsNullOrEmpty(kvp.Key))
                    throw new ArgumentException("Hash Field Key cannot be null or empty");
                parms[i++] = kvp.Key;
                parms[i++] = kvp.Value;
            }
            Elements = parms;
        }
    }
}