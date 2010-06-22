using System;
using System.Collections.Generic;
using System.Linq;
using Guanima.Redis.Client;
using Guanima.Redis.Commands.PubSub;
using Guanima.Redis.Extensions;
using Guanima.Redis.Protocol;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        public int Publish(IRedisNode node, string toChannel, RedisValue message)
        {
            return ExecuteInt(node, new PublishCommand(toChannel, message));   
        }

        public int Publish(String toChannel, RedisValue message)
        {
            int count = 0;
            ForEachServer(node => count+=Publish(node, toChannel, message));
            return count;
        }

        public RedisValue ReceivePublishedMessage(IRedisNode node)
        {
            using (var socket = node.Acquire())
            {
                return socket.ExpectMultiBulkReply();
            }
        }

        internal RedisValue ReceivePublishedMessage(PooledSocket socket, ref string channel)
        {
            byte[][] result = socket.ExpectMultiBulkReply();
            channel = result[1].FromUtf8();
            return result[2];
        }

        public RedisValue Subscribe(IRedisNode node, IEnumerable<String> toChannels)
        {
            if (toChannels.Count() == 0)
                throw new ArgumentNullException("toChannels");
            return ExecValue(node, new SubscribeCommand(toChannels));
        }

        public RedisValue Subscribe(IRedisNode node, params string[] toChannels)
        {
            if (toChannels.Length == 0)
                throw new ArgumentNullException("toChannels");
            return ExecValue(node, new SubscribeCommand(toChannels));
        }

        public RedisValue UnSubscribe(IRedisNode node, params string[] fromChannels)
        {
            return ExecValue(node, new UnsubscribeCommand(fromChannels));
        }

        public RedisValue PSubscribe(IRedisNode node, IEnumerable<String> toChannelsMatchingPatterns)
        {
            if (toChannelsMatchingPatterns.Count() == 0)
                throw new ArgumentNullException("toChannelsMatchingPatterns");
            return ExecValue(node, new PSubscribeCommand(toChannelsMatchingPatterns));
        }

        public RedisValue PSubscribe(IRedisNode node, params string[] toChannelsMatchingPatterns)
        {
            if (toChannelsMatchingPatterns.Length == 0)
                throw new ArgumentNullException("toChannelsMatchingPatterns");
            return ExecValue(node, new PSubscribeCommand(toChannelsMatchingPatterns));
        }

        public RedisValue PUnSubscribe(IRedisNode node, params string[] fromChannelsMatchingPatterns)
        {
            return ExecValue(node, new PUnsubscribeCommand(fromChannelsMatchingPatterns));
        }

        public IRedisSubscription CreateSubscription()
        {
            return new RedisSubscription(this);
        }

    }
}
