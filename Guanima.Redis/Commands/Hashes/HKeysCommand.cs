using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HKeysCommand : KeyCommand
    {
        public HKeysCommand(string key)
            : base(key)
        {
        }
    }
}
