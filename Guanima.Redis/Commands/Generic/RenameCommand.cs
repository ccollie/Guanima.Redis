using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public class RenameCommand : RedisCommand
    {
        public RenameCommand(String oldName, String newName)
        {
            ValidateKey(oldName);
            if (newName == null) 
                throw new ArgumentNullException("newName", "New name must be specified.");
            if (newName.Length == 0)
                throw new ArgumentException("Item key must be specified.", "newName");

            SetParameters(oldName, newName);
        }
    }
}
