using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class KeysCommand : KeyCommand
    {
        public KeysCommand(String key) :
            base(key)
        {
        }
    }
}
