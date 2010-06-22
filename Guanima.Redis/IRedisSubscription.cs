using System;
using System.Collections.Generic;

namespace Guanima.Redis
{
    public interface IRedisSubscription : IDisposable
    {
        /// <summary>
        /// The number of active subscriptions this client has
        /// </summary>
        int SubscriptionCount { get; }

        /// <summary>
        /// Registered handler called after client *Subscribes* to each new channel
        /// </summary>
        Action<string> OnSubscribe { get; set; }

        /// <summary>
        /// Registered handler called when each message is received
        /// </summary>
        Action<string, byte[]> OnMessage { get; set; }

        /// <summary>
        /// Registered handler called when each channel is unsubscribed
        /// </summary>
        Action<string> OnUnSubscribe { get; set; }

        /// <summary>
        /// Exception handler
        /// </summary>
        Action<Exception> OnException { get; set; }

        /// <summary>
        /// Subscribe to channels by name
        /// </summary>
        /// <param name="channels"></param>
        void Subscribe(IEnumerable<string> channels);
        void Subscribe(params string[] channels);


        void UnSubscribeAll();
        void UnSubscribe(params string[] channels);
        void UnSubscribe(IEnumerable<string> channels);
    }
}