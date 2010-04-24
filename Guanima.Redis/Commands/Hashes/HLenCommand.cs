using System;

namespace Guanima.Redis.Commands.Hashes
{
    [Serializable]
    public sealed class HLenCommand : KeyCommand
    {
        public HLenCommand(String key) :
            base(key)
        {
        }
    }
}