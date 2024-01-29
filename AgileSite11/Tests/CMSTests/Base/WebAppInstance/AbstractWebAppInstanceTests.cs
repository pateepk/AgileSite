using System;
using System.Configuration;

using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Helpers;
using CMS.WebFarmSync;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Abstract base class for integration tests that require running web app instance.
    /// </summary>
    public abstract class AbstractWebAppInstanceTests : AutomatedTestsWithLocalDB
    {
        private CMSActionContext mTestContext;

        /// <summary>
        /// Default site name.
        /// </summary>
        protected const string DEFAULT_SITE_NAME = "TestInstanceSite";


        /// <summary>
        /// Site that is running on the <see cref="InstanceUrl"/>.
        /// </summary>
        protected SiteInfo Site
        {
            get;
            private set;
        }


        private IWebInstanceTestsEnvironmentManager InstanceManager
        {
            get
            {
                return WebInstanceTestsAssemblySetUp.Manager;
            } 
        }


        /// <summary>
        /// Url of requested web application.
        /// </summary>
        protected string InstanceUrl
        {
            get
            {
                return InstanceManager.WebAppInstanceUrl;
            }
        }


        /// <summary>
        /// Domain the requested web application runs on.
        /// </summary>
        protected string InstanceDomain
        {
            get
            {
                return URLHelper.GetDomain(InstanceUrl);
            }
        }


        /// <summary>
        /// Physical path to the web application.
        /// </summary>
        protected string InstancePath
        {
            get
            {
                return InstanceManager.WebAppInstancePath;
            }
        }


        /// <summary>
        /// Fixture setup
        /// </summary>
        [OneTimeSetUp]
        public void AbstractWebAppInstanceTestsFixtureSetup()
        {
            if (SharedDatabaseForAllTests)
            {
                EnsureLicense();
                EnsureSite();
                SetWebInstanceConnectionString();
                EnableWebFarms();
            }

            RunExtenderAction<AbstractWebAppInstanceTests>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        [SetUp]
        public void AbstractWebAppInstanceTestsTestsSetup()
        {
            InstanceManager.EnsureIISProcess();                     
            
            if (!SharedDatabaseForAllTests)
            {
                EnsureLicense();

                Site = null;
                EnsureSite();

                SetWebInstanceConnectionString();
            }

            mTestContext = new CMSActionContext { LogWebFarmTasks = true };
            EnableWebFarms();

            RunExtenderAction<AbstractWebAppInstanceTests>(e => e.SetUp());
        }


        /// <summary>
        /// Test tear down
        /// </summary>
        [TestCleanup]
        [TearDown]
        public void AbstractWebAppInstanceTestsTearDown()
        {
            RunExtenderAction<AbstractWebAppInstanceTests>(e => e.TearDown(), true);

            mTestContext.Dispose();
        }


        /// <summary>
        /// Fixture tear down
        /// </summary>
        [OneTimeTearDown]
        public void AbstractWebAppInstanceTestsFixtureTearDown()
        {
            RunExtenderAction<AbstractWebAppInstanceTests>(e => e.FixtureTearDown(), true);
        }



        /// <summary>
        /// Ensures site.
        /// </summary>
        protected void EnsureSite()
        {
            if (Site != null)
            {
                return;
            }

            var siteName = GetSiteName();
            Site = SiteInfoProvider.GetSiteInfo(siteName) ?? CreateSite(siteName, InstanceDomain);
        }


        /// <summary>
        /// Deletes site.
        /// </summary>
        protected virtual void DeleteSite()
        {
            using (new CMSActionContext { LogWebFarmTasks = false })
            {
                Site.Delete();
            }
            
            Site = null;
        }


        /// <summary>
        /// Creates site for instance tests.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="domain">Site domain name.</param>
        protected virtual SiteInfo CreateSite(string siteName, string domain)
        {
            var siteInfo = SiteInfo.New(i =>
            {
                i.SiteName = siteName;
                i.DisplayName = siteName;
                i.Status = SiteStatusEnum.Running;
                i.DomainName = domain;
                i.SiteGUID = Guid.NewGuid();
                i.SiteLastModified = DateTime.Now;
            });
            siteInfo.Insert();

            return siteInfo;
        }


        /// <summary>
        /// Returns site name of the site running on <see cref="AbstractWebAppInstanceTests.InstanceUrl"/>.
        /// </summary>
        protected virtual string GetSiteName()
        {
            return DEFAULT_SITE_NAME;
        }


        /// <summary>
        /// Sets up web farms.
        /// </summary>
        protected virtual void EnableWebFarms()
        {
            SettingsHelper.AppSettings["CMSWebFarmMode"] = "Automatic";
            SettingsHelper.AppSettings["CMSWebFarmSyncInterval"] = "10";
            SettingsHelper.AppSettings["CMSWebFarmServerName"] = "Tests";

            WebFarmContext.WebFarmEnabled = true;

            SystemContext.IsWebSite = true;
            WebFarmSyncHandlers.Init();
            SystemContext.IsWebSite = false;

            WebFarmTaskProcessor.Current.EnsureRunningThread();
            WebFarmMonitor.Current.EnsureRunningThread();
            WebFarmTaskCreator.Current.EnsureRunningThread();
        }


        private void SetWebInstanceConnectionString()
        {
            var connString = ConnectionHelper.ConnectionString;
            var webAppPath = InstanceManager.WebAppInstancePath;
            var config = SettingsHelper.OpenConfiguration(webAppPath);

            var cs = config.ConnectionStrings.ConnectionStrings["CMSConnectionString"];
            if (cs.ConnectionString != connString)
            {
                cs.ConnectionString = connString;
                config.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}