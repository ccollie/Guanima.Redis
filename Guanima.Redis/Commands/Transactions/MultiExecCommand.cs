using System;
using System.Collections.Generic;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.Transactions
{

    public class MultiExecCommand : RedisCommand
    {
        private readonly List<RedisCommand> _commands = new List<RedisCommand>();

        public MultiExecCommand(IEnumerable<RedisCommand> commands)
        {
            if (commands == null)
                throw new ArgumentNullException("commands");
            _commands.AddRange(commands);
        }

        public MultiExecCommand(params RedisCommand[] commands)
        {
            if (commands.Length == 0)
                throw new ArgumentException("At least 1 command expected", "commands");
            _commands.AddRange(commands);
        }

        public override string Name
        {
            get { return "MULTIEXEC"; }
        }

        public override void WriteTo(PooledSocket socket)
        {
            new MultiCommand().WriteTo(socket);

            foreach (var command in _commands)
            {
                command.WriteTo(socket);
            }
            new ExecCommand().WriteTo(socket);                    
        }

        public override void ReadFrom(PooledSocket socket)
        {
            socket.ExpectSingleLineReply(); // OK - matching Multi

            for (int i = 0; i < _commands.Count; i++)
            {
                // read "QUEUED"
                var status = socket.ExpectSingleLineReply();
            }
            // The result is a multi-bulk, so
            // consume the count of returned items
            var count = socket.ExpectMultiBulkCount();
            if (count != _commands.Count)
                throw new RedisClientException(
                    String.Format("Invalid number of bulk responses. Expected {0}, Got {1}", _commands.Count, count));

            foreach (var command in _commands)
            {
                command.ReadFrom(socket);
            }
        }

    }
}