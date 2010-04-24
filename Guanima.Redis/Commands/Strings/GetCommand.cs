using System;

namespace Guanima.Redis.Commands.Strings
{
    [Serializable]
    public class GetCommand : KeyCommand
    {
        public GetCommand(string key) : 
            base(key)
        {
        }
    }
}
