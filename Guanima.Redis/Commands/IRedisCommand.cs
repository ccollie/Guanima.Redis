using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands
{
    public interface IRedisCommand
    {
        string Name { get; }
        void SendCommand(IRedisProtocol protocol);
        void ReadReply(IRedisProtocol protocol);
    }
}