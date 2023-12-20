using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Routing;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.EventLog;
using CMS.Search;
using CMS.MacroEngine;

using NUnit.Framework;

using TestContext = NUnit.Framework.TestContext;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for advanced tests
    /// </summary>
    [Category.Automated]
    public class AutomatedTests : IDisposable
    {
        #region "Constants"

        /// <summary>
        /// Name of app key for license key that is used in isolated integration tests.
        /// </summary>
        protected const string APP_KEY_LICENSE_KEY = "CMSTestLicenseKey";

        #endregion


        #region "Variables"

        private static int mDomainTestIndex = -1;

        private static bool mFirstTestFixture = true;

        private bool mLastCleanUpSucceeded;

        private readonly AppStateReset mReset;

        private List<TestExtender> mTestExtenders;

        private static string mTemporaryWebAppPathRoot;
        private string mTemporaryWebAppPath;

        private CMSActionContext testActionContext;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the <see cref="FakeEventLogProvider"/> used in tests instead of <see cref="EventLogProvider"/>.
        /// </summary>
        protected FakeEventLogProvider TestsEventLogProvider
        {
            get;
            private set;
        }


        /// <summary>
        /// List of current test extenders
        /// </summary>
        private List<TestExtender> TestExtenders
        {
            get
            {
                return mTestExtenders ?? (mTestExtenders = GetTestExtenders());
            }
        }


        /// <summary>
        /// Returns true if the test is the first one executed on current domain
        /// </summary>
        private static bool FirstTestInDomain
        {
            get
            {
                return (mDomainTestIndex <= 0);
            }
        }


        /// <summary>
        /// Returns true if the test was correctly initialized
        /// </summary>
        private bool TestInitialized
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the application is currently initialized
        /// </summary>
        protected bool ApplicationInitialized
        {
            get;
            set;
        }


        /// <summary>
        /// Last test name
        /// </summary>
        public string LastTestName
        {
            get;
            private set;
        }


        /// <summary>
        /// Current test name
        /// </summary>
        public string CurrentTestName
        {
            get;
            private set;
        }


        /// <summary>
        /// Temporary application root path
        /// </summary>
        private string TemporaryAppPathRoot
        {
            get
            {
                return mTemporaryWebAppPathRoot ?? (mTemporaryWebAppPathRoot = Path.Combine("TempAppFolders", GetType().Assembly.GetName().Name));
            }
        }


        /// <summary>
        /// Temporary application path for current test
        /// </summary>
        protected string TemporaryAppPath
        {
            get
            {
                if (mTemporaryWebAppPath == null)
                {
                    mTemporaryWebAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryAppPathRoot, Guid.NewGuid().ToString());

                    Directory.CreateDirectory(mTemporaryWebAppPath);
                }

                return mTemporaryWebAppPath;
            }
        }

        #endregion


        #region "SetUp / TearDown methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected AutomatedTests()
        {
            mReset = CreateReset();
        }


        /// <summary>
        /// One time set up before first test in test fixture is run
        /// </summary>
        [OneTimeSetUp]
        public void InitFixtureBase()
        {
            TestsCategoryCheck.CheckAllTestsCategories(GetType());

            // Delete temporary web app folders from previous tests run
            if (mFirstTestFixture)
            {
                TestsDirectoryHelper.DeleteDirectoryIgnoreIOErrors(TemporaryAppPathRoot);
            }

            // Reset the application state (if not first test fixture)
            ResetAppStateForFixture();

            RunExtenderAction<AutomatedTests>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Set up before each test
        /// </summary>
        [SetUp]
        public void InitBase()
        {
            TestInitialized = false;

            LoadTestName();

            TestsCategoryCheck.CheckCategories(GetType());

            mDomainTestIndex++;

            // Reset the application state
            AutomaticAppStateReset();

            TestInitialized = true;

            // Track if the context was properly closed before the next test
            mLastCleanUpSucceeded = false;

            ConfigureMacroEvaluationTimeout();
            DisableUnnecessaryFunctionality();
            FakeEventLogProvider();

            RunExtenderAction<AutomatedTests>(e => e.SetUp());
        }


        /// <summary>
        /// Configures timeout for macro resolver in test environment
        /// </summary>
        private void ConfigureMacroEvaluationTimeout()
        {
            var timeout = ValidationHelper.GetInteger(TestsConfig.GetTestAppSetting("CMSTestMacroEvaluationTimeout"), 0);
            if (timeout > 0)
            {
                MacroStaticSettings.EvaluationTimeout = timeout;
            }
        }


        /// <summary>
        /// Clean up after each test
        /// </summary>
        [TearDown]
        public void CleanUpBase()
        {
            RunExtenderAction<AutomatedTests>(e => e.TearDown(), true);

            if (testActionContext != null)
            {
                testActionContext.Dispose();
            }

            if (TestInitialized)
            {
                CleanUpBeforeNextTest();
            }

            mLastCleanUpSucceeded = true;
        }


        /// <summary>
        /// One time clean up after all tests in test fixture are run
        /// </summary>
        [OneTimeTearDown]
        public void CleanUpFixtureBase()
        {
            RunExtenderAction<AutomatedTests>(e => e.FixtureTearDown(), true);

            TestsDirectoryHelper.DeleteDirectoryIgnoreIOErrors(TemporaryAppPath);
            mTemporaryWebAppPath = null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the test extender
        /// </summary>
        public void AddExtender(TestExtender extender)
        {
            TestExtenders.Add(extender);
        }              


        /// <summary>
        /// Gets the current test extenders
        /// </summary>
        protected virtual List<TestExtender> GetTestExtenders()
        {
            return new List<TestExtender>();
        }


        /// <summary>
        /// Gets the current test extenders
        /// </summary>
        protected virtual void RunExtenderAction<TCurrentLevel>(Action<TestExtender> action, bool reversed = false)
            where TCurrentLevel : AutomatedTests
        {
            var extenders = TestExtenders;

            if (reversed)
            {
                extenders = ((IEnumerable<TestExtender>)extenders).Reverse().ToList();
            }

            for (int i = 0; i < extenders.Count; i++)
            {
                var extender = extenders[i];

                if (extender is TestExtender<TCurrentLevel>)
                {
                    action(extender);
                }
            }
        }


        /// <summary>
        /// Disables all functionality unnecessary during tests run. Such as events logging, synchronization and smart search tasks processing.
        /// </summary>
        private void DisableUnnecessaryFunctionality()
        {
            SearchIndexInfoProvider.SearchEnabled = false;
            ThreadQueueWorkerSettings.AllowEnqueue = false;

            // Note: CMSActionContext.DisableAll() is not used as it disables more functionality than automated tests need
            testActionContext = new CMSActionContext
            {
                LogSynchronization = false,
                LogExport = false,

                LogIntegration = false,
                EnableLogContext = false,
                LogWebFarmTasks = false,
                CreateVersion = false,

                AllowAsyncActions = false,

                SendEmails = false,
                SendNotifications = false,

                TouchParent = false,
                UseGlobalAdminContext = false,
                UpdateUserCounts = false,
                UseCacheForSynchronizationXMLs = false,

                EnableSmartSearchIndexer = false,
                CreateSearchTask = false
            };
        }


        /// <summary>
        /// Ensures selected assembly discovery is used in each <see cref="AutomatedTests"/> implementation (after <see cref="ResetAppState"/>).
        /// </summary>
        private void SetupTestsAssemblyDiscovery()
        {
            var limitAssemblyDiscovery = ValidationHelper.GetBoolean(TestsConfig.GetTestAppSetting("CMSLimitAssemblyDiscovery"), false);
            if (limitAssemblyDiscovery)
            {
                TestsAssemblyDiscoveryHelper.EnsureTestsAssemblyDiscoveryUsed(GetType().Assembly);
            }
        }


        /// <summary>
        /// Resets all current contexts
        /// </summary>
        private static void ResetContexts()
        {
            StaticContext.Reset();
            ThreadContext.Reset();
            SystemContext.WebApplicationPhysicalPath = null;
        }


        /// <summary>
        /// Fakes the event log provider to attach additional actions for test behavior
        /// </summary>
        private void FakeEventLogProvider()
        {
            var eventLogProvider = new FakeEventLogProvider
            {
                LogEventsToDatabase = false
            };

            eventLogProvider.SetAsDefaultProvider();
            TestsEventLogProvider = eventLogProvider;

            Service.Use<IEventLogService, FakeEventLogService>();

            ((ITestEventLogService)CoreServices.EventLog).TestsEventLogProvider = eventLogProvider;
        }


        /// <summary>
        /// Loads the current test name into the context
        /// </summary>
        private void LoadTestName()
        {
            LastTestName = CurrentTestName;
            CurrentTestName = TestContext.CurrentContext.Test.Name;
        }


        private void CleanUpBeforeNextTest()
        {
            EndApplication();

            // Stop all running threads and do not allow any new threads
            CMSThread.StopAllThreads(false, true);

            ResetContexts();

            ClearSystem();

            TestsDirectoryHelper.DeleteDirectoryIgnoreIOErrors(TemporaryAppPath);
            mTemporaryWebAppPath = null;
        }


        private void ClearSystem()
        {
            // Clear cache
            ClearCache();

            // Clear registered routes
            RouteTable.Routes.Clear();
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
        }


        /// <summary>
        /// Clear cache
        /// </summary>
        private void ClearCache()
        {
            var keyList = new List<string>();
            var enumerator = HttpRuntime.Cache.GetEnumerator();

            // Get cache items
            while (enumerator.MoveNext())
            {
                keyList.Add(enumerator.Key.ToString());
            }

            // Remove the items
            foreach (string key in keyList)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }


        /// <summary>
        /// Cleans up the data context
        /// </summary>
        protected static void CleanUpDataContext()
        {
            ConnectionHelper.Clear();

            SettingsHelper.ConnectionStrings.Clear();

            ModuleManager.ClearHashtables(false);
        }


        /// <summary>
        /// Milestone
        /// </summary>
        protected void TestMilestone()
        {
            var extenders = TestExtenders;

            for (int i = 0; i < extenders.Count; i++)
            {
                var extender = extenders[i];

                extender.TestMilestone();
            }
        }

        #endregion


        #region "App state reset"

        /// <summary>
        /// Creates the reset instance
        /// </summary>
        private AppStateReset CreateReset()
        {
            var reset = new AppStateReset();

            var originalCondition = reset.AssemblyCondition ?? (asm => true);

            reset.AssemblyCondition = assembly =>
                originalCondition(assembly) && // Include default condition
                (assembly != typeof(AppStateReset).Assembly) && // Exclude Tests assembly
                (assembly != GetType().Assembly); // Exclude current test assembly from reset

            return reset;
        }


        /// <summary>
        /// Resets the application state in case the application already went through pre-initialization phase. Does not perform reset for the first executed test.
        /// </summary>
        private void AutomaticAppStateReset()
        {
            if (!FirstTestInDomain)
            {
                // Make sure cleanup is done if previous test tear down failed
                if (!mLastCleanUpSucceeded)
                {
                    CleanUpBeforeNextTest();
                }

                SetupTestsAssemblyDiscovery();

                ResetAppState();

                SetupTestsAssemblyDiscovery();

                // After app reset the state is not initialized
                ApplicationInitialized = false;
            }
        }


        /// <summary>
        /// Resets the application state
        /// </summary>
        public void ResetAppState()
        {
            mReset.Reset();
        }


        /// <summary>
        /// Reset the application state (if not first test fixture)
        /// </summary>
        private void ResetAppStateForFixture()
        {
            if (!mFirstTestFixture)
            {
                ResetAppState();
            }
            else
            {
                mFirstTestFixture = false;
            }

            SetupTestsAssemblyDiscovery();
        }

        #endregion


        #region "App initialization"

        /// <summary>
        /// Performs application initialization if not already initialized
        /// </summary>
        protected virtual void InitApplication()
        {
            if (!ApplicationInitialized)
            {
                SystemContext.WebApplicationPhysicalPath = TemporaryAppPath;

                CMSApplication.Init();

                ApplicationInitialized = true;
            }
        }


        /// <summary>
        /// Performs application end 
        /// </summary>
        protected void EndApplication()
        {
            if (ApplicationInitialized)
            {
                CMSApplication.ApplicationEnd();

                ApplicationInitialized = false;
            }
        }

        #endregion
    }
}