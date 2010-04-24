using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SMembersCommand : KeyCommand
    {
        public SMembersCommand(string key) 
            : base(key)
        {
        }
    }
}
