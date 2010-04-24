using System;

namespace Guanima.Redis.Commands.Connection
{
    [Serializable]
    public sealed class AuthCommand : RedisCommand
    {
        public AuthCommand(string password) 
        {
            if (password == null)
                throw new ArgumentNullException("password", "Null password specified.");
            if (password.Length == 0)
                throw new ArgumentException("Empty password.", "password");

            SetParameters(password);
        }
      
    }
}
