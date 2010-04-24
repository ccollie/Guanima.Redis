using System;
using System.Runtime.Serialization;

namespace Guanima.Redis
{
	/// <summary>
	/// The exception that is thrown when an unknown error occures in the <see cref="T:Guanima.Redis.RedisClient"/>
	/// </summary>
	[Serializable]
	public class RedisException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class.
		/// </summary>
		public RedisException() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with a specified error message.
		/// </summary>
		public RedisException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public RedisException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with serialized data.
		/// </summary>
		protected RedisException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}


    /// <summary>
    /// The exception that is thrown when an unknown error occures in the <see cref="T:Guanima.Redis.RedisClient"/>
    /// </summary>
    [Serializable]
    public class RedisAuthenticationException : RedisException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class.
        /// </summary>
        public RedisAuthenticationException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with a specified error message.
        /// </summary>
        public RedisAuthenticationException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public RedisAuthenticationException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with serialized data.
        /// </summary>
        protected RedisAuthenticationException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }


    /// <summary>
    /// The exception that is thrown when a timeout occurs when trying to acquire a lock/>
    /// </summary>
    [Serializable]
    public class RedisLockTimeoutException : RedisException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class.
        /// </summary>
        public RedisLockTimeoutException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with a specified error message.
        /// </summary>
        public RedisLockTimeoutException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public RedisLockTimeoutException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisAuthenticationException"/> class with serialized data.
        /// </summary>
        protected RedisLockTimeoutException(
          SerializationInfo info,
          StreamingContext context)
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