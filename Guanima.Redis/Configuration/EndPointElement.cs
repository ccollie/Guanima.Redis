using System;
using System.Configuration;
using System.Net;
using Guanima.Redis.Utils;

namespace Guanima.Redis.Configuration
{
	/// <summary>
	/// Represents a configuration element that contains a Redis node address. This class cannot be inherited. 
	/// </summary>
	public sealed class EndPointElement : ConfigurationElement
	{
	    public const int RedisDefaultPort = 6379;

		private IPEndPoint _endpoint;

	    /// <summary>
		/// Gets or sets the ip address of the node.
		/// </summary>
		[ConfigurationProperty("address", IsRequired = true, IsKey = true, DefaultValue = "localhost")]
		public string Address
		{
			get { return (string)base["address"]; }
			set { base["address"] = value; }
		}

		/// <summary>
		/// Gets or sets the port of the node.
		/// </summary>
        [ConfigurationProperty("port", IsRequired = true, IsKey = true, DefaultValue = 6379), IntegerValidator(MinValue = 0, MaxValue = 65535)]
		public int Port
		{
			get { return (int)base["port"]; }
			set { base["port"] = value; }
		}

        /// <summary>
        /// Gets or sets the alias of the node.
        /// </summary>
        [ConfigurationProperty("alias", IsRequired = false, IsKey = false)]
        public string Alias
        {
            get { return (string)base["alias"]; }
            set { base["alias"] = value; }
        }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false, DefaultValue = null)]
        public string Password
        {
            get
            {
                var pwd = base["password"];
                return (pwd == null) ? null : pwd.ToString();
            }
            set { base["password"] = value; }
        }

        private IPAddress ParseIpAddress(string candidate)
        {
            if (String.IsNullOrEmpty(candidate))
                return null;
            try
            {
                return IPAddress.Parse(candidate);
            } 
            catch
            {
                return null;
            }
        }

		/// <summary>
		/// Gets the <see cref="T:System.Net.IPEndPoint"/> representation of this instance.
		/// </summary>
		public IPEndPoint EndPoint
		{
			get
			{
				if (_endpoint == null)
				{
				   
                    // Check if a numeric ip was specified. If so, skip DNS lookup
				    IPAddress address = null;

                    if (!IPAddress.TryParse(Address, out address) || address == null)
                    {
                        IPHostEntry entry = Dns.GetHostEntry(Address);
                        IPAddress[] list = entry.AddressList;

                        if (list.Length == 0)
                            throw new ConfigurationErrorsException(String.Format("Could not resolve host '{0}'.",
                                                                                 Address));

                        // get the first IPv4 address from the list (not sure how Redis works against ipv6 addresses whihc are not localhost)
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (list[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                address = list[i];
                                break;
                            }
                        }
                    }
				    if (address == null)
						throw new ConfigurationErrorsException(String.Format("Host '{0}' does not have an IPv4 address.", Address));
						
					_endpoint = new IPEndPoint(address, Port);
				}

				return _endpoint;
			}
		}

		#region [ T:IPAddressValidator         ]
		private class IPAddressValidator : ConfigurationValidatorBase
		{
			private IPAddressValidator() { }

			public override bool CanValidate(Type type)
			{
				return (type == typeof(string)) || base.CanValidate(type);
			}

			public override void Validate(object value)
			{
				string address = value as string;

				if (String.IsNullOrEmpty(address))
					return;

				IPAddress tmp;

				if (!IPAddress.TryParse(address, out tmp))
					throw new ConfigurationErrorsException("Invalid address specified: " + address);
			}
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