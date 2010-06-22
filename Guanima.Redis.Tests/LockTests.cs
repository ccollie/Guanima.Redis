using System;
using NUnit.Framework;

namespace Guanima.Redis.Tests
{
    [TestFixture]
    public class LockTests : RedisClientTestFixture
    {
        private const string LockName = "_LOCK_";

        [Test]
        public void Acquiring_A_Lock_Should_Create_A_Redis_Key()
        {
            using (r.Lock(LockName))
            {
                Assert.That(r.Exists(LockName));
                var val = r.Get(LockName);
                Assert.That(val, Is.Not.EqualTo(null));
                long lval = val;
                Assert.That(lval, Is.GreaterThan(0));
            }    
        }

        [Test]
        public void A_Lock_Should_Delete_Its_Redis_Key_Before_Leaving_Scope()
        {
            using (r.Lock(LockName))
            {
                Assert.That(r.Exists(LockName));
            }
            Assert.That(!r.Exists(LockName));
            
            using (r.Lock(LockName, TimeSpan.Zero, TimeSpan.FromDays(1)))
            {
                Assert.That(r.Exists(LockName));
            }
            Assert.That(!r.Exists(LockName));
        }


        [Test]
        public void Acquiring_A_Lock_Should_Prevent_Another_Client_From_Acquiring_It()
        {
            using (r.Lock(LockName))
            {
                Assert.That(r.Exists(LockName));
                using(var newClient = CreateClient())
                {
                    Assert.Throws(typeof (RedisLockTimeoutException),
                                  () =>
                                      {
                                          using (newClient.Lock(LockName, TimeSpan.FromSeconds(2)))
                                          {
                                              Assert.Fail("Lock was acquired twice.");
                                          }
                                      });
                }
            }            
        }
    }
}
