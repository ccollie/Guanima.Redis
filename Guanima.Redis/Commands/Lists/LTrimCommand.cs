using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LTrimCommand : KeyCommand
    {
        public LTrimCommand(string key, int start, int end) 
            : base(key, start, end)
        {
        }
    }
}
