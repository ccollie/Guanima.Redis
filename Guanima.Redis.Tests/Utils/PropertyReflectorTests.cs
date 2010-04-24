using Guanima.Redis.Utils;
using NUnit.Framework;

namespace Guanima.Redis.Tests.Utils
{
    // http://blog.guymahieu.com/2006/07/11/deep-reflection-of-properties-propertyreflector/

    [TestFixture]
    public class PropertyReflectorTests
    {
        private PropertyReflector _propertyReflector;

        [SetUp]
        public void SetUp()
        {
            _propertyReflector = new PropertyReflector();
        }

        [Test]
        public void TestGetValue()
        {
            CheckGetValue(GetTestObject());
        }

        [Test]
        public void TestSetValue()
        {
            CheckSetValue(GetTestObject());
        }

        //[Test]
        //public void TestDynamicProxy()
        //{
        //    MyTestObject o = GetProxyTestObject();
        //    CheckGetValue(o);
        //    CheckSetValue(o);
        //}

        private void CheckSetValue(MyTestObject o)
        {
            _propertyReflector.SetValue(o, "Id", "1b");
            _propertyReflector.SetValue(o, "Name", "OneB");
            _propertyReflector.SetValue(o, "Child.Id", "2b");
            _propertyReflector.SetValue(o, "Child.Name", "TwoB");
            Assert.AreEqual(o.Id, "1b");
            Assert.AreEqual(o.Name, "OneB");
            Assert.AreEqual(o.Child.Id, "2b");
            Assert.AreEqual(o.Child.Name, "TwoB");
        }

        private void CheckGetValue(MyTestObject o)
        {
            Assert.AreEqual(_propertyReflector.GetValue(o, "Id"), o.Id);
            Assert.AreEqual(_propertyReflector.GetValue(o, "Name"), o.Name);
            Assert.AreEqual(_propertyReflector.GetValue(o, "Child.Id"), o.Child.Id);
            Assert.AreEqual(_propertyReflector.GetValue(o, "Child.Name"), o.Child.Name);

            Assert.IsNull(_propertyReflector.GetValue(o, "Child.Child"));
            Assert.IsNull(_propertyReflector.GetValue(o, "Child.Child.Child.Id"));
        }

        private static MyTestObject GetTestObject()
        {
            return new MyTestObject("1", "One", new MyTestObject("2", "Two", null));
        }

        //private static MyTestObject GetProxyTestObject()
        //{
        //    ProxyGenerator generator = new ProxyGenerator();
        //    MyTestObject o = (MyTestObject)generator.CreateClassProxy(typeof(MyTestObject), new StandardInterceptor());
        //    MyTestObject o2 = (MyTestObject)generator.CreateClassProxy(typeof(MyTestObject), new StandardInterceptor());
        //    o.Id = "1";
        //    o.Name = "One";
        //    o.Child = o2;
        //    o2.Id = "2";
        //    o2.Name = "Two";
        //    o2.Child = null;
        //    return o;
        //}
    }


    public interface ITestObject
    {
        string Id { get; set; }

        string Name { get; set; }
    }

    public abstract class AbstractTestObject : ITestObject
    {
        private string id;

        public AbstractTestObject() { }

        protected AbstractTestObject(string id)
        {
            this.id = id;
        }

        public virtual string Id
        {
            get { return id; }
            set { id = value; }
        }

        public abstract string Name { get; set; }
    }

    public class MyTestObject : AbstractTestObject
    {
        private string _name;
        private AbstractTestObject _child;

        public MyTestObject() { }

        public MyTestObject(string id, string name, AbstractTestObject child)
            : base(id)
        {
            _name = name;
            _child = child;
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual AbstractTestObject Child
        {
            get { return _child; }
            set { _child = value; }
        }
    }
}
