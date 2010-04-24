using System.Collections.Generic;
using Guanima.Redis.Commands;
using Guanima.Redis.Utils;

namespace Guanima.Redis
{
    public class RedisPipeline : Disposable, IRedisPipeline
    {
        private readonly RedisClient _client;
        private readonly bool _transactional;

        public RedisPipeline(RedisClient client)
            :this(client, false)
        {
            
        }

        public RedisPipeline(RedisClient client, bool transactional)
        {
            _client = client;
            _transactional = transactional;
        }

        public IEnumerable<RedisCommand> Commands
        {
            get { return _client.QueuedCommands; }
        }

        public bool IsTransactional
        {
            get { return _transactional; }    
        }

        protected override void Release()
        {
            _client.FlushPipeline(IsTransactional);
        }
    }
}
