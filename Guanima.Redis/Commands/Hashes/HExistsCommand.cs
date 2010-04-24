using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HExistsCommand : RedisCommand
    {
        public HExistsCommand(string key, string field) 
        {
            ValidateKey(key);
 
            SetParameters(key, field);
        }
    }
}