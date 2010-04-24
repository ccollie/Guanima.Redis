using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class SubstrCommand : KeyCommand
    { 
        public SubstrCommand(string key, int start, int end) 
            : base(key, start, end)
        {
        }
    }
}