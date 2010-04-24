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

        public override void SendCommand(IRedisProtocol protocol)
        {
            new MultiCommand().SendCommand(protocol);

            foreach (var command in _commands)
            {
                command.SendCommand(protocol);
            }
            new ExecCommand().SendCommand(protocol);                    
        }

        public override void ReadReply(IRedisProtocol protocol)
        {
            protocol.ExpectSingleLineReply(); // OK - matching Multi

            for (int i = 0; i < _commands.Count; i++)
            {
                // read "QUEUED"
                var status = protocol.ExpectSingleLineReply();
            }
            // The result is a multi-bulk, so
            // consume the count of returned items
            var count = protocol.ExpectMultiBulkCount();
            if (count != _commands.Count)
                throw new RedisClientException(
                    String.Format("Invalid number of bulk responses. Expected {0}, Got {1}", _commands.Count, count));

            foreach (var command in _commands)
            {
                command.ReadReply(protocol);
            }
        }

    }
}