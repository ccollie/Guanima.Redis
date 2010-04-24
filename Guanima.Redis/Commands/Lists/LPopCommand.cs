using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LPopCommand : KeyCommand
    {
        public LPopCommand(string key)
            : base(key)
        {
        }
    }
}
