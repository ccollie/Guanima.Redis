using Guanima.Redis.Commands.Transactions;

namespace Guanima.Redis
{
    partial class RedisClient
    {
        private RedisClientTransaction _currentTransaction;
        
        #region [ Transactions                 ]

        public bool InTransaction
        {
            get { return _currentTransaction != null; }
        }

        public IRedisClientTransaction BeginTransaction()
        {
            if (InTransaction)
                throw new RedisException("Transaction already started");
            FlushPipeline();
            BeginPipeline();
            _currentTransaction = new RedisClientTransaction(this);
            return _currentTransaction;
        }

        // dont know if i like this
        internal void FinishTransaction()
        {
            if (InTransaction && _queuedCommands != null)
            {
                try
                {
                    foreach (var kvp in _queuedCommands)
                    {
                        var node = kvp.Key;
                        var commandsForThisNode = kvp.Value;

                        using(var socket = node.Acquire())
                        {
                            var command = new MultiExecCommand(commandsForThisNode);
                            protocolHandler.Socket = socket;
                            command.Execute(protocolHandler);
                        }
                    }
                }
                catch(RedisClientException ex)
                {
                    log.Error(ex);
                    throw;
                }
                finally
                {
                    _currentTransaction = null;
                    ClearPipeline();
                }
            }
        }

        #endregion
    }
}
