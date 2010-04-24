using System;
using System.Collections.Generic;
using Guanima.Redis.Commands;
using Guanima.Redis.Commands.Generic;

namespace Guanima.Redis
{
    partial class RedisClient
    {
        // list of commands ordered by server and insertion order
        private Dictionary<IRedisNode, List<RedisCommand>> _queuedCommands;
        
        // Globals list of queued commands in the order that they were added
        private List<RedisCommand> _queuedCommandList;

        public bool Pipelining
        {
            get { return _queuedCommands != null || InTransaction; }
        }

        public IDisposable BeginPipeline()
        {
            return BeginPipeline(false);
        }

        public IDisposable BeginPipeline(bool transactional)
        {
            if (InTransaction)
                throw new RedisClientException("Pipelining not applicable in transactions.");
            if (Pipelining)
                throw new RedisException("Already pipelining.");
            EnsureCommandQueue();
            return new RedisPipeline(this, transactional);
        }

        private void EnsureCommandQueue()
        {
            if (_queuedCommands == null)
            {
                _queuedCommands = new Dictionary<IRedisNode, List<RedisCommand>>();
                _queuedCommandList = new List<RedisCommand>();
            }
        }

        internal void FlushPipeline()
        {
            FlushPipeline(false);
        }

        internal void FlushPipeline(bool transactional)
        {
            if (_queuedCommands != null)
            {
                var temp = _queuedCommands;
                _queuedCommands = null;
                _queuedCommandList = null;

                // TODO: if transactional, search through an pull out MULTI or EXEC
 
                foreach (var item in temp)
                {
                    var command = new PipelineCommand(item.Value, transactional);
                    Execute(item.Key, command);
                }
            }
        }

        internal void ClearPipeline()
        {
            if (_queuedCommands != null)
            {
                _queuedCommands.Clear();
                _queuedCommands = null;
            }
            _queuedCommandList = null;
        }

        internal void EnqueueCommand(IRedisNode node, RedisCommand command)
        {
            List<RedisCommand> commands;
            EnsureCommandQueue();
            if (!_queuedCommands.TryGetValue(node, out commands))
            {
                commands = new List<RedisCommand>();
                _queuedCommands[node] = commands;
            }
            commands.Add(command);
            _queuedCommandList.Add(command);
        }

        internal IEnumerable<RedisCommand> QueuedCommands
        {
            get
            {
                if (_queuedCommandList == null)
                    return new List<RedisCommand>();
                return _queuedCommandList;
            }
        }

    }
}
