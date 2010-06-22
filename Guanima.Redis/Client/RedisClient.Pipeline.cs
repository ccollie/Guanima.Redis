using System;
using System.Collections.Generic;
using Guanima.Redis.Client;
using Guanima.Redis.Commands;

namespace Guanima.Redis
{
    partial class RedisClient
    {
        // list of commands ordered by server and insertion order
        private Dictionary<IRedisNode, RedisCommandQueue> _commandQueues;
        private bool _pipelining = false;
        
        // Globals list of queued commands in the order that they were added
        private List<RedisCommand> _queuedCommandList;

        public bool Pipelining
        {
            get { return _pipelining || InTransaction; }
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
            foreach(var queue in _commandQueues.Values)
            {
                queue.ReadAllResults();
            }
            _pipelining = true;
            return new RedisPipeline(this, transactional);
        }

        private void EnsureCommandQueue()
        {
            if (_queuedCommandList == null)
                _queuedCommandList = new List<RedisCommand>();
            if (_commandQueues == null)
                _commandQueues = new Dictionary<IRedisNode, RedisCommandQueue>();
        }

        internal void FlushPipeline()
        {
            FlushPipeline(false);
        }

        internal void FlushPipeline(bool transactional)
        {
            _pipelining = false;
            try
            {
                foreach (var queue in _commandQueues.Values)
                {
                    if (transactional)
                        queue.IsTransactional = false;
                    else
                    {
                        queue.ReadAllResults();
                    }
                }
            }
            finally
            {
                _queuedCommandList = null;
            }
        }

        internal void ClearPipeline()
        {
            if (_commandQueues != null)
            {
                foreach (var q in _commandQueues.Values)
                {
                    q.Clear();
                }
            }
            _queuedCommandList = null;
        }

        internal RedisCommandQueue GetCommandQueue(IRedisNode node)
        {
            EnsureCommandQueue();
			RedisCommandQueue q;
			if (!_commandQueues.TryGetValue(node, out q)) {
				q = new RedisCommandQueue(node, InTransaction);
			    q.CurrentDB = this.CurrentDB;
				_commandQueues[node] = q;
			}
            else
            {
                q.IsTransactional = InTransaction;
            }
            return q;
        }

        public void EnqueueCommand(IRedisNode node, RedisCommand command)
        {
			RedisCommandQueue q = GetCommandQueue(node);
 			q.Enqueue(command);
            // if (Pipelined || InTransaction)
             _queuedCommandList.Add(command);
        }

        public void Flush()
        {
            foreach (var q in _commandQueues.Values)
            {
                q.Flush();
            }            
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
