using System;
using System.Collections.Generic;
using Guanima.Redis.Commands;

namespace Guanima.Redis
{
    public interface IRedisPipeline : IDisposable
    {
        IEnumerable<RedisCommand> Commands { get; }
    }

}
