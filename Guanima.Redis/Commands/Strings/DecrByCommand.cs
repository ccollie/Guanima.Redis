using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class DecrByCommand : KeyCommand
    {
        public DecrByCommand(String key, long delta) :
            base(key, delta)
        {
        }
    }
}
