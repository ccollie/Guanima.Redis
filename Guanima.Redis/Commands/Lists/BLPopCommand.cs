using System;
using System.Collections.Generic;
using System.Linq;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class BLPopCommand : RedisCommand
    {
        public BLPopCommand(IEnumerable<string> keys, int timeoutInSeconds)
        {
            int count = keys.Count();
            var parameters = new RedisValue[count + 2];
            parameters[0] = Name;
            int i = 0;
            foreach (var key in keys)
            {
                if (String.IsNullOrEmpty(key))
                    throw new ArgumentException("Key must not be null or empty");
                parameters[i++] = key;
            }
            parameters[i] = timeoutInSeconds;
            Arguments = parameters;
        }

        public BLPopCommand(IEnumerable<string> keys, TimeSpan timeout) :
            this(keys, (int)timeout.TotalSeconds)
        {
        }

        public BLPopCommand(string key, TimeSpan timeout) :
            this(new string[] { key }, timeout)
        {
        }
    }
}