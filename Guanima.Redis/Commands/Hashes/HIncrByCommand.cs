using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public class HIncrByCommand : KeyCommand
    {
        public HIncrByCommand(string key, string field, int value)
            : base(key, field, value)
        {
            // todo: validate field
        }
    }
}
