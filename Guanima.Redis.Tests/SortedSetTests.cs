using System.Collections.Generic;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class SortedSetTests : RedisClientTestFixture
    {
        #region ZCard

        [Test]
        public void ZCard_On_Non_Existing_Key_Should_Return_0()
        {
            Assert.AreEqual(r.ZCard("ztmp-blabla"), 0);
        }

        #endregion

        #region ZAdd

        [Test]
        public void Test_Basic_ZAdd_And_Score_Update()
        {
            r.ZAdd("ztmp", 10, "x");
            r.ZAdd("ztmp", 20, "y");
            r.ZAdd("ztmp", 30, "z");
            var aux1 = r.ZRange("ztmp", 0, -1);
            r.ZAdd("ztmp", 1, "y");
            var aux2 = r.ZRange("ztmp", 0, -1);
            AssertListsAreEqual(aux1, "x", "y", "z");
            AssertListsAreEqual(aux2, "y", "x", "z");
        }
        #endregion

        #region ZIncrBy

        [Test]
        public void ZIncrBy_Can_Create_A_New_Sorted_Set() 
        {
            r.Del("zset");
            r.ZIncrBy("zset", 1, "foo");
            AssertListsAreEqual(r.ZRange("zset", 0, -1), new[] {"foo"});
            Assert.AreEqual(r.ZScore("zset", "foo"), 1);
        } 

        [Test]
        public void Test_ZIncrBy_Increment_And_Decrement() 
        {
            r.ZIncrBy("zset", 2, "foo");
            r.ZIncrBy("zset", 1, "bar");
            var v1 = r.ZRange("zset", 0, -1);
    	   
            AssertListsAreEqual(v1, "bar","foo");
            r.ZIncrBy("zset", 10, "bar");
            r.ZIncrBy("zset",-5, "foo");
            r.ZIncrBy("zset",-5,  "bar");
            var v2 = r.ZRange("zset", 0, -1);
            AssertListsAreEqual(v2, "foo", "bar");
            Assert.AreEqual(-3, r.ZScore("zset", "foo")); 
            Assert.AreEqual(6, r.ZScore("zset", "bar"));
        } 

        #endregion

        #region ZRank

        [Test]
        public void ZRank_Should_Return_Correct_Member_Ranks()
        {
            r.Del("zranktmp");
            r.ZAdd("zranktmp", 10, "x");
            r.ZAdd("zranktmp", 20, "y");
            r.ZAdd("zranktmp", 30, "z");

            Assert.AreEqual(r.ZRank("zranktmp", "x"), 0);
            Assert.AreEqual(r.ZRank("zranktmp", "y"), 1);
            Assert.AreEqual(r.ZRank("zranktmp", "z"), 2);

            r.ZRem("zranktmp", "y");
            Assert.That(r.ZRank("zranktmp", "x"), Is.EqualTo(0));
            Assert.That(r.ZRank("zranktmp", "z"), Is.EqualTo(1));
         } 


        #endregion

        #region ZRevRank
        [Test]
        public void ZRevRank_Should_Return_Correct_Member_Ranks()
        {
            r.Del("zranktmp");
            r.ZAdd("zranktmp", 10, "x");
            r.ZAdd("zranktmp", 20, "y");
            r.ZAdd("zranktmp", 30, "z");

            Assert.That(r.ZRevRank("zranktmp", "x"), Is.EqualTo(2));
            Assert.That(r.ZRevRank("zranktmp", "y"), Is.EqualTo(1));
            Assert.That(r.ZRevRank("zranktmp", "z"), Is.EqualTo(0));
        } 
        
        #endregion

        #region ZRange

        [Test]
        public void Test_ZRangeByScore_WithScores()
        {
            r.ZAdd("zset", 1, "a");
            r.ZAdd("zset", 2, "b");
            r.ZAdd("zset", 3, "c");
            r.ZAdd("zset", 4, "d");
            r.ZAdd("zset", 5, "e");
            var actual = r.ZRangeByScore("zset", 2, 4, true);
            var expected = new[]{"b","2","c","3","d","4"};
            AssertListsAreEqual(expected, actual); 
        }

        [Test]
        public void Test_ZRangeByScore_With_Limit()
        {
            r.ZAdd("zset", 1, "a");
            r.ZAdd("zset", 2, "b");
            r.ZAdd("zset", 3, "c");
            r.ZAdd("zset", 4, "d");
            r.ZAdd("zset", 5, "e");
            var actual = r.ZRangeByScore("zset", 0, 10, 0, 2);
            AssertListsAreEqual(new[] { "a", "b" }, actual);
            actual = r.ZRangeByScore("zset", 0, 10, 2, 3);
            AssertListsAreEqual(actual, new[] { "c", "d", "e" });

            actual = r.ZRangeByScore("zset", 0, 10, 2, 10);
            AssertListsAreEqual(new[] { "c", "d", "e" }, actual);

            actual = r.ZRangeByScore("zset", 0, 10, 20, 10);
            Assert.AreEqual(0, actual.MultiBulkValues.Length);
        } 

        [Test]
        public void Test_ZRangeByScore_With_Limit_And_WithScores()
        {
            r.ZAdd("zset", 10, "a");
            r.ZAdd("zset", 20, "b");
            r.ZAdd("zset", 30, "c");
            r.ZAdd("zset", 40, "d");
            r.ZAdd("zset", 50, "e");
            var actual = r.ZRangeByScore("zset", 20, 50, 2, 3, true);
            var expected = new[]{"d", "40", "e", "50"};
            AssertListsAreEqual(expected, actual); 
        }


        #endregion

        #region ZScore

        [Test]
        public void Test_ZScore()
        {
            var aux = new List<int>();
            for (var i = 0; i < 1000; i++)
            {
                int score = TestHelpers.RandomInt(0, int.MaxValue);
                aux.Add(score);
                r.ZAdd("zscoretest", score, i);
            }
            for (var j = 0; j < 1000; j++)
            {
                Assert.AreEqual(r.ZScore("zscoretest", j), aux[j]);
            }
        }

        #endregion
    }
}