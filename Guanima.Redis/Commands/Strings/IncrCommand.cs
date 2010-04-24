using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class IncrCommand : KeyCommand
    {
        public IncrCommand(String key) :
            base(key)
        {
        }
    }
}
