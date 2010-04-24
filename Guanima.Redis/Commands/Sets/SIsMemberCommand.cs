using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SIsMemberCommand : KeyValueCommand
    {
        public SIsMemberCommand(String key, RedisValue value) :
            base(key, value)
        {
        }
        
    }
}