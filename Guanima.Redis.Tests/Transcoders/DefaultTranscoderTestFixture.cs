using System;
using Guanima.Redis.Transcoders;
using NUnit.Framework;

namespace Guanima.Redis.Tests.Transcoders
{
    [Serializable]
    public class TestClass
    {
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public int TotalPoints { get; set; }
        public double Credits { get; set; }
    }

    [TestFixture]
    public class DefaultTranscoderTestFixture
    {
        private DefaultTranscoder _transcoder;

        [SetUp]
        public void Setup()
        {
            _transcoder = new DefaultTranscoder();
        }

        [Test]
        public void Can_Properly_Serialize_Null()
        {
            var toBytes = _transcoder.Serialize(null);
            var fromBytes = _transcoder.Deserialize(toBytes);        
            Assert.IsNull(fromBytes);
        }

        [Test]
        public void Should_Persist_Primitive_Types_Properly()
        {
            Boolean boolValue = true;
            String stringValue = "String Value";
            Int16 int16Value = -167;
            Int32 int32Value = 562781;
            Int64 int64Value = 9920873721;
            UInt16 uint16Value = 917;
            UInt32 uint32Value = 92011;
            UInt64 uint64Value = 21109201;
            double doubleValue = Math.PI;
            Single singleValue = (float) 234.122;
            DateTime dateTimeValue = DateTime.UtcNow;
            Char charValue = 'z';

            TestRoundTrip(boolValue);
            TestRoundTrip(stringValue);
            TestRoundTrip(int16Value);
            TestRoundTrip(int32Value);
            TestRoundTrip(int64Value);
            TestRoundTrip(uint16Value);
            TestRoundTrip(uint32Value);
            TestRoundTrip(uint64Value);
            TestRoundTrip(doubleValue);
            TestRoundTrip(singleValue);
            TestRoundTrip(dateTimeValue);
            TestRoundTrip(charValue);
        }

        [Test]
        public void Can_Properly_Serialize_Arbitrary_Classes()
        {
            var now = DateTime.Now;
            var user = new TestClass
                           {
                               Credits = 1.0,
                               IsActive = false,
                               TotalPoints = 100,
                               LastLogin = now,
                               Username = "username"
                           };
            var toBytes = _transcoder.Serialize(user);
            var fromBytes = _transcoder.Deserialize(toBytes);
            Assert.IsInstanceOf(typeof (TestClass), fromBytes);
            var actual = (TestClass) fromBytes;
            Assert.AreEqual(user.Credits, actual.Credits);
            Assert.AreEqual(user.IsActive, actual.IsActive);
            Assert.AreEqual(user.LastLogin, actual.LastLogin);
            Assert.AreEqual(user.TotalPoints, actual.TotalPoints);
            Assert.AreEqual(user.Username, actual.Username);
        }

        private void TestRoundTrip(object value)
        {
            var toBytes = _transcoder.Serialize(value);
            var fromBytes = _transcoder.Deserialize(toBytes);
            Assert.AreEqual(value, fromBytes, "Error serializing value of type {0}.", value.GetType());
        }

    }
}
