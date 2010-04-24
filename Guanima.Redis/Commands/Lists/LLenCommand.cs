using System;

namespace Guanima.Redis.Commands.Lists
{
    [Serializable]
    public sealed class LLenCommand : KeyCommand
    {
        public LLenCommand(String key) :
            base(key)
        {
        }
    }
}
