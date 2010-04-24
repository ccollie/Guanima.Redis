using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Guanima.Redis.Configuration;
using Guanima.Redis.KeyTransformers;
using Guanima.Redis.NodeLocators;
using Guanima.Redis.Transcoders;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class RedisClientTest
    {
        public const string TestObjectKey = "Hello_World";

        protected virtual RedisClient GetClient()
        {
            return GetSingleNodeClient();
        }

        protected RedisClient GetSingleNodeClient()
        {
            // try to hit all lines in the config classes
            var mcc = new RedisClientConfiguration();
            var server = new EndPointConfiguration(new IPEndPoint(IPAddress.Loopback, 6379));
            mcc.Servers.Add(server);

            mcc.NodeLocator = typeof(SingleNodeLocator);
            mcc.KeyTransformer = typeof(DefaultKeyTransformer);
            mcc.Transcoder = typeof(DefaultTranscoder);

            mcc.SocketPool.MinPoolSize = 10;
            mcc.SocketPool.MaxPoolSize = 100;
            mcc.SocketPool.ConnectionTimeout = new TimeSpan(0, 0, 10);
            mcc.SocketPool.DeadTimeout = new TimeSpan(0, 0, 30);
            return new RedisClient(mcc);
        }

        /// <summary>
        /// Tests if the client can initialize itself from enyim.com/Redis
        /// </summary>
        [Test]
        public void DefaultConfigurationTest()
        {
            using (new RedisClient()) ;
        }

        /// <summary>
        /// Tests if the client can initialize itself from a specific config
        /// </summary>
        [Test]
        public void NamedConfigurationTest()
        {
            using (new RedisClient("test/validConfig")) ;
        }

        /// <summary>
        /// Tests if the client can handle an invalid configuration
        /// </summary>
        [Test]
        public void InvalidConfigurationTest()
        {
            try
            {
                using (var client = new RedisClient("test/invalidConfig"))
                {
                    Assert.IsFalse(false, ".ctor should have failed.");
                }
            }
            catch
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Tests if the client can be declaratively initialized
        /// </summary>
        [Test]
        public void ProgrammaticConfigurationTest()
        {
            // try to hit all lines in the config classes
            var mcc = new RedisClientConfiguration();

            mcc.Servers.Add(new EndPointConfiguration(new IPEndPoint(IPAddress.Loopback, 20000)));
            mcc.Servers.Add(new EndPointConfiguration(new IPEndPoint(IPAddress.Loopback, 20002)));

            mcc.NodeLocator = typeof(DefaultNodeLocator);
            mcc.KeyTransformer = typeof(SHA1KeyTransformer);
            mcc.Transcoder = typeof(DefaultTranscoder);

            mcc.SocketPool.MinPoolSize = 10;
            mcc.SocketPool.MaxPoolSize = 100;
            mcc.SocketPool.ConnectionTimeout = new TimeSpan(0, 0, 10);
            mcc.SocketPool.DeadTimeout = new TimeSpan(0, 0, 30);

            using (new RedisClient(mcc)) ;
        }

   

	
      
        [Test]
        public void SortTest()
        {
            var client = GetClient();
            client.Del("INTLIST");
            int i;
            for (i = 1; i <= 100; i++)
            {
                client.LPush("INTLIST", i);
            }
            RedisValue sorted = client.Sort("INTLIST", null, false, true, 0, 10);
            Assert.AreEqual(sorted.MultiBulkValues.Length, 10);
            List<string> asList = sorted;
            i = 100;
            foreach (var item in asList)
            {
                Assert.AreEqual(item, i.ToString());
                i--;
            }
        }

        //[TestCase]
        //public void MultiGetTest()
        //{
        //    // note, this test will fail, if Redis version is < 1.2.4
        //    RedisClient client = new RedisClient();

        //    List<string> keys = new List<string>();

        //    for (int i = 0; i < 100; i++)
        //    {
        //        string k = "multi_get_test_" + i;
        //        keys.Add(k);

        //        client[k] = i;
        //    }

        //    IDictionary<string, ulong> cas;
        //    var retvals = client.MGet(keys);

        //    Assert.AreEqual<int>(100, retvals.Count, "MultiGet should have returned 100 items.");

        //    object value;

        //    for (int i = 0; i < retvals.Count; i++)
        //    {
        //        string key = "multi_get_test_" + i;

        //        Assert.IsTrue(retvals.TryGetValue(key, out value), "missing key: " + key);
        //        Assert.AreEqual(value, i, "Invalid value returned: " + value);
        //    }
        //}

    }
}