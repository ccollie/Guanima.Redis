using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.Transactions;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public class PipelineCommand : RedisCommand
    {
        private readonly List<RedisCommand> _commands = new List<RedisCommand>();
        private readonly bool _transactional;
        private readonly MultiCommand _multi;
        private readonly ExecCommand _exec;

        public PipelineCommand(IEnumerable<RedisCommand> commands)
            :this(commands,false)
        {
            
        }

        public PipelineCommand(IEnumerable<RedisCommand> commands, bool transactional)
        {
            if (commands == null)
                throw new ArgumentNullException("commands");
            _commands.AddRange(commands);
            _transactional = transactional;
            if (transactional)
            {
                _multi = new MultiCommand();
                _exec = new ExecCommand();
            } 
            else
            {
                _multi = null;
                _exec = null;
            }
        }
        
        public PipelineCommand(params RedisCommand[] commands)
        {
            if (commands == null)
                throw new ArgumentNullException("commands");
            _commands.AddRange(commands);
        }

        public bool Transactional
        {
            get { return _transactional; }
        }

        public override void SendCommand(IRedisProtocol protocol)
        {
            if (Transactional)
                _multi.SendCommand(protocol);

            foreach (var command in _commands)
            {
                // todo: if transactional, ignore MULTI or EXEC, if any
                command.SendCommand(protocol);
            }
            if (Transactional)
                _exec.SendCommand(protocol);
        }

        public override void ReadReply(IRedisProtocol protocol)
        {
            var replies = new List<RedisValue>();
            if (Transactional)
            {
                _multi.ReadReply(protocol);  // OK

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
            }

            foreach (var command in _commands)
            {
                command.ReadReply(protocol);
                replies.Add(command.Result);
            }
            this.Result = new RedisValue(){Type = RedisValueType.MultiBulk, MultiBulkValues =replies.ToArray()};   
        }

    
    }
}