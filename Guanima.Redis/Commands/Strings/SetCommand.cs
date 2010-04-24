using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public class SetCommand : KeyValueCommand
    {
        public SetCommand(string key, RedisValue value) 
            : base(key, value)
        {
        }
    }
}
