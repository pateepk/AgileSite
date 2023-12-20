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


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="taskType">The type of task to register.</param>
        /// <param name="isMemoryTask">Indicates if the task modifies only memory. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="taskType"/> does not inherit <see cref="WebFarmTaskBase"/> or does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task with type <paramref name="taskType"/> has been already registered.</exception>
        public void RegisterTask(Type taskType, bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None)
        {
            WebFarmTaskManager.RegisterTask(taskType, isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentException">Thrown when task to be registered does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task has been already registered.</exception>
        public void RegisterTask<T>(bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None) where T : WebFarmTaskBase, new()
        {
            WebFarmTaskManager.RegisterTask<T>(isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public bool CreateTask(WebFarmTaskBase task)
        {
            return WebFarmTaskCreator.CreateTask(task);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed).</returns>
        public bool CreateIOTask(WebFarmTaskBase task)
        {
            return WebFarmTaskCreator.CreateTask(task);
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
    }
}
