using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class RPopCommand : KeyCommand
    {
        public RPopCommand(string key)
            : base(key)
        {
        }
    }
}
