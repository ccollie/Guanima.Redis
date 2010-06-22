using System;
using System.Collections.Generic;
using System.Threading;
using Guanima.Redis.Extensions;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class SimplePubSub : RedisClientTestFixture
    {
        const string ChannelName = "CHANNEL";
        const string MessagePrefix = "MESSAGE ";
        const int PublishMessageCount = 5;


        [Test]
        public void Publish_And_Receive_5_Messages()
        {
            using (var subscription = r.CreateSubscription())
            {
                subscription.OnException = ex =>
                {
                    Console.WriteLine("Caught exception '{0}'", ex);
                };
    
                subscription.OnSubscribe = channel =>
                {
                   Console.WriteLine("Subscribed to '{0}'", channel);
                };

                subscription.OnUnSubscribe = channel =>
                {
                    Console.WriteLine("UnSubscribed from '{0}'", channel);
                };

                subscription.OnMessage = (channel, msg) =>
                {
                    Console.WriteLine("Received '{0}' from channel '{1}'", msg.FromUtf8(), channel);
                };

                subscription.Subscribe(ChannelName); 

                ThreadPool.QueueUserWorkItem(x =>
                                                 {
                                                     Thread.Sleep(200);
                                                     Console.WriteLine("Begin publishing messages...");

                                                     using (var redisPublisher = CreateClient())
                                                     {
                                                         for (var i = 1; i <= PublishMessageCount; i++)
                                                         {
                                                             var message = MessagePrefix + i;
                                                             Console.WriteLine("Publishing '{0}' to '{1}'", message, ChannelName);
                                                             redisPublisher.Publish(ChannelName, message);
                                                         }
                                                     }
                                                 });

                Console.WriteLine("Started Listening On '{0}'", ChannelName);
            }

            Console.WriteLine("EOF");

            Thread.Sleep(5000);

            /*Output: 
            Started Listening On 'CHANNEL'
            Subscribed to 'CHANNEL'
            Begin publishing messages...
            Publishing 'MESSAGE 1' to 'CHANNEL'
            Received 'MESSAGE 1' from channel 'CHANNEL'
            Publishing 'MESSAGE 2' to 'CHANNEL'
            Received 'MESSAGE 2' from channel 'CHANNEL'
            Publishing 'MESSAGE 3' to 'CHANNEL'
            Received 'MESSAGE 3' from channel 'CHANNEL'
            Publishing 'MESSAGE 4' to 'CHANNEL'
            Received 'MESSAGE 4' from channel 'CHANNEL'
            Publishing 'MESSAGE 5' to 'CHANNEL'
            Received 'MESSAGE 5' from channel 'CHANNEL'
            UnSubscribed from 'CHANNEL'
            EOF
             */
        }

        [Test]
        public void Publish_5_Messages_To_3_Clients()
        {
            const int noOfClients = 3;

            var clients = new List<RedisClient>(noOfClients);
            var subscriptions = new List<IRedisSubscription>();
            var counts = new int[noOfClients];

            for (var i = 0; i < noOfClients; i++)
            {
                var clientNo = i+1;
                counts[i] = 0;
                var subscriber = CreateClient();
                clients.Add(subscriber);
                var subscription = subscriber.CreateSubscription();

                 subscriptions.Add(subscription);
                 
                 subscription.OnException = ex =>
                 {
                     Console.WriteLine("Client {0} Caught exception '{1}'", clientNo, ex.Message);
                 };

                 subscription.OnSubscribe = channel =>
                                                {
                                                    Console.WriteLine("Client #{0} Subscribed to '{1}'", clientNo, channel);
                                                };
                 
                 subscription.OnUnSubscribe = channel =>
                                                  {
                                                      Console.WriteLine("Client #{0} UnSubscribed from '{1}'", clientNo, channel);
                                                  };

                 subscription.OnMessage = (channel, msg) =>
                                              {
                                                  Console.WriteLine("Client #{0} Received '{1}' from channel '{2}'",
                                                                    clientNo, msg.FromUtf8(), channel);

                                                 // var index = subscriptions.IndexOf(subscription);
                                                 // if (Interlocked.Increment(ref counts[index]) == PublishMessageCount)
                                                  //{
                                                  //    subscription.UnSubscribe();
                                                  //}
                                              };

                 subscription.Subscribe(ChannelName); 

                 Console.WriteLine("Client #{0} started Listening On '{1}'", clientNo, ChannelName);

            }


            using (var redisClient = CreateClient())
            {
                Console.WriteLine("Begin publishing messages...");

                for (var i = 1; i <= PublishMessageCount; i++)
                {
                    var message = MessagePrefix + i;
                    Console.WriteLine("Publishing '{0}' to '{1}'", message, ChannelName);
                    redisClient.Publish(ChannelName, message);
                }
            }

            Thread.Sleep(5000);

            /*Output:
            Client #1 started Listening On 'CHANNEL'
            Client #2 started Listening On 'CHANNEL'
            Client #1 Subscribed to 'CHANNEL'
            Client #2 Subscribed to 'CHANNEL'
            Client #3 started Listening On 'CHANNEL'
            Client #3 Subscribed to 'CHANNEL'
            Begin publishing messages...
            Publishing 'MESSAGE 1' to 'CHANNEL'
            Client #1 Received 'MESSAGE 1' from channel 'CHANNEL'
            Client #2 Received 'MESSAGE 1' from channel 'CHANNEL'
            Publishing 'MESSAGE 2' to 'CHANNEL'
            Client #1 Received 'MESSAGE 2' from channel 'CHANNEL'
            Client #2 Received 'MESSAGE 2' from channel 'CHANNEL'
            Publishing 'MESSAGE 3' to 'CHANNEL'
            Client #3 Received 'MESSAGE 1' from channel 'CHANNEL'
            Client #3 Received 'MESSAGE 2' from channel 'CHANNEL'
            Client #3 Received 'MESSAGE 3' from channel 'CHANNEL'
            Client #1 Received 'MESSAGE 3' from channel 'CHANNEL'
            Client #2 Received 'MESSAGE 3' from channel 'CHANNEL'
            Publishing 'MESSAGE 4' to 'CHANNEL'
            Client #1 Received 'MESSAGE 4' from channel 'CHANNEL'
            Client #3 Received 'MESSAGE 4' from channel 'CHANNEL'
            Publishing 'MESSAGE 5' to 'CHANNEL'
            Client #1 Received 'MESSAGE 5' from channel 'CHANNEL'
            Client #3 Received 'MESSAGE 5' from channel 'CHANNEL'
            Client #1 UnSubscribed from 'CHANNEL'
            Client #1 EOF
            Client #3 UnSubscribed from 'CHANNEL'
            Client #3 EOF
            Client #2 Received 'MESSAGE 4' from channel 'CHANNEL'
            Client #2 Received 'MESSAGE 5' from channel 'CHANNEL'
            Client #2 UnSubscribed from 'CHANNEL'
            Client #2 EOF
             */
        }
    }
}