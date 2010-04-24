namespace Guanima.Redis.Commands
{
    public abstract class KeyCommand : RedisCommand
    {
        protected KeyCommand(string key, params RedisValue[] values)
        {
            ValidateKey(key);
            var vals = new RedisValue[1 + values.Length];
            vals[0] = key;
            if (values.Length > 0)
                values.CopyTo(vals, 1);

            SetParameters(vals);
        }

        protected KeyCommand(string key)
        {
            ValidateKey(key);
            SetParameters(key);
        }
    }
}
