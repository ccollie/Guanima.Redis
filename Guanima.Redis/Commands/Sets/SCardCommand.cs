using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public class SCardCommand : KeyCommand
    {
        public SCardCommand(string key) 
            : base(key)
        {
        }
    }
}
