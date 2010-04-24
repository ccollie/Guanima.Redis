using System;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Generic;

namespace Guanima.Redis.Configuration
{
	/// <summary>
	/// Configures the authentication settings for Redis servers.
	/// </summary>
	public sealed class AuthenticationElement : ConfigurationElement, IAuthenticationConfiguration
	{
		// TODO make this element play nice with the configuratino system (allow saving, etc.)
		private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();


        public AuthenticationElement():base(){}

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IAuthenticator"/> which will be used authehticate the connections to the Redis nodes.
		/// </summary>
		[ConfigurationProperty("password", IsRequired = false), TypeConverter(typeof(String))]
		public String Password
		{
			get { return (String)base["password"]; }
			set { base["password"] = value; }
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
		{
			var property = new ConfigurationProperty(name, typeof(string), value);
			base[property] = value;

			_parameters[name] = value;

			return true;
		}

		#region [ IAuthenticationConfiguration ]

		string IAuthenticationConfiguration.Password
		{
			get { return this.Password; }
			set
			{
				this.Password = value;
			}
		}

		Dictionary<string, object> IAuthenticationConfiguration.Parameters
		{
			// HACK we should return a clone, but i'm lazy now
			get { return _parameters; }
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
