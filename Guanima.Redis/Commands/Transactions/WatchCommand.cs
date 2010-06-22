using System;

namespace Guanima.Redis.Commands.Transactions
{
    [Serializable]
    public sealed class WatchCommand : RedisCommand
    {
        public WatchCommand(params string[] keys)
        {
            Arguments = CommandUtils.ConstructParameters(Command.Watch, keys);
        }

        public WatchCommand(string key)
            : this(new string[] { key })
        {
        }
    }
}
