using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Guanima.Redis.Configuration;
using Guanima.Redis.Extensions;
using Guanima.Redis.KeyTransformers;
using Guanima.Redis.NodeLocators;
using Guanima.Redis.Transcoders;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class RedisClientTestFixture
    {
        public const int TestDb = 9;
        protected RedisClient r;

        [SetUp]
        public virtual void Setup()
        {
            r = CreateClient();
            r.FlushDB();
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (r != null)
                ((IDisposable)r).Dispose();
        }


        protected virtual RedisClient CreateClient()
        {
            var client = GetSingleNodeClient();
            return client;
        }

        protected virtual RedisClient GetSingleNodeClient()
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
            mcc.DefaultDB = 9;
            return new RedisClient(mcc);
        }

        protected static IEnumerable<int> GetArrayOfNumbers() 
        {
            return new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        }

        protected static IEnumerable<KeyValuePair<string,string>> GetKeyValueArray() 
        {
            return new Dictionary<string,string>()
            {
                {"foo","bar"},
                {"cheddar", "cheese"},
                {"beef", "wellington"},
                {"hoge", "piyo"},
                {"hummus", "veggies"}
            };
        }

        protected static IEnumerable<KeyValuePair<string,string>> GetNamespacedKeyValueArray() 
        {
            return new Dictionary<string, string>()
            {
                {"france:cheese", "brie"},
                {"usa:cheese", "vermont cheddar"},
                {"italy:cheese", "mozarella"},
                {"england:cheese", "head"}
            };
        }

        public static IDictionary<String,double> GetZSetValues() 
        {
            return new Dictionary<string, double>()
            {
                {"a", -10},
                {"b", 0}, 
                {"c", 10}, 
                {"d", 20}, 
                {"e", 20}, 
                {"f", 30}
            };
        }
        
        public string RandString(int min, int max)
        {
            return TestHelpers.RandString(min, max, TestHelpers.RandStringType.Alpha);    
        }

        public string RandString(int min, int max, TestHelpers.RandStringType type)
        {
            return TestHelpers.RandString(min, max, type);
        }

        protected static void AssertListsAreEqual(IEnumerable<String> actualList, IEnumerable<String> expectedList)
        {
            if (actualList == null && expectedList == null)
                return;

            Assert.IsFalse(actualList == null || expectedList == null);

            Assert.AreEqual(actualList.Count(), expectedList.Count());
            var expected = expectedList.ToArray();
            var actual = actualList.ToArray();
            Assert.AreEqual(actual, expected);
        }

        protected static void AssertListsAreEqual(IEnumerable<String> actualList, List<String> expectedList)
        {
            Assert.AreEqual(actualList.Count(), expectedList.Count);
            var i = 0;
            actualList.ForEach(x => Assert.AreEqual(x, expectedList[i++]));           
        }

        protected static void AssertAreEqual(List<String> expectedList, List<String> actualList)
        {
            Assert.AreEqual(actualList.Count, expectedList.Count);
            var i = 0;
            actualList.ForEach(x => Assert.AreEqual(x, expectedList[i++]));
        }

        protected static void AssertAreEqual(byte[][] actualList, IEnumerable<String> expectedList)
        {
            if (actualList == null && expectedList == null)
                return;

            Assert.IsFalse(actualList == null || expectedList == null);
               
            var actual = actualList.ToStringList();
            var expected = expectedList.ToList();

            Assert.AreEqual(actual.Count, expected.Count);
            var i = 0;
            actual.ForEach(x => Assert.AreEqual(x, expected[i++]));
        }

        protected static void AssertListsAreEqual(RedisValue value, params string[] expected)
        {
            var items = new List<String>(expected);
            if (value.Type != RedisValueType.MultiBulk)
                throw new ArgumentException("Expected a multi-bulk response. Got : " + value);
            var actual = new List<string>();
            foreach (var redisValue in value)
            {
                actual.Add(redisValue);
            }
            AssertAreEqual(actual, items);
        }

        protected static void AssertListsAreEqual(RedisValue[] values, params string[] expected)
        {
            var items = new List<String>(expected);
            Assert.False(values == null);
            var actual = new List<string>();
            foreach (var redisValue in values)
            {
                actual.Add(redisValue);
            }
            Assert.AreEqual(items, actual);
        }

        protected static void AssertEquals(IEnumerable<long> expected, RedisValue value)
        {
            var items = new List<long>(expected);
            if (value.Type != RedisValueType.MultiBulk)
                throw new ArgumentException("Expected a multi-bulk response. Got : " + value);
            var actual = new List<long>();
            foreach (var redisValue in value)
            {
                actual.Add(redisValue);
            }
            Assert.AreEqual(actual, items);
        }

        protected static void AssertListsAreEqual(byte[][] actualList, params string[] expected)
        {
            var items = new List<String>(expected);
            AssertAreEqual(actualList, items);
        }


        protected static void AssertStringsAreEqual(String first, byte[] second)
        {
            if (first == null && second == null)
                return;
            var temp = first.ToUtf8ByteArray();
            Assert.AreEqual(temp, second);
        }

        protected static void AssertStringsAreEqual(byte[] first, String second)
        {
            AssertStringsAreEqual(second, first);
        }
    }
}