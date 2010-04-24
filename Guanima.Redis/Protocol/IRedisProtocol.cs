namespace Guanima.Redis.Protocol
{
    public interface IRedisProtocol
    {
        PooledSocket Socket { get; set; }

        byte[] ReadBulkData();

        /// <summary>
        /// Expect a the bulk count from the redis server
        /// </summary>
        /// <returns>Bulk item count</returns>
        int ExpectBulkCount();

        /// <summary>
        /// Expect a multi bulk reply from the server.
        /// </summary>
        /// <returns>Multi bulk item count</returns>
        int ExpectMultiBulkCount();

        void IssueCommand(string name, params RedisValue[] parameters);

        void WriteValue(RedisValue value);

        RedisValue ReadReply();

        /// <summary>
        /// Waits for an integer reply from the server
        /// </summary>
        /// <returns>Integer reply value</returns>
        int ExpectIntegerReply();

        /// <summary>
        /// Waits for a single line reply from the server
        /// </summary>
        /// <returns>Single line reply value</returns>
        string ExpectSingleLineReply();

        /// <summary>
        /// Waits for a bulk reply from the server as binary.
        /// </summary>
        /// <returns></returns>
        byte[] ExpectBulkReply();


        /// <summary>
        /// Waits for a multi bulk reply from the server in binary.
        /// </summary>
        /// <returns>Multi bulk byte/string array.</returns>
        byte[][] ExpectMultiBulkReply();

    }
}