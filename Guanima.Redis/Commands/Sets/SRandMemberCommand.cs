using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SRandMemberCommand : KeyCommand
    {
        public SRandMemberCommand(string key) 
            : base(key)
        {
        }
    }
}
