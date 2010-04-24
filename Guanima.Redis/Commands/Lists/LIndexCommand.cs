using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LIndexCommand : KeyCommand
    {
        public LIndexCommand(string key, int index)
            : base(key, index)
        {
        }
    }
}
