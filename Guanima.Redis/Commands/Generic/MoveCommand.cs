using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class MoveCommand : KeyCommand
    {
        public MoveCommand(String oldName, int db) :
            base(oldName, db)
        {
        }
    }
}
