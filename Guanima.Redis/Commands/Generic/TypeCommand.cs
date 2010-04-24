using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class TypeCommand : KeyCommand
    {
        public TypeCommand(String key) :
            base(key)
        {
        }
    }
}
