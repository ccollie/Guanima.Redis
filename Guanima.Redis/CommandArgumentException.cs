using System;

namespace Guanima.Redis
{
	/// <summary>
	/// The exception that is thrown when a command argument is invalid.
	/// </summary>
	[Serializable]
	public class CommandArgumentException : RedisClientException
	{
	    private readonly string _paramName = "";

		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.CommandArgumentException"/> class.
		/// </summary>
		public CommandArgumentException() { }

        public CommandArgumentException(string paramName)
        {
            _paramName = paramName;
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.CommandArgumentException"/> class with a specified error message.
		/// </summary>
		public CommandArgumentException(string paramName, string message) 
            : base(message)
		{
		    _paramName = paramName;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.CommandArgumentException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		public CommandArgumentException(string paramName, string message, Exception inner) 
            : base(message, inner)
		{
		    _paramName = paramName;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="T:Guanima.Redis.CommandArgumentException"/> class with serialized data.
		/// </summary>
        protected CommandArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }


        public string ParamName
        {
            get { return _paramName; }
        }
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