using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    public class HashTests : BaseRedisClientTests
    {

        [Test]
        public void Test_HSet_HLen_Big_Hash_Creation() 
	    {
		    var bigHash = new Dictionary<String,String>();
            for (var i = 0; i < 1024; i++)
            {
                var key = RandString(0, 8);
                var val = RandString(0, 8);
                if (bigHash.ContainsKey(key))
                {
                    i--;
                    continue;
                }
                r.HSet("bighash", key, val);
                bigHash[key] = val;
            }
            Assert.AreEqual(r.HLen("bighash"), 1024);
        } 

     
        [Test]
        public void Test_HIncrBy_Against_A_Non_Existing_Key()
	    {
            Assert.AreEqual(2, r.HIncrBy("smallhash", "tmp", 2));
            Assert.AreEqual(2, r.HGet("smallhash", "tmp"));
            Assert.AreEqual(2, r.HIncrBy("bighash", "tmp", 2));
            Assert.AreEqual(2, r.HGet("bighash", "tmp"));
        } 

        [Test]
        public void TestHSet() 
        {
            Assert.IsTrue(r.HSet("metavars", "foo", "bar"));
            Assert.IsTrue(r.HSet("metavars", "hoge", "piyo"));
            Assert.AreEqual((string)r.HGet("metavars", "foo"), "bar");
            Assert.AreEqual((string)r.HGet("metavars", "hoge"), "piyo");

            Assert.Throws(typeof(RedisException), () => {
                r.Set("test", "foobar");
                r.HSet("test", "hoge", "piyo");
            });
        }
    	
	    [Test]
        public void TestHashGet() 
	    {
            Assert.IsTrue(r.HSet("metavars", "foo", "bar"));
  
            Assert.AreEqual("bar", (string)r.HGet("metavars", "foo"));
            Assert.IsNull((string)r.HGet("metavars", "hoge"));
            Assert.IsNull((string)r.HGet("hashDoesNotExist", "field"));

            Assert.Throws(typeof(RedisException), () => {
                r.RPush("metavars", "foo");
                r.HGet("metavars", "foo");
            });
        }

	    [Test]
        public void TestHashExists() 
        {
            Assert.IsTrue(r.HSet("metavars", "foo", "bar"));
            Assert.IsTrue(r.HExists("metavars", "foo"));
            Assert.IsFalse(r.HExists("metavars", "hoge"));
            Assert.IsFalse(r.HExists("hashDoesNotExist", "field"));
        }

	    [Test]
        public void TestHashDelete() 
	    {
            Assert.IsTrue(r.HSet("metavars", "foo", "bar"));
            Assert.IsTrue(r.HExists("metavars", "foo"));
            Assert.IsTrue(r.HDel("metavars", "foo"));
            Assert.IsFalse(r.HExists("metavars", "foo"));

            Assert.IsFalse(r.HDel("metavars", "hoge"));
            Assert.IsFalse(r.HDel("hashDoesNotExist", "field"));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("foo", "bar");
                r.HDel("foo", "field");
            });
        }

	    [Test]
        public void TestHashLength() 
	    {
            Assert.IsTrue(r.HSet("metavars", "foo", "bar"));
            Assert.IsTrue(r.HSet("metavars", "hoge", "piyo"));
            Assert.IsTrue(r.HSet("metavars", "foofoo", "barbar"));
            Assert.IsTrue(r.HSet("metavars", "hogehoge", "piyopiyo"));

            Assert.AreEqual(4, r.HLen("metavars"));
            Assert.AreEqual(0, r.HLen("hashDoesNotExist"));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("foo", "bar");
                r.HLen("foo");
            });
        }

	    [Test]
        public void TestHasHSetPreserve() 
	    {
            Assert.IsTrue(r.HSetNx("metavars", "foo", "bar"));
            Assert.IsFalse(r.HSetNx("metavars", "foo", "barbar"));
            Assert.AreEqual("bar", r.HGet("metavars", "foo"));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("test", "foobar");
                r.HSetNx("test", "hoge", "piyo");
            });
        }

        [Test]
        public void TestHasHSetAndGetMultiple()
        {
            var hashKVs = new Dictionary<string, RedisValue>
            {
                {"foo", "bar"}, 
                {"hoge", "piyo"}
            };

            // key=>value pairs via array instance
            r.HMSet("metavars", hashKVs);
            var keys = hashKVs.Keys;
            var multiRet = r.HMGet("metavars", keys);
            Assert.NotNull(multiRet);
            Assert.AreEqual(keys.Count, multiRet.Length);

            int i = 0;
            foreach (var k in keys)
            {
                var retval =   multiRet[i++];
                var expected = hashKVs[k];

                // Apparently, this does not call Equals(), so it blows up
                //Assert.AreEqual(expected, retval);
                Assert.IsTrue(retval == expected);

            }
            
            // key=>value pairs via function arguments
            r.Del("metavars");
            r.HMSet("metavars", hashKVs);
            Assert.AreEqual(new RedisValue[]{"bar", "piyo"}, r.HMGet("metavars", "foo", "hoge"));
        }

	    [Test]
        public void TestHashIncrementBy() {
            // test subsequent increment commands
            Assert.AreEqual(10, r.HIncrBy("hash", "counter", 10));
            Assert.AreEqual(20, r.HIncrBy("hash", "counter", 10));
            Assert.AreEqual(0, r.HIncrBy("hash", "counter", -20));

            Assert.IsTrue(r.HSet("hash", "field", "stringvalue"));
            Assert.AreEqual(10, r.HIncrBy("hash", "field", 10));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("foo", "bar");
                r.HIncrBy("foo", "bar", 1);
            });
        }

	    [Test]
        public void TestHashKeys() 
	    {
            var hashKVs = new Dictionary<string, RedisValue>
            {
                {"foo", "bar"}, 
                {"hoge", "piyo"}
            };
	        var emptyDict = new Dictionary<string, RedisValue>();

            r.HMSet("metavars", hashKVs);

            Assert.AreEqual(hashKVs.Keys, r.HKeys("metavars"));
            Assert.AreEqual(emptyDict, r.HKeys("hashDoesNotExist"));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("foo", "bar");
                r.HKeys("foo");
            });
        }

	    [Test]
        public void TestHashValues() 
	    {
            var hashKVs = new Dictionary<string, RedisValue>
                              {
                {"foo", "bar"}, 
                {"hoge", "piyo"}
            };

            r.HMSet("metavars", hashKVs);

            Assert.AreEqual(hashKVs.Values, r.HVals("metavars"));
            Assert.AreEqual(new RedisValue[]{}, r.HVals("hashDoesNotExist"));

            Assert.Throws(typeof(RedisException), () => {
                r["foo"] = "bar";
                r.HVals("foo");
            });
        }

	    [Test]
        public void TestHashGetAll() 
	    {
            var hashKVs = new Dictionary<string, RedisValue>
            {
                {"foo", "bar"}, 
                {"hoge", "piyo"}
            };
            r.HMSet("metavars", hashKVs);

            Assert.AreEqual(hashKVs, r.HGetAll("metavars"));
            Assert.AreEqual(new Dictionary<string, RedisValue>(), r.HGetAll("hashDoesNotExist"));

            Assert.Throws(typeof(RedisException), () => {
                r.Set("foo", "bar");
                r.HGetAll("foo");
            });
        }

    }
}
