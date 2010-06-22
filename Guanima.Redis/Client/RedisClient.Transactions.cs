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
            if (InTransaction && _queuedCommandList != null)
            {
                try
                {
                    foreach (var kvp in _commandQueues)
                    {
                        var node = kvp.Key;
                        // TODO: Fix this....
                        kvp.Value.ReadAllResults();
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
