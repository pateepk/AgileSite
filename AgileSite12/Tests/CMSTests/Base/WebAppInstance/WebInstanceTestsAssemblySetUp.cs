using System.Reflection;
using CMS.Helpers;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Base class for assembly set up for <see cref="WebAppInstanceTests"/>.
    /// </summary>
    public class WebInstanceTestsAssemblySetUp 
    {
        private const string WEB_APP_INSTACE_PORT_KEY = "CMSTestWebAppInstancePort";

        internal static IWebInstanceTestsEnvironmentManager Manager
        {
            get;
            private set;
        }


        private bool TestsExcluded
        {
            get
            {
                return !TestsCategoryCheck.CheckAssemblySetUp(GetType());
            }
        }


        /// <summary>
        /// Sets up tests environment for <see cref="AbstractWebAppInstanceTests"/>. Call this method in set up method marked with <see cref="OneTimeSetUpAttribute"/>.
        /// </summary>
        protected void SetUpEnvironment()
        {
            if (TestsExcluded) return;
            
            var port = ValidationHelper.GetInteger(TestsConfig.GetTestAppSetting(WEB_APP_INSTACE_PORT_KEY), 0);
            var instanceName = Assembly.GetCallingAssembly().GetName().Name;

            Manager = GetManager(instanceName, port);
            Manager.SetUp();
        }

        
        /// <summary>
        /// Cleans up tests environment for <see cref="AbstractWebAppInstanceTests"/>. Call this method in set up method marked with <see cref="OneTimeTearDownAttribute"/>.
        /// </summary>
        protected void CleanUpEnvironment()
        {
            if (TestsExcluded) return;

            Manager.CleanUp();
        }


        /// <summary>
        /// Initializes new instance of web app instance manager.
        /// </summary>
        /// <param name="instanceName"></param>
        /// <param name="port">The port to run the instance on.</param>
        /// <returns></returns>
        protected virtual IWebInstanceTestsEnvironmentManager GetManager(string instanceName, int port = 0)
        {
            return new WebInstanceTestsEnvironmentManager(instanceName, port);
        }
    }
}
