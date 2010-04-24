
namespace Guanima.Redis.Commands
{
    public abstract class KeyValueCommand : RedisCommand
    {
        protected KeyValueCommand(string key, RedisValue value)
        {
            ValidateKey(key);
            SetParameters(key, value);
        }
    }
}
