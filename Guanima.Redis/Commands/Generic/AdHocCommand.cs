using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class AdHocCommand : RedisCommand
    {      
        public AdHocCommand(params RedisValue[] parms)
        {
            this.SetParameters(parms);
        }

        internal RedisClient Client {get; set;} 
    }
}
