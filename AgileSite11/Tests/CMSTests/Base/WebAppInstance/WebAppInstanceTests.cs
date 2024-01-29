using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for integration tests that require running web app instance.
    /// </summary>
    [Category.WebAppInstanceRequired]
    public class WebAppInstanceTests : AbstractWebAppInstanceTests
    {
        /// <summary>
        /// Returns site name of the site running on <see cref="AbstractWebAppInstanceTests.InstanceUrl"/>.
        /// </summary>
        protected override string GetSiteName()
        {
            return "TestInstanceSite";
        }


        /// <summary>
        /// Fixture setup
        /// </summary>
        [OneTimeSetUp]
        public void WebAppInstanceTestsFixtureSetup()
        {
            RunExtenderAction<WebAppInstanceTests>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        [SetUp]
        public void WebAppInstanceTestsTestsSetup()
        {
            RunExtenderAction<WebAppInstanceTests>(e => e.SetUp());
        }


        /// <summary>
        /// Test tear down
        /// </summary>
        [TestCleanup]
        [TearDown]
        public void WebAppInstanceTestsTearDown()
        {
            RunExtenderAction<WebAppInstanceTests>(e => e.TearDown(), true);
        }


        /// <summary>
        /// Fixture tear down
        /// </summary>
        [OneTimeTearDown]
        public void WebAppInstanceTestsFixtureTearDown()
        {
            RunExtenderAction<WebAppInstanceTests>(e => e.FixtureTearDown(), true);
        }
    }
}
