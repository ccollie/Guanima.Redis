using System;
using Guanima.Redis.Extensions;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class ExpireAtCommand : RedisCommand
    {
        public ExpireAtCommand(String key, DateTime expiration) :
            this(key, expiration.ToUnixTimestamp())
        {
        }

        public ExpireAtCommand(String key, long unixTime) 
        {
            ValidateKey(key);
            if (unixTime < DateTimeExtensions.UnixEpoch)
                throw new ArgumentOutOfRangeException("unixTime", "expiresAt must be >= 1970/1/1");
            SetParameters(key, unixTime);
        }
    }
}
