using System;
using System.Collections.Generic;
using Guanima.Redis.Transcoders;

namespace Guanima.Redis
{
    /// <summary>
    /// Provides custom server pool implementations
    /// </summary>
    public interface IServerPool : IDisposable
    {
        ITranscoder Transcoder { get; }
        IRedisNodeLocator NodeLocator { get; }
        IRedisKeyTransformer KeyTransformer { get; }

        PooledSocket Acquire(string key);
        IEnumerable<IRedisNode> GetServers();

        IAuthenticator Authenticator { get; set; }

        void Start();
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