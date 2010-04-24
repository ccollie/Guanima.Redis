using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class AdHocCommand : RedisCommand
    {
        private readonly string _commandText;
        private readonly byte[] _data;

        public AdHocCommand(string commandText)
        {
            if (String.IsNullOrEmpty(commandText))
                throw new ArgumentException("Command text cannot be null or empty");
            _commandText = commandText;
        }


        public AdHocCommand(string commandText, byte[] value)
        {
            ValidateDataLength(value);
            if (String.IsNullOrEmpty(commandText))
                throw new ArgumentException("Command text cannot be null or empty");
            _commandText = commandText;
            _data = value;
        }

    }
}
