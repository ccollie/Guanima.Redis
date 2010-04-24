using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HGetAllCommand : KeyCommand
    {
        public HGetAllCommand(string key)
            : base(key)
        {
        }
    }
}
