using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using Guanima.Redis.Configuration;
using Guanima.Redis.KeyTransformers;
using Guanima.Redis.NodeLocators;
using Guanima.Redis.Transcoders;
using Guanima.Redis.Utils;

namespace Guanima.Redis
{
	public class DefaultServerPool : IServerPool
	{
		// holds all dead servers which will be periodically rechecked and put back into the working servers if found alive
	    readonly List<IRedisNode> _deadServers = new List<IRedisNode>();
		// holds all of the currently working servers
	    readonly List<IRedisNode> _workingServers = new List<IRedisNode>();

		private ReadOnlyCollection<IRedisNode> _publicWorkingServers;

		// used to synchronize read/write accesses on the server lists
		private ReaderWriterLock _serverAccessLock = new ReaderWriterLock();

		private Timer _isAliveTimer;
		private readonly IRedisClientConfiguration _configuration;
		private readonly IRedisKeyTransformer _keyTransformer;
		private IRedisNodeLocator _nodeLocator;
		private readonly ITranscoder _transcoder;

		public IEnumerable<IRedisNode> GetServers()
		{
			return WorkingServers;
		}

		public DefaultServerPool(IRedisClientConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration", "Invalid or missing pool configuration. Check if the Guanima/Redis section or your custom section presents in the app/web.config.");

			_configuration = configuration;
			_isAliveTimer = new Timer(callback_isAliveTimer, null, (int)_configuration.SocketPool.DeadTimeout.TotalMilliseconds, (int)this._configuration.SocketPool.DeadTimeout.TotalMilliseconds);

			// create the key transformer instance
			Type t = _configuration.KeyTransformer;
			_keyTransformer = (t == null) ? new DefaultKeyTransformer() : (IRedisKeyTransformer)FastActivator.CreateInstance(t);

			// create the item _transcoder instance
			t = _configuration.Transcoder;
			_transcoder = (t == null) ? new DefaultTranscoder() : (ITranscoder)FastActivator.CreateInstance(t);
		}

		//public event Action<PooledSocket> SocketConnected;

		/// <summary>
		/// This will start the pool: initializes the nodelocator, warms up the socket pools, etc.
		/// </summary>
		public void Start()
		{
			// initialize the server list
			foreach (IEndPointConfiguration ip in _configuration.Servers)
			{
				var node = new RedisNode(ip, _configuration.SocketPool, Authenticator);

				_workingServers.Add(node);
			}

			// initializes the locator
			RebuildIndexes();
		}

		private void RebuildIndexes()
		{
			_serverAccessLock.UpgradeToWriterLock(Timeout.Infinite);

			try
			{
				Type ltype = _configuration.NodeLocator;

				IRedisNodeLocator l = ltype == null ? new DefaultNodeLocator() : (IRedisNodeLocator)FastActivator.CreateInstance(ltype);
				l.Initialize(_workingServers);

				_nodeLocator = l;

				_publicWorkingServers = null;
			}
			finally
			{
				_serverAccessLock.ReleaseLock();
			}
		}

		/// <summary>
		/// Checks if a dead node is working again.
		/// </summary>
		/// <param name="state"></param>
		private void callback_isAliveTimer(object state)
		{
			_serverAccessLock.AcquireReaderLock(Timeout.Infinite);

			try
			{
				if (_deadServers.Count == 0)
					return;

				List<IRedisNode> resurrectList = _deadServers.FindAll(delegate(IRedisNode node) { return node.Ping(); });

				if (resurrectList.Count > 0)
				{
					_serverAccessLock.UpgradeToWriterLock(Timeout.Infinite);

					resurrectList.ForEach(delegate(IRedisNode node)
					{
						// maybe it got removed while we were waiting for the writer lock upgrade?
						if (_deadServers.Remove(node))
							_workingServers.Add(node);
					});

					RebuildIndexes();
				}
			}
			finally
			{
				_serverAccessLock.ReleaseLock();
			}
		}

		/// <summary>
		/// Marks a node as dead (unusable)
		///  - moves the node to the  "dead list"
		///  - recreates the locator based on the new list of still functioning servers
		/// </summary>
		/// <param name="node"></param>
		private void MarkAsDead(IRedisNode node)
		{
			_serverAccessLock.UpgradeToWriterLock(Timeout.Infinite);

			try
			{
				// server gained AoeREZ while AFK?
				if (!node.IsAlive)
				{
					_workingServers.Remove(node);
					_deadServers.Add(node);

					RebuildIndexes();
				}
			}
			finally
			{
				_serverAccessLock.ReleaseLock();
			}
		}

		/// <summary>
		/// Returns the <see cref="t:IKeyTransformer"/> instance used by the pool
		/// </summary>
		public IRedisKeyTransformer KeyTransformer
		{
			get { return _keyTransformer; }
		}

		public IRedisNodeLocator NodeLocator
		{
			get { return _nodeLocator; }
		}

		public ITranscoder Transcoder
		{
			get { return _transcoder; }
		}

		/// <summary>
		/// Finds the <see cref="T:Guanima.Redis.RedisNode"/> which is responsible for the specified item
		/// </summary>
		/// <param name="itemKey"></param>
		/// <returns></returns>
		private IRedisNode LocateNode(string itemKey)
		{
			_serverAccessLock.AcquireReaderLock(Timeout.Infinite);

			try
			{
				IRedisNode node = NodeLocator.Locate(itemKey);
				if (node == null)
					return null;

				if (node.IsAlive)
					return node;

				MarkAsDead(node);

				return LocateNode(itemKey);
			}
			finally
			{
				_serverAccessLock.ReleaseLock();
			}
		}

		public PooledSocket Acquire(string itemKey)
		{
			if (_serverAccessLock == null)
				throw new ObjectDisposedException("ServerPool");

			IRedisNode server = LocateNode(itemKey);

			if (server == null)
				return null;

			return server.Acquire();
		}

		public ReadOnlyCollection<IRedisNode> WorkingServers
		{
			get
			{
				if (_publicWorkingServers == null)
				{
					_serverAccessLock.AcquireReaderLock(Timeout.Infinite);

					try
					{
						if (_publicWorkingServers == null)
						{
							_publicWorkingServers = new ReadOnlyCollection<IRedisNode>(_workingServers.ToArray());
						}
					}
					finally
					{
						_serverAccessLock.ReleaseLock();
					}
				}

				return _publicWorkingServers;
			}
		}

		public IDictionary<IRedisNode, IList<string>> SplitKeys(IEnumerable<string> keys)
		{
			var keysByNode = new Dictionary<IRedisNode, IList<string>>(RedisNode.Comparer.Instance);

			IList<string> nodeKeys;

		    foreach (string key in keys)
			{
				IRedisNode node = LocateNode(key);

				if (!keysByNode.TryGetValue(node, out nodeKeys))
				{
					nodeKeys = new List<string>();
					keysByNode.Add(node, nodeKeys);
				}

				nodeKeys.Add(key);
			}

			return keysByNode;
		}

		#region [ IDisposable                  ]
		void IDisposable.Dispose()
		{
			var rwl = _serverAccessLock;

			if (Interlocked.CompareExchange(ref _serverAccessLock, null, rwl) == null)
				return;

			GC.SuppressFinalize(this);

			try
			{
				rwl.UpgradeToWriterLock(Timeout.Infinite);

				Action<IRedisNode> cleanupNode = node =>
				{
					//node.SocketConnected -= OnSocketConnected;
					node.Dispose();
				};

				// dispose the nodes (they'll kill connections, etc.)
				_deadServers.ForEach(cleanupNode);
				_workingServers.ForEach(cleanupNode);

				_deadServers.Clear();
				_workingServers.Clear();

				_nodeLocator = null;

				_isAliveTimer.Dispose();
				_isAliveTimer = null;
			}
			finally
			{
				rwl.ReleaseLock();
			}
		}
		#endregion

		#region IServerPool Members

		IRedisKeyTransformer IServerPool.KeyTransformer
		{
			get { return KeyTransformer; }
		}

		ITranscoder IServerPool.Transcoder
		{
			get { return Transcoder; }
		}

		//IAuthenticator IServerPool.Authenticator
		//{
		//    get { return authenticator; }
		//}

		PooledSocket IServerPool.Acquire(string key)
		{
			return Acquire(key);
		}

		IEnumerable<IRedisNode> IServerPool.GetServers()
		{
			return GetServers();
		}

		void IServerPool.Start()
		{
			Start();
		}

		//event Action<PooledSocket> IServerPool.SocketConnected
		//{
		//    add { SocketConnected += value; }
		//    remove { SocketConnected -= value; }
		//}

		#endregion

		public IAuthenticator Authenticator { get; set; }
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