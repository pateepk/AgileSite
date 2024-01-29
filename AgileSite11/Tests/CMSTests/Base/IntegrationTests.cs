using System;
using System.Configuration;

using CMS.Base;
using CMS.DataEngine;

using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for integration tests
    /// </summary>
    [Category.Integration]
    public class IntegrationTests : AutomatedTestsWithData
    {
        private bool mInitialized;


        /// <summary>
        /// Fixture setup
        /// </summary>
        [OneTimeSetUp]
        public void IntegrationTestsFixtureSetup()
        {
            InitDatabase();

            RunExtenderAction<IntegrationTests>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        [SetUp]
        public void IntegrationTestsSetup()
        {
            InitDatabase();

            RunExtenderAction<IntegrationTests>(e => e.SetUp());
        }


        /// <summary>
        /// Test tear down
        /// </summary>
        [TestCleanup]
        [TearDown]
        public void IntegrationTestsTearDown()
        {
            RunExtenderAction<IntegrationTests>(e => e.TearDown(), true);

            EndApplication();

            CleanUpDataContext();

            mInitialized = false;
        }


        /// <summary>
        /// Fixture tear down
        /// </summary>
        [OneTimeTearDown]
        public void IntegrationTestsFixtureTearDown()
        {
            RunExtenderAction<IntegrationTests>(e => e.FixtureTearDown(), true);
        }


        private void InitDatabase()
        {
            if (!mInitialized)
            {
                CMSAppSettings.GetApplicationSettings += GetTestApplicationSettings;

                SetupConnectionStrings();

                InitApplication();
            }
        }


        /// <summary>
        /// Sets up the connection strings for the test
        /// </summary>
        protected virtual void SetupConnectionStrings()
        {
            var connString = TestsConfig.GetTestConnectionString("CMSTestConnectionString");

            if (String.IsNullOrEmpty(connString))
            {
                throw new Exception("Integration test must define CMSTestConnectionString in the app.config connection strings.");
            }

            ConnectionHelper.ConnectionString = connString;
        }


        /// <summary>
        /// Reads settings from test application configuration file. 
        /// </summary>
        /// <param name="key">Setting key.</param> 
        protected virtual string GetTestApplicationSettings(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
