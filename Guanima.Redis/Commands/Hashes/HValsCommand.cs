using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HValsCommand : KeyCommand
    {
        public HValsCommand(string key)
            : base(key)
        {
        }
    }
}
