using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class SelectCommand : RedisCommand
    {
        public SelectCommand(int db) :
            base(db)
        {
        }
    }
}
