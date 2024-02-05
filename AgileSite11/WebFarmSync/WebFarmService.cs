using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Service to provide web farm synchronization
    /// </summary>
    internal class WebFarmService : IWebFarmService
    {
        #region "Properties - Settings - Synchronization"

        /// <summary>
        /// Gets or sets value that indicates whether file synchronization is enabled.
        /// </summary>
        public bool SynchronizeFiles
        {
            get
            {
                // Don't synchronize files when running on Azure since Azure has shared storage
                return (!SystemContext.IsRunningOnAzure && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeFiles", "CMSWebFarmSynchronizeFiles", true));
            }
        }


        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for physical project files is enabled.
        /// </summary>
        public bool SynchronizePhysicalFiles
        {
            get
            {
                return (SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizePhysicalFiles", "CMSWebFarmSynchronizePhysicalFiles", true));
            }
        }


        /// <summary>
        /// Gets or sets value that indicates whether file delete synchronization is enabled.
        /// </summary>
        public bool SynchronizeDeleteFiles
        {
            get
            {
                // Don't synchronize files when running on Azure since Azure has shared storage
                return (!SystemContext.IsRunningOnAzure && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeDeleteFiles", "CMSWebFarmSynchronizeDeleteFiles", true));
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the web farm is enabled
        /// </summary>
        public bool WebFarmEnabled
        {
            get
            {
                return WebFarmContext.WebFarmEnabled;
            }
        }


        /// <summary>
        /// Returns unique identifier of the server
        /// </summary>
        public string ServerName
        {
            get
            {
                return WebFarmContext.ServerName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the given web farm task.
        /// </summary>
        /// <param name="task">Web farm task.</param>
        public void RegisterTask(WebFarmTask task)
        {
            WebFarmTaskManager.RegisterTask(task);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public bool CreateTask(string taskType, string taskTarget = null, params string[] taskTextData)
        {
            return WebFarmTaskCreator.CreateTask(taskType, taskTarget, taskTextData);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskFilePath">Task file path.</param>
        /// <param name="taskBinaryData">Task binary data.</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public bool CreateIOTask(string taskType, string taskFilePath, BinaryData taskBinaryData = null, string taskTarget = null, params string[] taskTextData)
        {
            return WebFarmTaskCreator.CreateTask(taskType, taskTarget, taskTextData, taskBinaryData, taskFilePath);
        }


        /// <summary>
        /// Gets the list of names of servers to be updated. Current server is excluded.
        /// </summary>
        public IEnumerable<string> GetServerNamesToUpdate()
        {
            return WebFarmContext.ServersToUpdate.Select(s => s.ServerName);
        }


        /// <summary>
        /// Gets enumeration of names of all web farm servers.
        /// </summary>
        public IEnumerable<string> GetEnabledServerNames()
        {
            return WebFarmContext.EnabledServers.Select(s => s.ServerName);
        }

        #endregion
    }
}
