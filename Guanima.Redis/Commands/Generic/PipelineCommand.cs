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

        public override void WriteTo(PooledSocket socket)
        {
            if (Transactional)
                _multi.WriteTo(socket);

            foreach (var command in _commands)
            {
                // todo: if transactional, ignore MULTI or EXEC, if any
                command.WriteTo(socket);
            }
            if (Transactional)
                _exec.WriteTo(socket);
        }

        public override void ReadFrom(PooledSocket socket)
        {
            var replies = new List<RedisValue>();
            if (Transactional)
            {
                _multi.ReadFrom(socket);  // OK

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
            }

            foreach (var command in _commands)
            {
                command.ReadFrom(socket);
                replies.Add(command.Value);
            }
            this.Value = new RedisValue(){Type = RedisValueType.MultiBulk, MultiBulkValues =replies.ToArray()};   
        }

    
    }
}