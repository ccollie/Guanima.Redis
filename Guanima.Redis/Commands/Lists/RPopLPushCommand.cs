using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class RPopLPushCommand : KeyCommand
    {
        public RPopLPushCommand(string srcKey, string destKey)
            : base(srcKey, destKey)
        {
            ValidateKey(destKey);
        }

    }
}
