using System;
using Guanima.Redis.Extensions;
using Guanima.Redis.Utils;

namespace Guanima.Redis
{

    public class RedisLockToken : Disposable
    {
        private readonly RedisClient _client;
        private long _unixExpiration;
        private bool _released;

        internal RedisLockToken(RedisClient client, string key)
        {
            Key = key;
            Expiration = null;
            _client = client;
        }

        internal RedisLockToken(RedisClient client, string key, DateTime expiration)
        {
            Key = key;
            Expiration = expiration;
            _client = client;
        }

        public DateTime? Expiration { get; private set; }

        public long UnixExpiration
        {
            get
            {
                if (_unixExpiration == 0)
                {
                    _unixExpiration = Expiration.HasValue ? Expiration.Value.ToUnixTimestamp() : (DateTimeExtensions.UnixTimeNow + (60 * 60 * 24 * 365));
                }
                return _unixExpiration;
            }
        }

        public string Key { get; private set; }

        public void ReleaseLock()
        {
            if (_released) return;
            // We need to be careful when cleaning up the lock key.  If we took a really long
            // time for some reason, and the lock expired, someone else may have it, and
            // it's not safe for us to remove it.  Check how much time has passed since we
            // wrote the lock key and only delete it if it hasn't expired (or we're not using
            // lock expiration)
            if (Expiration == null || UnixExpiration > DateTimeExtensions.UnixTimeNow)
                _client.Del(Key);
            _released = true;
        }

        protected override void Release()
        {
            ReleaseLock();
        }
    }
}