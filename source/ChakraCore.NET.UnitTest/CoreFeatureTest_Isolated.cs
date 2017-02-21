using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using ChakraCore.NET.Extension.SharedMemory;

namespace ChakraCore.NET.UnitTest
{
    [TestClass,TestCategory("Core Features Isolated")]
        
    public class CoreFeatureTest_Isolated :ContextTestBase
    {

        [TestMethod]
        public void RunScript()
        {
            var result=context.RunScript(TestHelper.JSRunScript);
            Assert.AreEqual<string>(result, "test");
        }


        [TestMethod]
        public void ReadWriteTest()
        {
            System.Diagnostics.Debug.WriteLine("ReadWriteTest start");
            ReadWrite<string>("hello");
            ReadWrite<int>(1000);
            ReadWrite<float>(100.11f);
            ReadWrite<byte>(0x0f);
            ReadWrite<double>(-33.4455);
            ReadWrite<bool>(false);
            System.Diagnostics.Debug.WriteLine("ReadWriteTest stop");
        }

        [TestMethod]
        public void CallMethodTest()
        {
            CallMethod<string>("hello");
            CallMethod<int>(1000);
            CallMethod<float>(100.11f);
            CallMethod<byte>(0x0f);
            CallMethod<double>(-33.4455);
            CallMethod<bool>(false);
        }

        [TestMethod]
        public void CallFunctionTest()
        {
            CallFunction<string>("hi", "hihi");
            CallFunction<int>(1, 2);
            CallFunction<float>(1.1f, 2.2f);
            CallFunction<byte>(0x01, 0x02);
            CallFunction<double>(10.1, 20.2);
        }


        [TestMethod]
        public void CallBackTest()
        {
            context.RunScript(TestHelper.JSCall);
            context.ValueConverter.RegisterFunctionConverter<int, int>();
            int result=context.RootObject.CallFunction<int, Func<int, int>,int>("addcallback",1,
                (value) => {
                    return value + value;
                }
                );
            Assert.AreEqual(result, 3);
        }

        private void CallMethod<T>(T value)
        {
            context.RunScript(TestHelper.JSCall);
            context.RootObject.CallMethod<T>("add",value);
        }
        private void CallFunction<T>(T value,T expect)
        {
            context.RunScript(TestHelper.JSCall);
            T result=context.RootObject.CallFunction<T,T>("add", value);
            Assert.AreEqual<T>(result, expect);
        }
        [TestMethod]
        public void ProxyCallback()
        {
            TestStub.RegisterValueConverter(context);
            TestStub stub = new TestStub();
            context.RootObject.WriteProperty("myStub", stub);
            var result=context.RunScript(TestHelper.JSProxy);
            string s = context.RootObject.ReadProperty<string>("callProxyResult");
            Assert.AreEqual("hihi", s);
        }

        [TestMethod]
        public void ProxyTransferback()
        {
            TestStub.RegisterValueConverter(context);
            TestStub stub = new TestStub();
            context.RootObject.WriteProperty("a", stub);
            var result = context.RunScript(TestHelper.JSValueTest);
            TestStub b = context.RootObject.ReadProperty<TestStub>("b");
            Assert.IsTrue(object.ReferenceEquals(stub, b));
        }

        




        private void ReadWrite<T>(T value)
        {
            context.RootObject.WriteProperty<T>("a", value);
            context.RunScript(TestHelper.JSValueTest);
            T b = context.RootObject.ReadProperty<T>("b");
            Assert.AreEqual<T>(b, value);
        }



    }
}
