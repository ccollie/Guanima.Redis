using NUnit.Framework;
using System.Collections.Generic;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class SetTests : RedisClientTestFixture
    {

        [Test]
        public void A_Set_Should_Not_Save_Duplicate_Elements()
        {
            r.Del("myset");
            r.SAdd("myset", "foo");
            r.SAdd("myset", "foo");
            r.SAdd("myset", "foo");
            Assert.AreEqual(r.SCard("myset"), 1);
        } 

        const string Key = "Skey";
 
        [Test]
        public void TestSetAddAndIsMember()
        {
            Assert.IsFalse(r.SIsMember(Key, "V"));
            Assert.IsTrue(r.SAdd(Key, "V"));
            Assert.IsTrue(r.SIsMember(Key, "V"));
        }

        [Test]
        public void TestSetRemove()
        {
            r.Del(Key);
            Assert.IsTrue(r.SAdd(Key, "V"));
            Assert.IsTrue(r.SRem(Key, "V"));
            Assert.IsFalse(r.SIsMember(Key, "V"));
            Assert.IsFalse(r.SRem(Key, "Y"));
        }

        #region xtra //////////////////////
        private const string SetId = "testset";
        private List<string> _storeMembers;

        public override void  Setup()
        {
 	        base.Setup();
            _storeMembers = new List<string> { "one", "two", "three", "four" };
            r.FlushDB();
            r.Del(SetId);
        }

        [Test]
        public void Can_AddToSet_And_GetAllFromSet()
        {
            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            List<string> members = r.SMembers(SetId);
            Assert.That(members, Is.EquivalentTo(_storeMembers));
        }

        [Test]
        public void Can_RemoveFromSet()
        {
            const string removeMember = "two";

            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            r.SRem(SetId, removeMember);

            _storeMembers.Remove(removeMember);

            List<string> members = r.SMembers(SetId);

            Assert.That(members, Is.EquivalentTo(_storeMembers));
        }

        [Test]
        public void Can_PopFromSet()
        {
            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            var member = r.SPop(SetId);

            Assert.That(_storeMembers.Contains(member), Is.True);
        }

        [Test]
        public void Can_MoveBetweenSets()
        {
            const string fromSetId = "testmovefromset";
            const string toSetId = "testmovetoset";
            const string moveMember = "four";
            var fromSetIdMembers = new List<string> { "one", "two", "three", "four" };
            var toSetIdMembers = new List<string> { "five", "six", "seven" };

            fromSetIdMembers.ForEach(x => r.SAdd(fromSetId, x));
            toSetIdMembers.ForEach(x => r.SAdd(toSetId, x));

            r.SMove(fromSetId, toSetId, moveMember);

            fromSetIdMembers.Remove(moveMember);
            toSetIdMembers.Add(moveMember);

            List<string> readFromSetId = r.SMembers(fromSetId);
            List<string> readToSetId = r.SMembers(toSetId);

            Assert.That(readFromSetId, Is.EquivalentTo(fromSetIdMembers));
            Assert.That(readToSetId, Is.EquivalentTo(toSetIdMembers));
        }

        [Test]
        public void Can_GetSetCount()
        {
            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            var setCount = r.SCard(SetId);

            Assert.That(setCount, Is.EqualTo(_storeMembers.Count));
        }

        [Test]
        public void Does_SetContainsValue()
        {
            const string existingMember = "two";
            const string nonExistingMember = "five";

            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            Assert.That(r.SIsMember(SetId, existingMember), Is.True);
            Assert.That(r.SIsMember(SetId, nonExistingMember), Is.False);
        }

        [Test]
        public void Can_IntersectBetweenSets()
        {
            const string set1Name = "testintersectset1";
            const string set2Name = "testintersectset2";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));

            List<string> intersectingMembers = r.SInter(set1Name, set2Name);

            Assert.That(intersectingMembers, Is.EquivalentTo(new List<string> { "four", "five" }));
        }

        [Test]
        public void Can_Store_IntersectBetweenSets()
        {
            const string set1Name = "testintersectset1";
            const string set2Name = "testintersectset2";
            const string storeSetName = "testintersectsetstore";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));

            r.SInterStore(storeSetName, set1Name, set2Name);

            List<string> intersectingMembers = r.SMembers(storeSetName);

            Assert.That(intersectingMembers, Is.EquivalentTo(new List<string> { "four", "five" }));
        }

        [Test]
        public void Can_UnionBetweenSets()
        {
            const string set1Name = "testunionset1";
            const string set2Name = "testunionset2";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));

            List<string> unionMembers = r.SUnion(set1Name, set2Name);

            Assert.That(unionMembers, Is.EquivalentTo(
                    new List<string> { "one", "two", "three", "four", "five", "six", "seven" }));
        }

        [Test]
        public void Can_Store_UnionBetweenSets()
        {
            const string set1Name = "testunionset1";
            const string set2Name = "testunionset2";
            const string storeSetName = "testunionsetstore";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));

            r.SUnionStore(storeSetName, set1Name, set2Name);

            List<string> unionMembers = r.SMembers(storeSetName);

            Assert.That(unionMembers, Is.EquivalentTo(
                    new List<string> { "one", "two", "three", "four", "five", "six", "seven" }));
        }

        [Test]
        public void Can_DiffBetweenSets()
        {
            const string set1Name = "testdiffset1";
            const string set2Name = "testdiffset2";
            const string set3Name = "testdiffset3";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };
            var set3Members = new List<string> { "one", "five", "seven", "eleven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));
            set3Members.ForEach(x => r.SAdd(set3Name, x));

            List<string> diffMembers = r.SDiff(set1Name, set2Name, set3Name);

            Assert.That(diffMembers, Is.EquivalentTo(
                    new List<string> { "two", "three" }));
        }

        [Test]
        public void Can_Store_DiffBetweenSets()
        {
            const string set1Name = "testdiffset1";
            const string set2Name = "testdiffset2";
            const string set3Name = "testdiffset3";
            const string storeSetName = "testdiffsetstore";
            var set1Members = new List<string> { "one", "two", "three", "four", "five" };
            var set2Members = new List<string> { "four", "five", "six", "seven" };
            var set3Members = new List<string> { "one", "five", "seven", "eleven" };

            set1Members.ForEach(x => r.SAdd(set1Name, x));
            set2Members.ForEach(x => r.SAdd(set2Name, x));
            set3Members.ForEach(x => r.SAdd(set3Name, x));

            r.SDiffStore(storeSetName, set1Name, set2Name, set3Name);

            List<string> diffMembers = r.SMembers(storeSetName);

            Assert.That(diffMembers, Is.EquivalentTo(
                    new List<string> { "two", "three" }));
        }

        [Test]
        public void Can_GetRandomEntryFromSet()
        {
            _storeMembers.ForEach(x => r.SAdd(SetId, x));

            var randomEntry = r.SRandMember(SetId);

            Assert.That(_storeMembers.Contains(randomEntry), Is.True);
        }


        #endregion //////////////////////////////
    }
}