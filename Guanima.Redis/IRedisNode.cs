using System;
using System.Net;

namespace Guanima.Redis
{
    public interface IRedisNode : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="T:System.Net.IPEndPoint"/> of this instance
        /// </summary>
        IPEndPoint EndPoint { get; }

        /// <summary>
        /// Gets the Alias of this instance.
        /// </summary>
        string Alias { get; }

        string Password { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the server is working or not. Returns a <b>cached</b> state.</para>
        /// <para>To get real-time information and update the cached state, use the <see cref="M:Ping"/> method.</para>
        /// </summary>
        /// <remarks>Used by the <see cref="T:Guanima.Redis.IServerPool"/> to quickly check if the server's state is valid.</remarks>
        bool IsAlive { get; }

        /// <summary>
        /// Gets a value indicating whether the server is working or not.
        /// 
        /// If the server is not working, and the "being dead" timeout has been expired it will reinitialize itself.
        /// </summary>
        /// <remarks>It's possible that the server is still not up &amp; running so the next call to <see cref="M:Acquire"/> could mark the instance as dead again.</remarks>
        /// <returns></returns>
        bool Ping();

        /// <summary>
        /// Acquires a new item from the pool
        /// </summary>
        /// <returns>An <see cref="T:Guanima.Redis.PooledSocket"/> instance which is connected to the Redis server, or <value>null</value> if the pool is dead.</returns>
        PooledSocket Acquire();
    }
}