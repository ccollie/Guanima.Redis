using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HDelCommand : KeyCommand
    {
        public HDelCommand(string key, string field)
            : base(key, field)
        {
        }

    }
}
