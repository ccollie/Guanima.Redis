using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class GetSetCommand : KeyValueCommand
    {
        public GetSetCommand(string key, RedisValue value) : 
            base(key, value)
        {
        }
    }
}
