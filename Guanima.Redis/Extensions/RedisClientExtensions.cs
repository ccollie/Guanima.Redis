using System;
using System.Threading;

namespace Guanima.Redis.Extensions
{
    public static class RedisClientExtensions
    {

        // Acquire a lock. Any other code that needs the lock
        // (on any server) will spin waiting for the lock up to timeoutInSeconds
        public static void AcquireLock(this RedisClient client, string key, int timeoutInSeconds, TimeSpan? expires)
        {
            long? expirationInSeconds = (expires == null) ? (long?)null : (long)expires.Value.TotalSeconds;
            var start = DateTimeExtensions.UnixTimeNow;
            bool gotIt = false;
            while (DateTimeExtensions.UnixTimeNow - start < timeoutInSeconds)
            {
                var unixExpiration = GenerateExpiration(expirationInSeconds);
                // Use the expiration as the value of the lock.
                gotIt = client.SetNX(key, unixExpiration);
                if (gotIt)
                    break;

                long oldExpiration = 0;
                // Lock is being held.  Now check to see if it's expired (if we're using
                // lock expiration).
                // See "Handling Deadlocks" section on http://code.google.com/p/redis/wiki/SetnxCommand
                if (expires != null)
                    oldExpiration = (long)client.Get(key);

                if (oldExpiration < DateTimeExtensions.UnixTimeNow)
                {
                    // If it's expired, use GETSET to update it.
                    unixExpiration = GenerateExpiration(expirationInSeconds);
                    oldExpiration = client.GetSet(key, unixExpiration);
               
                    // Since GETSET returns the old value of the lock, if the old expiration
                    // is still in the past, we know no one else has expired the lock
                    // and we now have it.
                    if (oldExpiration < DateTimeExtensions.UnixTimeNow)
                    {
                        gotIt = true;
                        break;
                    }
                }
                Thread.Sleep(500);  // TODO: Make this configurable
            }
            if (!gotIt)
            {
                throw new RedisLockTimeoutException(String.Format("Timeout on lock {0} exceeded {1} sec", key, timeoutInSeconds));
            }
        }

        public static void ReleaseLock(this RedisClient client, string key, long? expiration)
        {
            // We need to be careful when cleaning up the lock key.  If we took a really long
            // time for some reason, and the lock expired, someone else may have it, and
            // it's not safe for us to remove it.  Check how much time has passed since we
            // wrote the lock key and only delete it if it hasn't expired (or we're not using
            // lock expiration)
            if (expiration == null || expiration.Value > DateTimeExtensions.UnixTimeNow)
                client.Del(key);
        }

        static long GenerateExpiration(long? expiration)
        {
            // TODO: Make exipation on null value configurable
            long value = (expiration.HasValue ? expiration.Value : (long) 5.Days().TotalSeconds);
            return (DateTimeExtensions.UnixTimeNow + value + 1);
        }
    }
}
