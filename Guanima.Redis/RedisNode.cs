using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Guanima.Redis.Configuration;
using System.Security;
using Guanima.Redis.Utils;

namespace Guanima.Redis
{
	/// <summary>
	/// Represents a Redis node in the pool.
	/// </summary>
	[DebuggerDisplay("{{RedisNode [ Address: {EndPoint}, IsAlive = {IsAlive} ]}}")]
	public sealed class RedisNode : IRedisNode
	{
		private static readonly object SyncRoot = new Object();

		private bool _isDisposed;
		private readonly double _deadTimeout = 2 * 60;

		private readonly IPEndPoint _endPoint;
		private readonly ISocketPoolConfiguration _config;
		private InternalPoolImpl _internalPoolImpl;
		private readonly IAuthenticator _authenticator;
       
		public RedisNode(IEndPointConfiguration endpointConfig, ISocketPoolConfiguration socketPoolConfig, IAuthenticator authenticator)
		{
			_endPoint = endpointConfig.EndPoint;
			_config = socketPoolConfig;

			_deadTimeout = socketPoolConfig.DeadTimeout.TotalSeconds;
			if (_deadTimeout < 0)
				throw new InvalidOperationException("deadTimeout must be >= TimeSpan.Zero");

			if (socketPoolConfig.ConnectionTimeout.TotalMilliseconds >= Int32.MaxValue)
				throw new InvalidOperationException("ConnectionTimeout must be < Int32.MaxValue");

			_authenticator = authenticator;
			_internalPoolImpl = new InternalPoolImpl(this, socketPoolConfig);
		    
            Alias = endpointConfig.Alias;
		    Password = endpointConfig.Password;
		}

		/// <summary>
		/// Gets the <see cref="T:System.Net.IPEndPoint"/> of this instance
		/// </summary>
		public IPEndPoint EndPoint
		{
			get { return _endPoint; }
		}

        public string Alias
        {
            get; private set;
        }

        public string Password
        {
            get; private set;
        }

		/// <summary>
		/// <para>Gets a value indicating whether the server is working or not. Returns a <b>cached</b> state.</para>
		/// <para>To get real-time information and update the cached state, use the <see cref="M:Ping"/> method.</para>
		/// </summary>
		/// <remarks>Used by the <see cref="T:Guanima.Redis.IServerPool"/> to quickly check if the server's state is valid.</remarks>
		public bool IsAlive
		{
			get { return _internalPoolImpl.IsAlive; }
		}

		/// <summary>
		/// Gets a value indicating whether the server is working or not.
		/// 
		/// If the server is not working, and the "being dead" timeout has been expired it will reinitialize itself.
		/// </summary>
		/// <remarks>It's possible that the server is still not up &amp; running so the next call to <see cref="M:Acquire"/> could mark the instance as dead again.</remarks>
		/// <returns></returns>
		public bool Ping()
		{
			// is the server working?
			if (_internalPoolImpl.IsAlive)
				return true;

			// deadTimeout was set to 0 which means forever
			if (_deadTimeout == 0)
				return false;

			TimeSpan diff = DateTime.UtcNow - _internalPoolImpl.MarkedAsDeadUtc;

			// only do the real check if the configured time interval has passed
			if (diff.TotalSeconds < _deadTimeout)
				return false;

			// this codepath is (should be) called very rarely
			// if you get here hundreds of times then you have bigger issues
			// and try to make the Redis instaces more stable and/or increase the deadTimeout
			lock (SyncRoot)
			{
				if (_internalPoolImpl.IsAlive)
					return true;

				// it's easier to create a new pool than reinitializing a dead one
				try { _internalPoolImpl.Dispose(); }
				catch { }

				Interlocked.Exchange(ref _internalPoolImpl, new InternalPoolImpl(this, _config));
			}

			return true;
		}

		/// <summary>
		/// Acquires a new item from the pool
		/// </summary>
		/// <returns>An <see cref="T:Guanima.Redis.PooledSocket"/> instance which is connected to the Redis server, or <value>null</value> if the pool is dead.</returns>
		public PooledSocket Acquire()
		{
			return _internalPoolImpl.Acquire();
		}

		/// <summary>
		/// Releases all resources allocated by this instance
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);

			// this is not a graceful shutdown
			// if someone uses a pooled item then 99% that an exception will be thrown
			// somewhere. But since the dispose is mostly used when everyone else is finished
			// this should not kill any kittens
			lock (SyncRoot)
			{
				if (_isDisposed)
					return;

				_isDisposed = true;

				_internalPoolImpl.Dispose();
				_internalPoolImpl = null;
			}
		}

		void IDisposable.Dispose()
		{
			this.Dispose();
		}

		#region [ InternalPoolImpl             ]
		private class InternalPoolImpl : IDisposable
		{
			private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(InternalPoolImpl));

			/// <summary>
			/// A list of already connected but free to use sockets
			/// </summary>
			private InterlockedQueue<PooledSocket> _freeItems;

			private bool _isDisposed;
			private bool _isAlive;
			private DateTime _markedAsDeadUtc;

			private readonly int _minItems;
			private readonly int _maxItems;
			private int _workingCount;

			private RedisNode _ownerNode;
			private readonly IPEndPoint _endPoint;
			private readonly ISocketPoolConfiguration _config;
			private Semaphore _semaphore;

			private object initLock = new Object();

			internal InternalPoolImpl(RedisNode ownerNode, ISocketPoolConfiguration config)
			{
				_ownerNode = ownerNode;
				_isAlive = true;
				_endPoint = ownerNode.EndPoint;
				_config = config;

				_minItems = config.MinPoolSize;
				_maxItems = config.MaxPoolSize;

				if (_minItems < 0)
					throw new InvalidOperationException("minItems must be larger than 0", null);
				if (_maxItems < _minItems)
					throw new InvalidOperationException("maxItems must be larger than minItems", null);
				if (config.ConnectionTimeout < TimeSpan.Zero)
					throw new InvalidOperationException("connectionTimeout must be >= TimeSpan.Zero", null);

				_semaphore = new Semaphore(_minItems, _maxItems);
				_freeItems = new InterlockedQueue<PooledSocket>();
			}

			private void InitPool()
			{
				try
				{
					if (_minItems > 0)
					{
						for (int i = 0; i < _minItems; i++)
						{
							_freeItems.Enqueue(CreateSocket());

							// cannot connect to the server
							if (!_isAlive)
								break;
						}
					}
				}
				catch (Exception e)
				{
					log.Error("Could not init pool.", e);

					MarkAsDead();
				}
			}

			private PooledSocket CreateSocket()
			{
				var retval = new PooledSocket(_endPoint, _config.ConnectionTimeout, _config.ReceiveTimeout, ReleaseSocket);
				retval.OwnerNode = _ownerNode;

				if (_ownerNode._authenticator != null)
					if (!_ownerNode._authenticator.Authenticate(retval))
						throw new SecurityException("auth failed: " + _endPoint);

				return retval;
			}

			public bool IsAlive
			{
				get { return _isAlive; }
			}

			public DateTime MarkedAsDeadUtc
			{
				get { return _markedAsDeadUtc; }
			}

			/// <summary>
			/// Acquires a new item from the pool
			/// </summary>
			/// <returns>An <see cref="T:Guanima.Redis.PooledSocket"/> instance which is connected to the Redis server, or <value>null</value> if the pool is dead.</returns>
			public PooledSocket Acquire()
			{
				if (log.IsDebugEnabled) log.Debug("Acquiring stream from pool.");

				if (!_isAlive)
				{
					if (log.IsDebugEnabled) log.Debug("Pool is dead, returning null.");

					return null;
				}

				PooledSocket retval;

				if (!_semaphore.WaitOne(_config.ConnectionTimeout))
				{
					if (log.IsDebugEnabled) log.Debug("Pool is full, timeouting.");

					// everyone is so busy
					throw new TimeoutException();
				}

				// maye we died while waiting
				if (!_isAlive)
				{
					if (log.IsDebugEnabled) log.Debug("Pool is dead, returning null.");

					return null;
				}

				// do we have free items?
				if (_freeItems.Dequeue(out retval))
				{
					#region [ get it from the pool         ]
					try
					{
						retval.Reset();

						if (log.IsDebugEnabled) log.Debug("Socket was reset. " + retval.InstanceId);
#if DEBUG
						Interlocked.Increment(ref _workingCount);
#endif
						return retval;
					}
					catch (Exception e)
					{
						log.Error("Failed to reset an acquired socket.", e);

						MarkAsDead();

						return null;
					}
					#endregion
				}

				// free item pool is empty
				if (log.IsDebugEnabled) log.Debug("Could not get a socket from the pool, Creating a new item.");

				try
				{
					// okay, create the new item
					retval = CreateSocket();
#if DEBUG
					Interlocked.Increment(ref _workingCount);
#endif
				}
				catch (Exception e)
				{
					log.Error("Failed to create socket.", e);
					MarkAsDead();

					return null;
				}

				if (log.IsDebugEnabled) log.Debug("Done.");

				return retval;
			}

			private void MarkAsDead()
			{
				if (log.IsWarnEnabled) log.WarnFormat("Marking pool {0} as dead", _endPoint);

				_isAlive = false;
				_markedAsDeadUtc = DateTime.UtcNow;
			}

			/// <summary>
			/// Releases an item back into the pool
			/// </summary>
			/// <param name="socket"></param>
			private void ReleaseSocket(PooledSocket socket)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("Releasing socket " + socket.InstanceId);
					log.Debug("Are we alive? " + _isAlive);
				}

				if (_isAlive)
				{
					// is it still working (i.e. the server is still connected)
					if (socket.IsAlive)
					{
						// mark the item as free
						_freeItems.Enqueue(socket);
#if DEBUG
						Interlocked.Decrement(ref _workingCount);
#endif
						// signal the event so if someone is waiting for it can reuse this item
						_semaphore.Release();
					}
					else
					{
						// kill this item
						socket.Destroy();

						// mark ourselves as not working for a while
						MarkAsDead();
					}
				}
				else
				{
					// one of our previous sockets has died, so probably all of them are dead
					// kill the socket thus clearing the pool, and after we become alive
					// we'll fill the pool with working sockets
					socket.Destroy();
				}
			}

			/// <summary>
			/// Releases all resources allocated by this instance
			/// </summary>
			public void Dispose()
			{
				// this is not a graceful shutdown
				// if someone uses a pooled item then 99% that an exception will be thrown
				// somewhere. But since the dispose is mostly used when everyone else is finished
				// this should not kill any kittens
				lock (this)
				{
					CheckDisposed();

					_isAlive = false;
					_isDisposed = true;

					PooledSocket ps;

					while (_freeItems.Dequeue(out ps))
					{
						try
						{
							ps.OwnerNode = null;
							ps.Destroy();
						}
						catch { }
					}

					_ownerNode = null;
					_semaphore.Close();
					_semaphore = null;
					_freeItems = null;
				}
			}

			private void CheckDisposed()
			{
				if (_isDisposed)
					throw new ObjectDisposedException("pool");
			}

			void IDisposable.Dispose()
			{
				Dispose();
			}
		}
		#endregion
		#region [ Comparer                     ]
		internal sealed class Comparer : IEqualityComparer<IRedisNode>
		{
			public static readonly Comparer Instance = new Comparer();

			bool IEqualityComparer<IRedisNode>.Equals(IRedisNode x, IRedisNode y)
			{
				return x.EndPoint.Equals(y.EndPoint);
			}

			int IEqualityComparer<IRedisNode>.GetHashCode(IRedisNode obj)
			{
				return obj.EndPoint.GetHashCode();
			}
		}
		#endregion
		#region [ NodeFactory                  ]
		//internal sealed class NodeFactory
		//{
		//    private Dictionary<string, RedisNode> nodeCache = new Dictionary<string, RedisNode>(StringComparer.OrdinalIgnoreCase);

		//    internal NodeFactory()
		//    {
		//        AppDomain.CurrentDomain.DomainUnload += DestroyPool;
		//    }

		//    public RedisNode Get(IPEndPoint endpoint, IRedisClientConfiguration config, IRedisAuthenticator _authenticator)
		//    {
		//        ISocketPoolConfiguration ispc = _config.SocketPool;

		//        string cacheKey = String.Concat(endpoint.ToString(), "-",
		//                                            ispc.ConnectionTimeout.Ticks, "-",
		//                                            ispc.DeadTimeout.Ticks, "-",
		//                                            ispc.MaxPoolSize, "-",
		//                                            ispc.MinPoolSize, "-",
		//                                            ispc.ReceiveTimeout.Ticks);

		//        RedisNode node;

		//        if (!nodeCache.TryGetValue(cacheKey, out node))
		//        {
		//            lock (nodeCache)
		//            {
		//                if (!nodeCache.TryGetValue(cacheKey, out node))
		//                {
		//                    node = new RedisNode(endpoint, _config);

		//                    nodeCache[cacheKey] = node;
		//                }
		//            }
		//        }

		//        return node;
		//    }

		//    private void DestroyPool(object sender, EventArgs e)
		//    {
		//        lock (this.nodeCache)
		//        {
		//            foreach (RedisNode node in this.nodeCache.Values)
		//            {
		//                node.Dispose();
		//            }

		//            this.nodeCache.Clear();
		//        }
		//    }
		//}
		#endregion
	}
}

#region [ License information          ]
/* ************************************************************
 *
 * Copyright (c) Attila Kiskó, enyim.com
 *
 * This source code is subject to terms and conditions of 
 * Microsoft Permissive License (Ms-PL).
 * 
 * A copy of the license can be found in the License.html
 * file at the root of this distribution. If you can not 
 * locate the License, please send an email to a@enyim.com
 * 
 * By using this source code in any fashion, you are 
 * agreeing to be bound by the terms of the Microsoft 
 * Permissive License.
 *
 * You must not remove this notice, or any other, from this
 * software.
 *
 * ************************************************************/
#endregion