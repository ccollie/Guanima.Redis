using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public sealed class DecrCommand : KeyCommand
    {
        public DecrCommand(String key) :
            base(key)
        {
        }
    }
}
