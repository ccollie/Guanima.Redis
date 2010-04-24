using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HSetCommand : KeyCommand
    {
        public HSetCommand(string key, string field, RedisValue value)
            : base(key, field, value)
        {

        }

   }
}