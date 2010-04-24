using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class ExistsCommand : KeyCommand
    {
        public ExistsCommand(string key) :
            base(key)
        {
        }
    }
}
