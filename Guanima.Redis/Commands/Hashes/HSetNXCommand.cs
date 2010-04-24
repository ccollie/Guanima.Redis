using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HSetNXCommand : KeyCommand
    {
        public HSetNXCommand(string key, string field, RedisValue value)
            : base(key, field, value)
        {

        }

   }
}