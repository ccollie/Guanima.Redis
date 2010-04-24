using System;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.Control
{
    [Serializable]
    public sealed class SlaveOfCommand : RedisCommand
    {
        public const int DefaultPort = 6379;    // TODO: Make global const

        public SlaveOfCommand()
        {
            SetParameters("NO","ONE");
        }

        public SlaveOfCommand(string host)
            :this(host,DefaultPort)
        {
        }

        public SlaveOfCommand(string host, int port)
        {
            SetParameters(host, port);
        }

    }
}
