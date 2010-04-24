using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SPopCommand : KeyCommand
    {
        public SPopCommand(string key)
            : base(key)
        {
        }
    }
}