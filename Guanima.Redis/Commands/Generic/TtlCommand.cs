using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class TtlCommand : KeyCommand
    {
        public TtlCommand(string key) :
            base(key)
        {
        }
    }
}
