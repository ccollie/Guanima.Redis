using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Guanima.Redis.Utils;

namespace Guanima.Redis.NodeLocators
{
    /// <summary>
    /// Implements Ketama cosistent hashing, compatible with the "spymemcached" Java client
    /// </summary>
    public sealed class KetamaNodeLocator : IRedisNodeLocator
    {
        private const int ServerAddressMutations = 160;

        private List<IRedisNode> _servers;

        // holds all server keys for mapping an item key to the server consistently
        private List<uint> _keys;
        // used to lookup a server based on its key
        private Dictionary<uint, IRedisNode> _keyToServer;

        private bool _isInitialized;

        // TODO make this configurable without restructuring the whole config system
        private const string HashName = "System.Security.Cryptography.MD5";

        void IRedisNodeLocator.Initialize(IList<IRedisNode> nodes)
        {
            if (_isInitialized) throw new InvalidOperationException("Instance is already initialized.");

            // sizeof(uint)
            const int KeyLength = 4;
            var hashAlgo = HashAlgorithm.Create(HashName);

            int partCount = hashAlgo.HashSize / 8 / KeyLength; // HashSize is in bits, uint is 4 bytes long
            if (partCount < 1) throw new ArgumentOutOfRangeException("The hash algorithm must provide at least 32 bits long hashes");

            var keys = new List<uint>(nodes.Count * ServerAddressMutations);
            var keyToServer = new Dictionary<uint, IRedisNode>(keys.Count, new UIntEqualityComparer());

            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                var currentNode = nodes[nodeIndex];

                // every server is registered numberOfKeys times
                // using UInt32s generated from the different parts of the hash
                // i.e. hash is 64 bit:
                // 01 02 03 04 05 06 07
                // server will be stored with keys 0x07060504 & 0x03020100
                string address = currentNode.EndPoint.ToString();

                for (int mutation = 0; mutation < ServerAddressMutations / partCount; mutation++)
                {
                    byte[] data = hashAlgo.ComputeHash(Encoding.ASCII.GetBytes(address + "-" + mutation));

                    for (int p = 0; p < partCount; p++)
                    {
                        var tmp = p * 4;
                        var key = ((uint)data[tmp + 3] << 24)
                                  | ((uint)data[tmp + 2] << 16)
                                  | ((uint)data[tmp + 1] << 8)
                                  | ((uint)data[tmp]);

                        keys.Add(key);
                        keyToServer[key] = currentNode;
                    }
                }
            }

            keys.Sort();

            _keys = keys;
            _keyToServer = keyToServer;

            _servers = new List<IRedisNode>();
            _servers.AddRange(nodes);

            _isInitialized = true;
        }

        public IKeyTagExtractor KeyTagExtractor
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private static uint GetKeyHash(string key)
        {
            var hashAlgo = HashAlgorithm.Create(HashName);
            var data = hashAlgo.ComputeHash(Encoding.UTF8.GetBytes(key));

            return ((uint)data[3] << 24) | ((uint)data[2] << 16) | ((uint)data[1] << 8) | ((uint)data[0]);
        }

        IRedisNode IRedisNodeLocator.Locate(string key)
        {
            if (!_isInitialized) throw new InvalidOperationException("You must call Initialize first");
            if (key == null) throw new ArgumentNullException("key");
            if (_servers.Count == 0) return null;
            if (_servers.Count == 1) return _servers[0];

            var retval = LocateNode(GetKeyHash(key));

            // isalive is not atomic
            if (!retval.IsAlive)
            {
                for (var i = 0; i < _servers.Count; i++)
                {
                    // -- this is from spyRedis so we select the same node for the same items
                    ulong tmpKey = (ulong)GetKeyHash(i + key);
                    tmpKey += (uint)(tmpKey ^ (tmpKey >> 32));
                    tmpKey &= 0xffffffffL; /* truncate to 32-bits */
                    // -- end

                    retval = LocateNode((uint)tmpKey);

                    if (retval.IsAlive) return retval;
                }
            }

            return retval.IsAlive ? retval : null;
        }

        private IRedisNode LocateNode(uint itemKeyHash)
        {
            // get the index of the server assigned to this hash
            int foundIndex = _keys.BinarySearch(itemKeyHash);

            // no exact match
            if (foundIndex < 0)
            {
                // this is the nearest server in the list
                foundIndex = ~foundIndex;

                if (foundIndex == 0)
                {
                    // it's smaller than everything, so use the last server (with the highest key)
                    foundIndex = _keys.Count - 1;
                }
                else if (foundIndex >= _keys.Count)
                {
                    // the key was larger than all server keys, so return the first server
                    foundIndex = 0;
                }
            }

            if (foundIndex < 0 || foundIndex > _keys.Count)
                return null;

            return _keyToServer[_keys[foundIndex]];
        }
    }
}