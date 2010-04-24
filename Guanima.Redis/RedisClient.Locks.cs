using System;
using Guanima.Redis.Extensions;

namespace Guanima.Redis
{
    public partial class RedisClient
    {

        public static DateTime GetLockDate(RedisValue value)
        {
            if (value.IsEmpty)
                return DateTime.MinValue;
            return ((long)value).AsDateTime();
        }

        private const int SecondsPerDay = (24*60*60);
        private const int SecondPerYear = 365*SecondsPerDay;
     
        private static int GetTimeoutSeconds(TimeSpan timeout)
        {
            // todo: change.. possible overflow
            int timeoutInSeconds = (int)timeout.TotalSeconds;
            // Edge case with TimeSpan.MaxValue
            if (timeoutInSeconds == int.MinValue || timeout == Infinite)
            {
                timeoutInSeconds = int.MaxValue;
            }
            return timeoutInSeconds;
        }

        public IDisposable Lock(string key)
        {
            this.AcquireLock(key, (int)int.MaxValue, null);
            return new RedisLockToken(this, key);
        }

        public IDisposable Lock(string key, TimeSpan timeout)
        {
            this.AcquireLock(key, GetTimeoutSeconds(timeout) , null);
            return new RedisLockToken(this, key);
        }

        public IDisposable Lock(string key, TimeSpan timeout, TimeSpan expiration)
        {
            this.AcquireLock(key, GetTimeoutSeconds(timeout), expiration);
                // TODO: Testable datetime provider
            var newExpiration = DateTime.UtcNow + expiration;
            return new RedisLockToken(this, key, newExpiration);
        }


        public static bool IsExpiredLockValue(RedisValue lockValue, DateTime currentTime)
        {
            if (lockValue.IsEmpty)
                return false;
            var timeNow = DateTimeExtensions.UnixTimeNow;
            return (timeNow >= lockValue);
        }


    }
}
