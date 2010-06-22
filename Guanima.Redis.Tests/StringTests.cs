using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class StringTests : RedisClientTestFixture
    {

        [Test]
        public void Can_Set_And_Get_String()
        {
            const string value = "value";
            using (var redis = CreateClient())
            {
                redis["key"] = value;
                string valueString = redis["key"];

                Assert.That(valueString, Is.EqualTo(value));
            }
        }

        [Test]
        public void Can_Set_And_Get_Key_With_Spaces()
        {
            const string key = "key with spaces";
            const string value = "value";
            using (var redis = CreateClient())
            {
                redis.Set(key, value);
                var valueString = (string) redis[key];

                Assert.That(valueString, Is.EqualTo(value));
            }
        }

        [Test]
        public void Can_Set_And_Get_Key_With_All_Byte_Values()
        {
            const string key = "bytesKey";

            var value = new byte[256];
            for (var i = 0; i < value.Length; i++)
            {
                value[i] = (byte)i;
            }

            using (var redis = CreateClient())
            {
                redis[key] = value;
                byte[] resultValue = redis[key];

                Assert.That(resultValue, Is.EquivalentTo(value));
            }
        }

        [Test]
        public void Can_Store_An_Arbitrary_Byte_Array()
        {
            byte[] bigBuffer = new byte[200 * 1024];

            for (int i = 0; i < bigBuffer.Length / 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    bigBuffer[i * 256 + j] = (byte)j;
                }
            }

            using (RedisClient client = CreateClient())
            {
                client.Set("BigBuffer", bigBuffer);

                byte[] bigBuffer2 = client.Get("BigBuffer");

                for (int i = 0; i < bigBuffer.Length / 256; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        if (bigBuffer2[i * 256 + j] != (byte)j)
                        {
                            Assert.AreEqual(j, bigBuffer[i * 256 + j], "Data should be {0} but its {1}");
                            break;
                        }
                    }
                }
            }
        }


        [Test]
        public void GetKeys_Returns_Matching_Collection()
        {
            r.Set("ss-tests:a1", "One");
            r.Set("ss-tests:a2", "One");
            r.Set("ss-tests:b3", "One");

            var matchingKeys = r.GetKeys("ss-tests:a*");

            Assert.That(matchingKeys.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetKeys_On_Non_Existent_Keys_Returns_Empty_Collection()
        {
            using (var redis = CreateClient())
            {
                var matchingKeys = redis.GetKeys("ss-tests:NOTEXISTS");

                Assert.That(matchingKeys.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void TestAppend()
        {
            r.Del("foo"); // shouldnt need this - fix test setup
            Assert.AreEqual(r.Append("foo", "bar"), 3);
            Assert.AreEqual(r.GetString("foo"), "bar");
            Assert.AreEqual(r.Append("foo", "100"), 6);
            Assert.AreEqual(r.GetString("foo"), "bar100");
        }

        [Test]
        public void TestSubstr() 
        {
            r["var"] = "foobar";
            Assert.AreEqual("foo", (string)r.Substr("var", 0, 2));
            Assert.AreEqual("bar", (string)r.Substr("var", 3, 5));
            Assert.AreEqual("bar", (string)r.Substr("var", -3, -1));

            Assert.IsNull(r.Substr("var", 5, 0));

            r["numeric"] = 123456789;
            Assert.AreEqual(12345, r.Substr("numeric", 0, 4));

            Assert.Throws(typeof(RedisClientException), () => {
                r.RPush("metavars", "foo");
                r.Substr("metavars", 0, 3);
            });
        }

        [Test]
        public void TestGetMultiple()
        {
            const string key = "KQU";
            const string key2 = "KEY2";

            r.Set(key, "Value1");
            r.Set(key2, "Value2");
            var result = r.MGet(key, key2);
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual((string)result[0], "Value1");
            Assert.AreEqual((string)result[1], "Value2");
        }

        [Test]
        public void SetNx_Will_Overwrite_Expiring_Key()
        {
            r["x"] = 10;
            r.Expire("x", 10000);
            r.SetNX("x", 20);
            Assert.AreEqual(r["x"], 20);
        }

        #region Inc/Dec

        private const string IncrKey = "_INCR_KEY_";

        [Test]
        public void TestIncr()
        {
            r.Set(IncrKey, 1);
            Assert.AreEqual(r.Incr(IncrKey), 2);
            Assert.AreEqual(r.GetString(IncrKey), "2");
        }

        [Test]
        public void TestIncrBy()
        {
            r.Set(IncrKey, 1);
            Assert.AreEqual(r.Incr(IncrKey, 5), 6);
            Assert.AreEqual(r.GetString(IncrKey), "6");
        }

        [Test]
        public void TestDecr()
        {
            r.Set(IncrKey, 2);
            Assert.AreEqual(r.Decr(IncrKey), 1);
            Assert.AreEqual((long)r[IncrKey], 1);
        }

        [Test]
        public void TestDecrBy()
        {
            r.Set(IncrKey, 6);
            Assert.AreEqual(r.Decr(IncrKey, 5), 1);
            Assert.AreEqual(r[IncrKey], 1);
        }
        #endregion
    }
}