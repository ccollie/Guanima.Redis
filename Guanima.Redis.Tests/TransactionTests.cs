using System.Linq;
using Guanima.Redis.Commands;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class TransactionTests : RedisClientTestFixture
    {
        [Test]
        public void Test_Transaction_Basics() 
        {
            r.Del("mylist");
            r.RPush("mylist", "a");
            r.RPush("mylist", "b");
            r.RPush("mylist", "c");
            RedisCommand lRangeCommand;
            using (var trans = r.BeginTransaction())
            {
                r.LRange("mylist", 0, -1);
                r.Set("scalar", 1);
                r.Incr("scalar");
                var cmds = trans.Commands.ToList();
                lRangeCommand = cmds[0];
            }
            var expected = new[] {"a", "b", "c"};
            AssertListsAreEqual(expected, lRangeCommand.Value);            
        } 

    }
}