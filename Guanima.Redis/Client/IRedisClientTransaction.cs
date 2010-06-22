namespace Guanima.Redis
{
    public interface IRedisClientTransaction : IRedisPipeline
    {
        void Commit();
        void Discard();
    }
}