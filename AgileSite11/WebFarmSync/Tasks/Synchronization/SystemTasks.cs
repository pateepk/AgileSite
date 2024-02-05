using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm synchronization for system
    /// </summary>
    internal class SystemTasks
    {
        #region "Methods"

        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SystemTaskType.RestartApplication,
                Execute = RestartApplication,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SystemTaskType.ClearWebFarmContext,
                Execute = ClearWebFarmServers,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Restarts the application
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RestartApplication(string target, string[] data, BinaryData binaryData)
        {
            SystemHelper.RestartApplication(SystemContext.WebApplicationPhysicalPath);
        }


        /// <summary>
        /// Clears the list of web farm servers
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearWebFarmServers(string target, string[] data, BinaryData binaryData)
        {
            WebFarmContext.Clear(false);
        }

        #endregion
    }
}
