using System;
using System.Collections.Generic;

namespace Guanima.Redis.Configuration
{
	/// <summary>
	/// Defines an interface for configuring the <see cref="T:Guanima.Redis.RedisClient"/>.
	/// </summary>
	public interface IRedisClientConfiguration
	{
        /// <summary>
        /// Gets a list of <see cref="T:Guanima.Redis.Configuration.IEndPointConfiguration"/> each representing a Redis server in the pool.
        /// </summary>
        IList<IEndPointConfiguration> Servers { get; }

		/// <summary>
		/// Gets the configuration of the socket pool.
		/// </summary>
		ISocketPoolConfiguration SocketPool { get; }

	    int DefaultDB { get;}
		
        /// <summary>
		/// Gets the authentication settings.
		/// </summary>
		//IAuthenticationConfiguration Authentication { get; }

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisKeyTransformer"/> which will be used to convert item keys for Redis.
		/// </summary>
		Type KeyTransformer { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="T:Guanima.Redis.IKeyTagExtractor"/> which will be extract the hashable portion of a key.
        /// </summary>
        Type KeyTagExtractor { get; set; }


		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisNodeLocator"/> which will be used to assign items to Redis nodes.
		/// </summary>
		Type NodeLocator { get; set; }

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.Transcoders.ITranscoder"/> which will be used serialzie or deserialize items.
		/// </summary>
		Type Transcoder { get; set; }
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