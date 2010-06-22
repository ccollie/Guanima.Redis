using System;
using System.Collections.Generic;
using Guanima.Redis.Extensions;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class ListTests : RedisClientTestFixture
    {
        const string ListId = "testlist";
        const string ListId2 = "testlist2";

        [Test]
        public void LLen_Against_A_Non_Existing_Key_Should_Return_0()
        {
            Assert.AreEqual(r.LLen("not-a-key"), 0);
        } 

        [Test]
        public void TestRPush()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            storeMembers.ForEach(x => r.RPush(ListId, x));

            var members = r.ListGetAll(ListId);

            AssertAreEqual(members, storeMembers);
        }

   
        [Test]
        public void Can_GetListLength()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            using (var redis = CreateClient())
            {
                storeMembers.ForEach(x => redis.RPush(ListId, x));

                var listCount = redis.LLen(ListId);

                Assert.That(listCount, Is.EqualTo(storeMembers.Count));
            }
        }

        [Test]
        public void Can_GetItemFromList()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            using (var redis = CreateClient())
            {
                storeMembers.ForEach(x => redis.RPush(ListId, x));

                var storeMember3 = storeMembers[2];
                var item3 = redis.LIndex(ListId, 2);

                AssertStringsAreEqual(item3, storeMember3);
            }
        }

        [Test]
        public void Can_Set_Item_In_List()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            using (var redis = CreateClient())
            {
                storeMembers.ForEach(x => redis.RPush(ListId, x));

                storeMembers[2] = "five";
                redis.LSet(ListId, 2, "five");

                var members = redis.ListGetAll(ListId);

                AssertAreEqual(members, storeMembers);
            }
        }

        [Test]
        public void Test_RPop()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            using (var redis = CreateClient())
            {
                storeMembers.ForEach(x => redis.RPush(ListId, x));

                var item4 = redis.RPop(ListId);
                AssertStringsAreEqual("four", item4);
            }
        }

        [Test]
        public void LPop_Against_An_Empty_List_Should_Return_Null()
        {
            r.Del("mylist");
            Assert.IsNull((string)r.LPop("mylist"));
        }


        [Test]
        public void Test_Basic_LPop_RPop()
        {
            r.RPush("mylist", 1);
            r.RPush("mylist", 2);
            r.LPush("mylist", 0);
            Assert.AreEqual(r.LPop("mylist"), 0);
            Assert.AreEqual(r.LPop("mylist"), 1);
            Assert.AreEqual(r.LPop("mylist"), 2);
            Assert.AreEqual(r.LLen("mylist"), 0);            
        } 


        [Test]
        public void Can_Dequeue_From_List()
        {
            var storeMembers = new List<string> { "one", "two", "three", "four" };
            using (var redis = CreateClient())
            {
                storeMembers.ForEach(x => redis.RPush(ListId, x));

                var item1 = redis.LPop(ListId);

                AssertStringsAreEqual("one",item1);
            }
        }

        [Test]
        public void Can_Move_Values_Between_Lists()
        {
            var list1Members = new List<string> { "one", "two", "three", "four" };
            var list2Members = new List<string> { "five", "six", "seven" };
            const string item4 = "four";

            using (var redis = CreateClient())
            {
                list1Members.ForEach(x => redis.RPush(ListId, x));
                list2Members.ForEach(x => redis.RPush(ListId2, x));

                list1Members.Remove(item4);
                list2Members.Insert(0, item4);
                redis.RPopLPush(ListId, ListId2);

                var readList1 = redis.ListGetAll(ListId);
                var readList2 = redis.ListGetAll(ListId2);

                AssertAreEqual(readList1, list1Members);
                AssertAreEqual(readList2, list2Members);
            }
        }


        [Test]
        //	LREM, starting from tail with negative count
        public void Test_LRem()  
        {
            r.FlushDB();
            r.RPush("mylist", "foo");
            r.RPush("mylist", "bar");
            r.RPush("mylist", "foobar");
            r.RPush("mylist", "foobared");
            r.RPush("mylist", "zap");
            r.RPush("mylist", "bar");
            r.RPush("mylist", "test");
            r.RPush("mylist", "foo");
            r.RPush("mylist", "foo");
            var res = r.LRem("mylist", -1, "bar");
            Assert.AreEqual(res,1);
            var list = r.LRange("mylist", 0, -1);
            AssertListsAreEqual(new[]{"foo", "bar", "foobar", "foobared", "zap", "test", "foo", "foo"}, 
                (List<String>)list);
        }

        [Test]
        public void LRem_Should_Remove_All_Occurrences_If_Count_Is_Set_To_0()
        {
            r.FlushDB();
            r.RPush("mylist", "foo");
            r.RPush("mylist", "bar");
            r.RPush("mylist", "foobar");
            r.RPush("mylist", "foobared");
            r.RPush("mylist", "zap");
            r.RPush("mylist", "bar");
            r.RPush("mylist", "test");
            r.RPush("mylist", "foo");
            var res = r.LRem("mylist", 0, "bar");
            var actual = r.LRange("mylist", 0, -1);
            var expected = new[] { "foo", "foobar", "foobared", "zap", "test", "foo" };
            AssertListsAreEqual(actual, expected);
            Assert.AreEqual(res, 2);        
        }  

        [Test]
        public void Test_Mass_LPush_LPop()
        {
            var sum = 0;
            r.Del("mylist");
            for (int i = 0; i < 1000; i++)
            {
                r.LPush("mylist", i);
                sum += i;
            }
            long sum2 = 0;
            for (int i = 0; i < 500; i++)
            {
                sum2 += r.LPop("mylist");
                sum2 += r.RPop("mylist");
            }
            Assert.IsTrue(sum == sum2);
        }

        #region LTrim
        [Test]
        public void Can_Trim_List()
        {
            r.Del("mylist");
            for (var i = 0; i < 100; i++)
            {
                r.LPush("mylist", i);
                r.LTrim("mylist", 0, 4);
            }
            var expected = new[] { "99", "98", "97", "96", "95" };
            var actual = r.LRange("mylist", 0, -1);
            AssertListsAreEqual(actual, expected);    
        } 

        #endregion

        #region RPopLPush

        [Test]
        public void Test_RPopLPush_Where_Source_And_Destination_Are_The_Same()
        {
            r.Del("mylist");
            r.RPush("mylist", "a");
            r.RPush("mylist", "b");
            r.RPush("mylist", "c");
            var l1 = r.LRange("mylist", 0, -1);
            AssertListsAreEqual(l1, new[] { "a", "b", "c" });

            var v = r.RPopLPush("mylist", "mylist");
            Assert.AreEqual((string)v, "c");

            var l2 = r.LRange("mylist", 0, -1);

            AssertListsAreEqual(l2, new[] { "c", "a", "b" });            
        }

        #endregion

        #region LRange

        [Test]
        public void TestLRange() 
	    {
            var emptyList = new long[] {};

            var numbers = new long[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
		    using(r.BeginPipeline()) {
			    for(int i = 0; i < 10; i++)
				    r.RPush("numbers", numbers[i]);
		    }
            
            AssertEquals( 
                numbers.Slice(0, 3), 
                r.LRange("numbers", 0, 3) );
        
            AssertEquals( numbers.Slice(4, 8), r.LRange("numbers", 4, 8) );
            AssertEquals( numbers.Slice(0, 0), r.LRange("numbers", 0, 0) );
            AssertEquals( emptyList, r.LRange("numbers", 1, 0) );
            AssertEquals( numbers, r.LRange("numbers", 0, -1) );
            AssertEquals( new long[]{5}, r.LRange("numbers", 5, -5) );
            AssertEquals( emptyList, r.LRange("numbers", 7, -5) );
            AssertEquals( numbers.Slice(-5, -2), r.LRange("numbers", -5, -2) );
            AssertEquals( numbers, r.LRange("numbers", -100, 100) );

            AssertEquals( new long[]{},  r.LRange("keyDoesNotExist", 0, 1)
        );

        // should throw an exception when trying to do a LRANGE on non-list types
        Assert.Throws(typeof(RedisException), ()=> {
            r.Set("foo", "bar");
            r.LRange("foo", 0, -1);
        });
    }

        [Test]
        public void LRange_Against_A_Non_Existing_Key_Should_Return_Null() 
        {
            RedisValue list = r.LRange("nosuchkey", 0, 1);
            Assert.IsTrue(list.IsEmpty);    
        } 

        #endregion

        #region LSet

        [Test]
        public void Test_LSet()
        {
            r.Del("mylist");
            var ints = new[] {99, 98, 97, 96, 95};
            foreach (int x in ints)
            {
                r.RPush("mylist", x);
            }
            r.LSet("mylist", 1, "foo");
            r.LSet("mylist", -1, "bar");
            var actual = r.LRange("mylist", 0, -1);
            var expected = new[] {"99", "foo", "97", "96", "bar"};
            AssertListsAreEqual(actual, expected);
            
        } 

        #endregion
    }
}