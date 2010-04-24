using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class AppendCommand : KeyValueCommand
    {
        public AppendCommand(string key, RedisValue value) :
            base(key, value)
        {
        }
    }
}
