using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class IncrByCommand : KeyCommand
    {
        public IncrByCommand(String key, long delta) :
            base(key, delta)
        {
        }
    }
}
