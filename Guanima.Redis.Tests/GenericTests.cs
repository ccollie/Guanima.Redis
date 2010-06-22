using System.Threading;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class ClientGenericTests : RedisClientTestFixture
    {
        private const string Key = "_TEST_KEY_";

        #region Select

        [Test]
        public void TestSelect()
        {
            r.Set(Key, "DB0");
            r.Select(1);
            r.Set(Key, "DB1");
            r.Select(0);
            Assert.AreEqual("DB0", r.GetString(Key));
            r.Select(1);
            Assert.AreEqual("DB1", r.GetString(Key));
        }
        #endregion

        #region Exists

        [Test]
        public void Test_Exists()
        {
            r["newkey"] = "test";
            Assert.IsTrue(r.Exists("newkey"));
            r.Del("newkey");
            Assert.IsFalse(r.Exists("newkey"));
        }

        #endregion

        #region Keys
        [Test]
        public void Test_Get_Keys_With_Pattern()
        {
            var keynames = new[] { "key_x", "key_y", "key_z", "foo_a", "foo_b", "foo_c" };
            foreach (var key in keynames)
            {
                r.Set(key, "hello");
            }
            AssertListsAreEqual(r.GetKeys("foo*"), new[] { "foo_a", "foo_b", "foo_c" });
        } 

        #endregion

        #region Set

        [Test]
        public void Test_Get_Set_Keys_In_Different_Databases()
        {	
            r.Select(TestDb);
            r.Set("a", "hello");
            r.Set("b", "world");
            r.Select(10);
            r.Set("a", "foo");
            r.Set("b", "bared");
            r.Select(TestDb);
            Assert.AreEqual(r.GetString("a"), "hello");
            Assert.AreEqual(r.GetString("b"), "world");
            r.Select(10);
            Assert.AreEqual(r.GetString("a"), "foo");
            Assert.AreEqual(r.GetString("b"), "bared");
        } 

        #endregion

        #region Del

        [Test]
        public void Test_Del_Against_A_Single_Item()
        {
            r["x"] = 1;
            r.Del("x");
            var actual = r["x"];
            Assert.IsTrue(actual.IsEmpty);
        }

        [Test]
        public void Test_VarArg_Del()
        {
            r.Set("foo1", "a");
            r.Set("foo2", "b");
            r.Set("foo3", "c");
            var delCount = r.Del("foo1", "foo2", "foo3");
            Assert.AreEqual(3, delCount);
            var items = r.MGet("foo1", "foo2", "foo3");
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual((string)items[0], null);
            Assert.AreEqual((string)items[1], null);
            Assert.AreEqual((string)items[2], null);
        }

        #endregion

        #region Move
        
        [Test]
        public void Test_Move_Basic_Usage() 
        {
            var savedSize = r.DBSize();
          //  r.FlushDB(10);
            r.Select(0);
            r["mykey"] = "foobar";
            r.Move("mykey", 10);
            Assert.IsFalse(r.Exists("mykey"));
            Assert.AreEqual(r.DBSize(), savedSize-1);
            r.Select(10);
            Assert.AreEqual((string)r["mykey"], "foobar");
            Assert.AreEqual(r.DBSize(), 1);
        } 

        #endregion

        #region Info

        [Test]
        public void TestInfo()
        {
            var info = r.Info();
            Assert.IsNotNull(info);
        }
        
        #endregion

        [Test]
        // Rename basic usage
        public void Rename_Basic_Test()
        {
            r.Set("mykey", "hello");
            r.Rename("mykey", "mykey1");
            r.Rename("mykey1", "mykey2");

            Assert.AreEqual((string)r.Get("mykey2"), "hello");
            Assert.IsFalse(r.Exists("mykey"));
        }

        [Test]
        public void Test_RenameNX_Basic_Usage()
        {
            r.Del("mykey");
            r.Del("mykey2");
            r.Set("mykey", "foobar");
            r.RenameNX("mykey", "mykey2");
            var res = (string)r.Get("mykey2");
            Assert.AreEqual("foobar", res);
            Assert.IsFalse(r.Exists("mykey"));
        }

        [Test]
        public void Test_RenameNx_Against_Existing_Key() 
        {
            r.Set("mykey", "foo");
            r.Set("mykey2", "bar");
            Assert.IsFalse(r.RenameNX("mykey", "mykey2"));
        } 

        [Test]
        public void Test_Rename_Against_An_Existing_Key()
        {
            r.Set("mykey", "a");
            r.Set("mykey2", "b");
            r.Rename("mykey2", "mykey");
            AssertStringsAreEqual(r.Get("mykey"), "b");
            Assert.IsFalse(r.Exists("mykey2"));
        }

        #region Sort
        [Test]
        public void Test_Sort_Against_Sorted_Set()
        {
            r.Del("zset");
            r.ZAdd("zset", 1, "a");
            r.ZAdd("zset", 5, "b");
            r.ZAdd("zset", 2, "c");
            r.ZAdd("zset", 10, "d");
            r.ZAdd("zset", 3, "e");
            var actual = r.Sort("zset", true, true, null, null);
            var expected = new string[] { "e", "d", "c", "b", "a" };
            AssertListsAreEqual(actual, expected);
        } 
        #endregion

        [Test]
        public void TestTypeCommand()
        {
            const string Key = "DummYKey";

            Assert.AreEqual(RedisType.None, r.Type("I_DO_NOT_EXIST"));

            r.Set(Key, "TEST");
            Assert.AreEqual(r.Type(Key), RedisType.String);
            Assert.AreEqual(r.Del(Key), 1);
            Assert.AreEqual(r.Type(Key), RedisType.None);

            r.SAdd(Key, "TEST");
            Assert.AreEqual(r.Type(Key), RedisType.Set);
            r.Del(Key);

            r.ZAdd(Key, 1.0, "TEST");
            Assert.AreEqual(r.Type(Key), RedisType.SortedSet);
            r.Del(Key);

            r.LPush(Key, "TEST");
            Assert.AreEqual(r.Type(Key), RedisType.List);
            r.Del(Key);

            r.HSet(Key, "TEST", "hallo");
            Assert.AreEqual(r.Type(Key), RedisType.Hash);
            r.Del(Key);

        }

        #region Pipeline

        [Test]
        public void TestPipelineBasic()
        {
            using(r.BeginPipeline())
            {
                r.Incr("p", 1);
                r.Incr("p", 5);
                r.Decr("p", 4);               
            }
            Assert.AreEqual((int)r["p"], 2);
        }

        [Test]
        public void TestTransactionalPipelineBasic()
        {
            // TODO: need Moq or RhinoMocks to really test this
            using (var pipeline = r.BeginPipeline(true))
            {
                r.Incr("p", 1);
                r.Incr("p", 5);
                r.Decr("p", 4);
            }
            Assert.AreEqual(2, r["p"]);
        }

        #endregion

        #region Timeout/Expiration

        [Test]
        public void TestExpirationAndTTL()
        {
            r["foo"] = "bar";

            // check for key expiration
            Assert.IsTrue(r.Expire("foo", 1));
            Assert.AreEqual(1, r.Ttl("foo"));
            Assert.IsTrue(r.Exists("foo"));
            Thread.Sleep(2);
            Assert.IsFalse(r.Exists("foo"));
            Assert.AreEqual(-1, r.Ttl("foo"));

            // check for consistent TTL values
            r.Set("foo", "bar");
            Assert.IsTrue(r.Expire("foo", 100));
            Thread.Sleep(3);
            Assert.AreEqual(97, r.Ttl("foo"));

            // delete key on negative TTL
            r["foo"] = "bar";
            Assert.IsTrue(r.Expire("foo", -100));
            Assert.IsFalse(r.Exists("foo"));
            Assert.AreEqual(-1, r.Ttl("foo"));
        }


 
        #endregion
    }
}