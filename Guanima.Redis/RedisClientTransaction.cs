using System.Collections.Generic;
using Guanima.Redis.Commands;
using Guanima.Redis.Utils;

namespace Guanima.Redis
{
    /// <summary>
    /// Experimental support, has not been tested.
    /// </summary>
    public class RedisClientTransaction : Disposable, IRedisClientTransaction
    {
        private bool _committed;
        private bool _discarded;
        private readonly RedisClient _client;

        public RedisClientTransaction(RedisClient client)
        {
            _client = client;
        }

        public void Discard()
        {            
            _client.ClearPipeline();
            _discarded = true;
        }

        public void Commit()
        {
            if (_committed)
                throw new RedisException("Commit already called for this transaction.");
            if (_discarded)
                throw new RedisException("Discard already called for this transaction.");

            _committed = true;
            _client.FinishTransaction();
        }

        #region Disposable

        protected override void Release()
        {
            // i dont know if i like this level of intimacy
            if (!_committed && !_discarded)
                Commit();
        }
   
        #endregion

        public IEnumerable<RedisCommand> Commands
        {
            get { return _client.QueuedCommands; }
        }
    }

}
