using CMS.DataEngine;
using CMS.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMS.CustomTests
{
    /// <summary>
    /// Sample unit tests. This test is completely disconnected from the database, and uses fake data to allow the API run without the database.
    /// </summary>
    [TestClass]
    public class SampleUnitTests : UnitTests
    {
        [ClassInitialize]
        public static void InitClass(TestContext context)
        {
            InitializeTestClass();
        }


        [ClassCleanup]
        public static void CleanUpClass()
        {
            CleanUpTestClass();
        }


        [TestInitialize]
        public void Init()
        {
            // Fake the data of a particular provider
            Fake<DataClassInfo, DataClassInfoProvider>().WithData(
                DataClassInfo.New(dc =>
                {
                    dc.ClassID = 1;
                    dc.ClassName = "MyTest.MyClass";
                })
            );
        }


        [TestMethod]
        public void MyTest_Unit_ReturnsClass()
        {
            // Try to get the faked data
            var cls = DataClassInfoProvider.GetDataClassInfo("MyTest.MyClass");

            Assert.IsNotNull(cls);
            Assert.AreEqual(1, cls.ClassID);
        }
    }
}
