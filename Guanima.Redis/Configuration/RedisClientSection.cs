using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Web.Configuration;
using Guanima.Redis.Transcoders;

namespace Guanima.Redis.Configuration
{
	/// <summary>
	/// Configures the <see cref="T:Guanima.Redis.RedisClient"/>. This class cannot be inherited.
	/// </summary>
	public sealed class RedisClientSection : ConfigurationSection, IRedisClientConfiguration
	{
	    /// <summary>
		/// Returns a collection of Redis servers which can be used by the client.
		/// </summary>
		[ConfigurationProperty("servers", IsRequired = true)]
		public EndPointElementCollection Servers
		{
			get
			{
			    return (EndPointElementCollection)base["servers"];
			}
		}

        /// <summary>
        /// Gets or sets the default db to connect to
        /// </summary>
        [ConfigurationProperty("defaultDB", IsRequired = false, IsKey = true, DefaultValue = 0), IntegerValidator(MinValue = 0, MaxValue = 65535)]
        public int DefaultDB
        {
            get { return (int)base["defaultDB"]; }
            set { base["defaultDB"] = value; }
        }


		/// <summary>
		/// Gets or sets the configuration of the socket pool.
		/// </summary>
		[ConfigurationProperty("socketPool", IsRequired = false)]
		public SocketPoolElement SocketPool
		{
			get { return (SocketPoolElement)base["socketPool"]; }
			set { base["socketPool"] = value; }
		}

        ///// <summary>
        ///// Gets or sets the configuration of the authenticator.
        ///// </summary>
        //[ConfigurationProperty("authentication", IsRequired = false), InterfaceValidator(typeof(IRedisA))]
        //public AuthenticationElement Authentication
        //{
        //    get { return (AuthenticationElement)base["authentication"]; }
        //    set { base["authentication"] = value; }
        //}

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisKeyTransformer"/> which will be used to convert item keys for Redis.
		/// </summary>
		[ConfigurationProperty("keyTransformer", IsRequired = false), TypeConverter(typeof(TypeNameConverter)), InterfaceValidator(typeof(IRedisKeyTransformer))]
		public Type KeyTransformer
		{
			get { return (Type)base["keyTransformer"]; }
			set { base["keyTransformer"] = value; }
		}

        /// <summary>
        /// Gets or sets the type of the <see cref="T:Guanima.Redis.IKeyTagExtractor"/> which will be used to extract the hashable portion of a key.
        /// </summary>
        [ConfigurationProperty("keyTagExtractor", IsRequired = false), TypeConverter(typeof(TypeNameConverter)), InterfaceValidator(typeof(IKeyTagExtractor))]
        public Type KeyTagExtractor
        {
            get { return (Type)base["keyTagExtractor"]; }
            set { base["keyTagExtractor"] = value; }
        }

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.IRedisNodeLocator"/> which will be used to assign items to Redis nodes.
		/// </summary>
		[ConfigurationProperty("nodeLocator", IsRequired = false), TypeConverter(typeof(TypeNameConverter)), InterfaceValidator(typeof(IRedisNodeLocator))]
		public Type NodeLocator
		{
			get { return (Type)base["nodeLocator"]; }
			set { base["nodeLocator"] = value; }
		}

		/// <summary>
		/// Gets or sets the type of the <see cref="T:Guanima.Redis.ITranscoder"/> which will be used serialzie or deserialize items.
		/// </summary>
		[ConfigurationProperty("transcoder", IsRequired = false), TypeConverter(typeof(TypeNameConverter)), InterfaceValidator(typeof(ITranscoder))]
		public Type Transcoder
		{
			get { return (Type)base["transcoder"]; }
			set { base["transcoder"] = value; }
		}

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		protected override void PostDeserialize()
		{
			var hostingContext = EvaluationContext.HostingContext as WebContext;

			if (hostingContext != null && hostingContext.ApplicationLevel == WebApplicationLevel.BelowApplication)
			{
				throw new InvalidOperationException("The " + SectionInformation.SectionName + " section cannot be defined below the application level.");
			}
		}


        #region [ IRedisClientConfiguration]

        IList<IEndPointConfiguration> IRedisClientConfiguration.Servers
        {
            get { return Servers.ToIEndPointConfigurationCollection(); }
        }


		ISocketPoolConfiguration IRedisClientConfiguration.SocketPool
		{
			get { return SocketPool; }
		}

		Type IRedisClientConfiguration.KeyTransformer
		{
			get { return KeyTransformer; }
			set { KeyTransformer = value; }
		}

		Type IRedisClientConfiguration.NodeLocator
		{
			get { return NodeLocator; }
			set { NodeLocator = value; }
		}

		Type IRedisClientConfiguration.Transcoder
		{
			get { return Transcoder; }
			set { Transcoder = value; }
		}

        /*
		IAuthenticationConfiguration IRedisClientConfiguration.Authentication
		{
			get { return Authentication; }
		}
        */

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