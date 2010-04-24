using System;
using System.Collections.Generic;
using Guanima.Redis.Transcoders;

namespace Guanima.Redis.Configuration
{
	/// <summary>
	/// COnfiguration class
	/// </summary>
	public class RedisClientConfiguration : IRedisClientConfiguration
	{
	    private readonly List<IEndPointConfiguration> _servers;
		private readonly ISocketPoolConfiguration _socketPool;
		private readonly IAuthenticationConfiguration _authentication;
		private Type _keyTransformer;
		private Type _nodeLocator;
		private Type _transcoder;
	    private Type _keyTagExtractor;
	    private int _defaultDb;
    
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.Configuration.RedisClientConfiguration"/> class.
		/// </summary>
		public RedisClientConfiguration()
		{
			_socketPool = new SocketPoolConfiguration();
			_authentication = new AuthenticationConfiguration();
            _servers = new List<IEndPointConfiguration>();
		}

		/// <summary>
		/// Gets a list of <see cref="T:Guanima.Redis.Configuration.IEndPointConfiguration"/> each representing a Redis server in the pool.
		/// </summary>
		public IList<IEndPointConfiguration> Servers
		{
			get { return _servers; }
		}

		/// <summary>
		/// Gets the configuration of the socket pool.
		/// </summary>
		public ISocketPoolConfiguration SocketPool
		{
			get { return _socketPool; }
		}

		/// <summary>
		/// Gets the _authentication settings.
		/// </summary>
		public IAuthenticationConfiguration Authentication
		{
			get { return _authentication; }
		}

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisKeyTransformer"/> which will be used to convert item keys for Redis.
		/// </summary>
		public Type KeyTransformer
		{
			get { return _keyTransformer; }
			set
			{
				ConfigurationHelper.CheckForInterface(value, typeof(IRedisKeyTransformer));

				_keyTransformer = value;
			}
		}

        /// <summary>
        /// Gets or sets the type of the <see cref="T:Guanima.Redis.IKeyTagExtractor"/> which will be used to extract the key tag from a key.
        /// </summary>
        public Type KeyTagExtractor
        {
            get { return _keyTagExtractor; }
            set
            {
                ConfigurationHelper.CheckForInterface(value, typeof(IKeyTagExtractor));

                _keyTagExtractor = value;
            }
        }

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisNodeLocator"/> which will be used to assign items to Redis nodes.
		/// </summary>
		public Type NodeLocator
		{
			get { return _nodeLocator; }
			set
			{
				ConfigurationHelper.CheckForInterface(value, typeof(IRedisNodeLocator));

				_nodeLocator = value;
			}
		}

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.Transcoders.ITranscoder"/> which will be used serialize or deserialize items.
		/// </summary>
		public Type Transcoder
		{
			get { return _transcoder; }
			set
			{
				ConfigurationHelper.CheckForInterface(value, typeof(ITranscoder));

				_transcoder = value;
			}
		}


		#region [ IRedisClientConfiguration]

        IList<IEndPointConfiguration> IRedisClientConfiguration.Servers
	    {
            get { return _servers; }
	    }


		ISocketPoolConfiguration IRedisClientConfiguration.SocketPool
		{
			get { return SocketPool; }
		}

        //IAuthenticationConfiguration IRedisClientConfiguration.Authentication
        //{
        //    get { return _authentication; }
        //}

		Type IRedisClientConfiguration.KeyTransformer
		{
			get { return this.KeyTransformer; }
			set { this.KeyTransformer = value; }
		}

        Type IRedisClientConfiguration.KeyTagExtractor
        {
            get { return this.KeyTagExtractor; }
            set { this.KeyTagExtractor = value; }
        }

		Type IRedisClientConfiguration.NodeLocator
		{
			get { return NodeLocator; }
			set { NodeLocator = value; }
		}

		Type IRedisClientConfiguration.Transcoder
		{
			get { return this.Transcoder; }
			set { this.Transcoder = value; }
		}

		#endregion

        #region IRedisClientConfiguration Members


        public int DefaultDB
        {
            get { return _defaultDb; }
            set { _defaultDb = value; }
        }

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