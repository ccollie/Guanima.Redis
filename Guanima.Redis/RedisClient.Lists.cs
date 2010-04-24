using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.Lists;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        const int FirstElement = 0;
        const int LastElement = -1;

        public RedisValue BLPop(IEnumerable<string> keys, TimeSpan timeout)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            var transformedKeys = TransformKeys(keys);
            IRedisNode prevNode = null, node = null;
            foreach (var key in keys)
            {
                if (prevNode != node)
                {
                    CannotCluster("BLPOP");
                }
                prevNode = node;
                node = GetNodeForTransformedKey(key);
            }
            if (node == null)
                throw new CommandArgumentException("keys", "BLPOP requires at least 1 key.");
            var command = new BLPopCommand(transformedKeys, timeout);
            return ExecValue(node, command);
        }

        public RedisValue BRPop(IEnumerable<string> keys, TimeSpan timeout)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            var transformedKeys = TransformKeys(keys);
            IRedisNode prevNode = null, node = null;
            foreach (var key in keys)
            {
                if (prevNode != node)
                {
                    CannotCluster("BLPOP");
                }
                prevNode = node;
                node = GetNodeForTransformedKey(key);
            }
            if (node == null)
                throw new CommandArgumentException("keys", "BRPOP requires at least 1 key.");
            var command = new BRPopCommand(transformedKeys, timeout);
            return ExecValue(node, command);
        }


        public RedisValue LIndex(string key, int index)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new LIndexCommand(transformedKey, index));
        }

        public int LLen(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new LLenCommand(transformedKey));
        }

        public RedisValue LPop(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new LPopCommand(transformedKey));           
        }


        public int LPush(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new LPushCommand(transformedKey, value));
        }


        public RedisValue LRange(string key, int start, int end)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new LRangeCommand(transformedKey, start, end));
        }

        public RedisValue ListGetAll(string key)
        {
            var transformedKey = TransformKey(key);
            return LRange(transformedKey, FirstElement, LastElement);
        }

        public int LRem(string key, int count, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new LRemCommand(transformedKey, count, value));
        }


        public RedisClient LSet(string key, int index, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            Execute(transformedKey, new LSetCommand(transformedKey, index, value));
            return this;
        }


        public RedisClient LTrim(string key, int start, int end)
        {
            var transformedKey = TransformKey(key);
            Execute(transformedKey, new LTrimCommand(transformedKey, start, end));
            return this;
        }

        public RedisValue RPop(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecValue(transformedKey, new RPopCommand(transformedKey));
        }

        public RedisValue RPopLPush(string srcKey, string dstKey)
        {
            var transformedSrcKey = TransformKey(srcKey);
            var transformedDstKey = TransformKey(dstKey);
  
            // keys can possibly map to multiple nodes....
            var node1 = GetNodeForTransformedKey(transformedSrcKey);
            var node2 = GetNodeForTransformedKey(transformedDstKey);
            if (node2 == node1)
            {
                var command = new RPopLPushCommand(transformedSrcKey, transformedDstKey);
                return ExecValue(transformedSrcKey, command);
            }
            CannotCluster("RPOPLPUSH");
            return RedisValue.Empty;
        }

        public int RPush(string key, RedisValue value)
        {
            var transformedKey = TransformKey(key);
            return ExecuteInt(transformedKey, new RPushCommand(transformedKey, value));
        }

    }
}
