using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Allows integration tests to run in isolation as for each test a clean database with current schema and default objects is created.
    /// </summary>
    /// <remarks>
    /// The isolated integration tests use LocalDB to create and drop databases when needed. It works with two types of database.
    /// The master database is created using SQL scripts from the solution. For each modification of this folder a new master database is created.
    /// The master database is detached after it has been created and its files are copied to create an instance database that a running test will use.
    /// The instance and master databases share a name, but the file names are different.
    /// The instance database is removed when a test finishes and a master database, that is out of date, is removed automatically.
    /// </remarks>
    [Category.IsolatedIntegration]
    public class IsolatedIntegrationTests : AutomatedTestsWithLocalDB
    {
        /// <summary>
        /// Test fixture set up.
        /// </summary>
        [OneTimeSetUp]
        public void IsolatedIntegrationTestsFixtureSetup()
        {
            RunExtenderAction<IsolatedIntegrationTests>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Test set up.
        /// </summary>
        [TestInitialize]
        [SetUp]
        public void IsolatedIntegrationTestsSetup()
        {
            RunExtenderAction<IsolatedIntegrationTests>(e => e.SetUp());
        }

        
        /// <summary>
        /// Test clean up.
        /// </summary>
        [TestCleanup]
        [TearDown]
        public void IsolatedIntegrationTestsTearDown()
        {
            RunExtenderAction<IsolatedIntegrationTests>(e => e.TearDown(), true);
        }


        /// <summary>
        /// Test fixture clean up.
        /// </summary>
        [OneTimeTearDown]
        public void IsolatedIntegrationTestsFixtureTearDown()
        {
            RunExtenderAction<IsolatedIntegrationTests>(e => e.FixtureTearDown(), true);
        }
    }
}
