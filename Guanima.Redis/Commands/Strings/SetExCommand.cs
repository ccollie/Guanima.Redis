using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class SetExCommand : RedisCommand
    {
        public SetExCommand(string key, RedisValue value, TimeSpan timeout)
            : this(key, value, (long)timeout.TotalSeconds)
        {
        }

        public SetExCommand(string key, RedisValue value, DateTime expiresAt) 
            :this(key, value, expiresAt.ToUniversalTime() - DateTime.UtcNow)
        {
        }

        public SetExCommand(string key, RedisValue value, long timeoutInSeconds)
        {
            ValidateKey(key);
            SetParameters(Name, key, value, timeoutInSeconds);
        }
    }
}
