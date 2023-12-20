using System;
using System.Collections.Generic;

namespace CMS.Core
{
    /// <summary>
    /// Web farm service interface
    /// </summary>
    public interface IWebFarmService
    {
        /// <summary>
        /// Gets or sets value that indicates whether file synchronization is enabled.
        /// </summary>
        bool SynchronizeFiles
        {
            get;
        }


        /// <summary>
        /// Gets value that indicates whether file synchronization for physical project files is enabled.
        /// </summary>
        bool SynchronizePhysicalFiles
        {
            get;
        }


        /// <summary>
        /// Gets or sets value that indicates whether file delete synchronization is enabled.
        /// </summary>
        bool SynchronizeDeleteFiles
        {
            get;
        }


        /// <summary>
        /// Returns true if the web farm is enabled.
        /// </summary>
        bool WebFarmEnabled
        {
            get;
        }


        /// <summary>
        /// Returns unique identifier of the server.
        /// </summary>
        string ServerName
        {
            get;
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="taskType">The type of task to register.</param>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="taskType"/> does not inherit <see cref="WebFarmTaskBase"/> or does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task with type <paramref name="taskType"/> has been already registered.</exception>
        void RegisterTask(Type taskType, bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None);


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentException">Thrown when task to be registered does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task has been already registered.</exception>
        void RegisterTask<T>(bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None) where T : WebFarmTaskBase, new();


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        bool CreateTask(WebFarmTaskBase task);


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed).</returns>
        bool CreateIOTask(WebFarmTaskBase task);


        /// <summary>
        /// Gets the list of names of servers to be updated. Current server is excluded.
        /// </summary>
        IEnumerable<string> GetServerNamesToUpdate();


        /// <summary>
        /// Gets enumeration of names of all enabled web farm servers.
        /// </summary>
        IEnumerable<string> GetEnabledServerNames();
    }
}
