using System;
using System.Collections.Generic;

namespace Guanima.Redis.NodeLocators
{
    /// <summary>
    /// This is a simple _node locator with no computation overhead, always returns the first server from the list. Use only in single server deployments.
    /// </summary>
    public sealed class SingleNodeLocator : IRedisNodeLocator
    {
        private IRedisNode _node;
        private bool _isInitialized;
        private IKeyTagExtractor _keyTagExtractor;
        private readonly object _initLock = new Object();

        void IRedisNodeLocator.Initialize(IList<IRedisNode> nodes)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Instance is already initialized.");

            // locking on this is rude but easy
            lock (_initLock)
            {
                if (_isInitialized)
                    throw new InvalidOperationException("Instance is already initialized.");

                if (nodes.Count > 0)
                    _node = nodes[0];

                _isInitialized = true;
            }
        }

        public IKeyTagExtractor KeyTagExtractor
        {
            get 
            {
                return _keyTagExtractor ?? (_keyTagExtractor = new DefaultKeyTagExtractor());
            }
            set { _keyTagExtractor = value; }
        }

        IRedisNode IRedisNodeLocator.Locate(string key)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must call Initialize first");

            return _node;
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