namespace Guanima.Redis
{
    public partial class RedisClient
    {
        public object GetObject(string key)
        {
            var obj = Get(key);
            if (obj.IsEmpty)
                return null;
            return _serverPool.Transcoder.Deserialize(obj);
        }

        public T Get<T>(string key)
        {
            var obj = Get(key);
            // TODO: throw if T is a value type ?
            if (obj.IsEmpty)
                return default(T);
            return (T) _serverPool.Transcoder.Deserialize(obj);
        }

        public RedisClient Save<T>(string key, T value)
        {
            var item = _serverPool.Transcoder.Serialize(value);
            return Set(key, (RedisValue)item); // cast is necessary to avoid recursion
        }
    }
}
