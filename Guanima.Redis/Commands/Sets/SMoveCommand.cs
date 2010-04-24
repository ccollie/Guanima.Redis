using System;

namespace Guanima.Redis.Commands.Sets
{
    [Serializable]
    public sealed class SMoveCommand : RedisCommand
    {
        public SMoveCommand(String srcKey, String dstKey, RedisValue member) 
        {
            if (srcKey == null)
                throw new ArgumentNullException("srcKey");
            if (dstKey == null)
                throw new ArgumentNullException("dstkey");
            SetParameters(srcKey, dstKey, member);
        }
    }
}
