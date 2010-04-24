using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class RenameNXCommand : RenameCommand
    {
         public RenameNXCommand(String oldName, String newName) :
                base(oldName, newName)
         {
         }
    }
}
