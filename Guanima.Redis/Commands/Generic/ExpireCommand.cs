using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class ExpireCommand : RedisCommand
    {
        public ExpireCommand(String key, TimeSpan timeout) :
            this(key, (int)timeout.TotalSeconds)
        {
        }

        public ExpireCommand(String key, int timeoutInSeconds) 
        {
            ValidateKey(key);
            if (timeoutInSeconds  < 0)
                throw new ArgumentOutOfRangeException("timeoutInSeconds", "timeoutInSeconds must be >= 0");
            SetParameters(key, timeoutInSeconds);
        }
    }
}
