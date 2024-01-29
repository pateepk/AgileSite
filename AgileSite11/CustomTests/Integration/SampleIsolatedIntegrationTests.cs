using CMS.DataEngine;
using CMS.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMS.CustomTests
{
    /// <summary>
    /// Sample of an isolated integration test, which runs using it's own database. The database for the test is created ad-hoc using the default installation scripts.
    /// </summary>
    [TestClass]
    public class SampleIsolatedIntegrationTests : IsolatedIntegrationTests
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


        /// <summary>
        /// Initializes the database
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            // Create data in the isolated empty database
            DataClassInfo.New(dc =>
                {
                    dc.ClassName = "MyTest.MyClass";
                    dc.ClassDisplayName = "My test class";
                })
            .Insert();
        }


        [TestMethod]
        public void MyTest_IsolatedIntegration_ReturnsClass()
        {
            // Try to get the data to verify it was created in the isolated database
            var cls = DataClassInfoProvider.GetDataClassInfo("MyTest.MyClass");

            Assert.IsNotNull(cls);
            Assert.AreEqual("My test class", cls.ClassDisplayName);
        }
    }
}
