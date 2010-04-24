using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class SetNXCommand : SetCommand
    {
        public SetNXCommand(string key, RedisValue value) 
            : base(key, value)
        {
        }
    }
}
