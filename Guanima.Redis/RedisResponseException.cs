using System;

namespace Guanima.Redis
{
	/// <summary>
	/// The exception that is thrown when an unknown error occures in the <see cref="T:Guanima.Redis.RedisClient"/>
	/// </summary>
	[Serializable]
	public class RedisResponseException : RedisException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class.
		/// </summary>
		public RedisResponseException():base() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with a specified error message.
		/// </summary>
		public RedisResponseException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public RedisResponseException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Guanima.Redis.RedisException"/> class with serialized data.
		/// </summary>
		protected RedisResponseException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
