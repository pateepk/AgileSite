using CMS.DataEngine;
using CMS.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMS.CustomTests
{
    /// <summary>
    /// Sample of integration tests. This test runs against an existing database specified in app.config connection string named CMSTestConnectionString.
    /// </summary>
    [TestClass]
    public class SampleIntegrationTests : IntegrationTests
    {
        public TestContext TestContext { get; set; }

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


        [TestMethod]
        public void MyTest_Integration_ReturnsClass()
        {
            
            // Try to get existing data from the database
            var cls = DataClassInfoProvider.GetDataClassInfo("cms.user");

            Assert.IsNotNull(cls);
            Assert.AreEqual("cms.user", cls.ClassName.ToLowerInvariant());
        }
    }
}
