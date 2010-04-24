using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LRangeCommand : KeyCommand
    {
        public LRangeCommand(string key, int start, int end) 
            : base(key, start, end)
        {
        }
    }
}
