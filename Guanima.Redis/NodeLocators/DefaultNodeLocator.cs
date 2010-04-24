using System;
using System.Collections.Generic;
using System.Text;
using Guanima.Redis.Utils;

namespace Guanima.Redis.NodeLocators
{
    /// <summary>
    /// This is a ketama-like consistent hashing based node locator. Used when no other <see cref="T:Guanima.Redis.IRedisNodeLocator"/> is specified for the pool.
    /// </summary>
    public sealed class DefaultNodeLocator : IRedisNodeLocator
    {
        private const int ServerAddressMutations = 100;

        // holds all server keys for mapping an item key to the server consistently
        private uint[] _keys;
        // used to lookup a server based on its key
        private readonly Dictionary<uint, IRedisNode> _servers = new Dictionary<uint, IRedisNode>(new UIntEqualityComparer());
        private bool _isInitialized;
        private readonly object _initLock = new Object();
        private IKeyTagExtractor _keyTagExtractor;

        public DefaultNodeLocator()
        {
            
        }

        void IRedisNodeLocator.Initialize(IList<IRedisNode> nodes)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Instance is already initialized.");

            // locking on this is rude but easy
            lock (_initLock)
            {
                if (_isInitialized)
                    throw new InvalidOperationException("Instance is already initialized.");

                _keys = new uint[nodes.Count * ServerAddressMutations];

                int nodeIdx = 0;

                foreach (var node in nodes)
                {
                    List<uint> tmpKeys = GenerateKeys(node, ServerAddressMutations);

                    tmpKeys.ForEach(delegate(uint k)
                                        {
                                            _servers[k] = node;
                                        });

                    tmpKeys.CopyTo(_keys, nodeIdx);
                    nodeIdx += ServerAddressMutations;
                }

                Array.Sort(_keys);

                _isInitialized = true;
            }
        }

        public IKeyTagExtractor KeyTagExtractor
        {
            get { return _keyTagExtractor ?? (_keyTagExtractor = new DefaultKeyTagExtractor()); }
            set { _keyTagExtractor = value; }
        }

        IRedisNode IRedisNodeLocator.Locate(string key)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("You must call Initialize first");

            if (key == null)
                throw new ArgumentNullException("key");

            if (_keys.Length == 0)
                return null;

            var hashableKey = KeyTagExtractor.GetKeyTag(key);

            uint itemKeyHash = BitConverter.ToUInt32(new FNV1a().ComputeHash(Encoding.UTF8.GetBytes(hashableKey)), 0);
            // get the index of the server assigned to this hash
            int foundIndex = Array.BinarySearch(_keys, itemKeyHash);

            // no exact match
            if (foundIndex < 0)
            {
                // this is the nearest server in the list
                foundIndex = ~foundIndex;

                if (foundIndex == 0)
                {
                    // it's smaller than everything, so use the last server (with the highest key)
                    foundIndex = _keys.Length - 1;
                }
                else if (foundIndex >= _keys.Length)
                {
                    // the key was larger than all server keys, so return the first server
                    foundIndex = 0;
                }
            }

            if (foundIndex < 0 || foundIndex > _keys.Length)
                return null;

            return _servers[_keys[foundIndex]];
        }

        private static List<uint> GenerateKeys(IRedisNode node, int numberOfKeys)
        {
            const int keyLength = 4;
            const int partCount = 1; // (ModifiedFNV.HashSize / 8) / KeyLength; // HashSize is in bits, uint is 4 byte long

            //if (partCount < 1)
            //    throw new ArgumentOutOfRangeException("The hash algorithm must provide at least 32 bits long hashes");

            var k = new List<uint>(partCount * numberOfKeys);

            // every server is registered numberOfKeys times
            // using UInt32s generated from the different parts of the hash
            // i.e. hash is 64 bit:
            // 00 00 aa bb 00 00 cc dd
            // server will be stored with keys 0x0000aabb & 0x0000ccdd
            // (or a bit differently based on the little/big indianness of the host)
            string address = node.EndPoint.ToString();

            for (int i = 0; i < numberOfKeys; i++)
            {
                byte[] data = new FNV1a().ComputeHash(Encoding.ASCII.GetBytes(String.Concat(address, "-", i)));

                for (int h = 0; h < partCount; h++)
                {
                    k.Add(BitConverter.ToUInt32(data, h * keyLength));
                }
            }

            return k;
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