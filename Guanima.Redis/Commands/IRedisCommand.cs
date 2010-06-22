namespace Guanima.Redis.Commands
{
    public interface IRedisCommand
    {
        string Name { get; }    // TODO: needs to be utf8 byte[]
        void WriteTo(PooledSocket socket);
        void ReadFrom(PooledSocket socket);
    }
}