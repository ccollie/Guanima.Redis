using System;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZCardCommand : KeyCommand
    {
        public ZCardCommand(string key) 
            : base(key)
        {
        }
    }
}
