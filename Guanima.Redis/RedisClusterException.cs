using System;

namespace Guanima.Redis
{
	/// <summary>
	/// The exception that is thrown when a command is not supported in cluster mode.
	/// </summary>
	[Serializable]
	public class RedisClusterException : RedisClientException
	{
		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisClusterException"/> class.
		/// </summary>
		public RedisClusterException() { }
		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisClusterException"/> class with a specified error message.
		/// </summary>
		public RedisClusterException(string message) : base(message) { }
		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisClusterException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public RedisClusterException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisClusterException"/> class with serialized data.
		/// </summary>
        protected RedisClusterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
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