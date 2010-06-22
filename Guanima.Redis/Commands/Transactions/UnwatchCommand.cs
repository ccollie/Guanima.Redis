using System;

namespace Guanima.Redis.Commands.Transactions
{
    [Serializable]
    public sealed class UnWatchCommand : RedisCommand
    {
        public UnWatchCommand(params string[] keys)
        {
            Arguments = CommandUtils.ConstructParameters(Command.Unwatch, keys);
        }

        public UnWatchCommand(string key)
            : this(new string[] { key })
        {
        }
    }
}
