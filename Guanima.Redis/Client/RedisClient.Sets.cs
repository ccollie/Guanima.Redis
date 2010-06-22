using System;
using System.Collections.Generic;
using System.Linq;
using Guanima.Redis.Extensions;
using Guanima.Redis.Commands.Sets;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        public bool SAdd(string key, RedisValue member)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new SAddCommand(transformedKey, member));
        }

        public bool SRem(string key, RedisValue member)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new SRemCommand(transformedKey, member));
        }

        public RedisValue SPop(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new SPopCommand(transformedKey));
        }


        public bool SMove(string srcKey, string dstKey, RedisValue member)
        {
            var transformedSrc = TransformKey(srcKey);
            var transformedDest = TransformKey(dstKey);
            
            var srcNode = GetNodeForTransformedKey(transformedSrc);
            var destNode = GetNodeForTransformedKey(transformedDest);
            if (srcNode == destNode)
            {
                return ExecuteBool(srcNode, new SMoveCommand(transformedSrc, transformedDest, member));              
            } 
            else
            {
                CannotCluster("SMOVE");    
            }
            return false;
        }

        public int SCard(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new SCardCommand(transformedKey));
        }


        public bool SIsMember(string key, RedisValue member)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new SIsMemberCommand(transformedKey, member));
        }

        public RedisValue SMembers(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new SMembersCommand(transformedKey));            
        }

        public RedisValue SRandMember(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new SRandMemberCommand(transformedKey));                        
        }

        public RedisValue SInter(params string[] keys)
        {
            return SInter((IEnumerable<string>) keys);
        }

        public RedisValue SInter(IEnumerable<string> keys)
        {
            var transformed = TransformKeys(keys);

            // map each key to the appropriate server in the pool
            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(transformed);

            // Optimize if we're dealing with a single node
            if (splitKeys.Count == 1)
            {
                var kvp = splitKeys.First();
                var node = kvp.Key;
                var cacheKeys = kvp.Value;
                return ExecValue(node, new SInterCommand(cacheKeys));
            }

            CannotCluster("SINTER");
            var unique = new Dictionary<byte[], bool>();

            int i = 0;
            // send a 'command' to each server
            foreach (var de in splitKeys)
            {
                var nodeItems = ExecValue(de.Key, new SInterCommand(de.Value));
                if (!nodeItems.IsEmpty)
                {
                    //if (i == 0)
                        
                    foreach(var elem in nodeItems)
                    {
                        if (unique.ContainsKey(elem))
                            continue;
                        unique.Add(elem,true);
                    }
                    i++;
                }
            }

            return unique.Keys.ToArray(); 
        }

        public int SInterStore(string destKey, params string[] keys)
        {
            return SInterStore(destKey, (IEnumerable<string>) keys);
        }


        public int SInterStore(string destKey, IEnumerable<string> keys)
        {
            var transformedDestKey = TransformKey(destKey);
            var transformed = TransformKeys(keys);

            // map each key to the appropriate server in the pool
            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(transformed);
            var destNode = GetNodeForTransformedKey(transformedDestKey);

            // Optimize if we're dealing with a single node
            if (splitKeys.Count == 1)
            {
                var kvp = splitKeys.First();
                var node = kvp.Key;
                if (node == destNode)
                {
                    var cacheKeys = kvp.Value;
                    return ExecuteInt(node, new SInterStoreCommand(transformedDestKey, cacheKeys));
                }
            }

            CannotCluster("SINTERSTORE");

            return 0;
        }


        private RedisValue SUnionCommon(IEnumerable<string> keys, string destKey)
        {
            var transformed = TransformKeys(keys);

            if (destKey != null)
            {
                destKey = TransformKey(destKey);
            }

            // map each key to the appropriate server in the pool
            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(transformed);

            // Optimize if we're dealing with a single node
            if (splitKeys.Count == 1)
            {
                var kvp = splitKeys.First();
                var node = kvp.Key;
                var cacheKeys = kvp.Value;
                if (destKey != null)
                {
                    return ExecuteInt(node, new SUnionStoreCommand(destKey, cacheKeys));
                }
                return ExecValue(node, new SUnionCommand(cacheKeys));
            }

            string commandName = (destKey != null) ? "SUNIONSTORE" : "SUNION";
            CannotCluster(commandName);

            return RedisValue.Empty;
        }

        public RedisValue SUnion(params string[] keys)
        {
            return SUnion((IEnumerable<string>)keys);
        }

        public RedisValue SUnion(IEnumerable<string> keys)
        {
            return SUnionCommon(keys, null);
        }

        public int SUnionStore(string destKey, params string[] keys)
        {
            return SUnionStore(destKey, (IEnumerable<string>)keys);
        }

        public int SUnionStore(string destKey, IEnumerable<string> keys)
        {
            if (destKey == null)
                throw new ArgumentNullException("destKey");
            return (int) SUnionCommon(keys, destKey);
        }


        public RedisValue SDiff(params string[] keys)
        {
            return SDiff((IEnumerable<string>)keys);
        }

        public RedisValue SDiff(IEnumerable<string> keys)
        {
            var transformed = TransformKeys(keys);

            // map each key to the appropriate server in the pool
            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(transformed);

            // Optimize if we're dealing with a single node
            if (splitKeys.Count == 1)
            {
                var kvp = splitKeys.First();
                var node = kvp.Key;
                var cacheKeys = kvp.Value;
                return ExecValue(node, new SDiffCommand(cacheKeys));
            }
            CannotCluster("SDIFF");

            return RedisValue.Empty;
        }

        public int SDiffStore(string destKey, params string[] keys)
        {
            return SDiffStore(destKey, (IEnumerable<string>) keys);
        }

        public int SDiffStore(string destKey, IEnumerable<string> keys)
        {
            var transformedKey = TransformKey(destKey);
            var transformedKeys = TransformKeys(keys);

            EnsureNotClustered("SDIFFSTORE", transformedKey, transformedKeys);
   
            return ExecuteInt(transformedKey, new SDiffStoreCommand(transformedKey, transformedKeys));
        }
    }
}
