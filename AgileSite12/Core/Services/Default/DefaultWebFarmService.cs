using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Core
{
    /// <summary>
    /// Default service to provide web farm synchronization
    /// </summary>
    internal class DefaultWebFarmService : IWebFarmService
    {
        /// <summary>
        /// Gets or sets value that indicates whether file synchronization is enabled.
        /// </summary>
        public bool SynchronizeFiles
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets value that indicates whether file synchronization for physical project files is enabled.
        /// </summary>
        public bool SynchronizePhysicalFiles
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets value that indicates whether file delete synchronization is enabled.
        /// </summary>
        public bool SynchronizeDeleteFiles
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets value that indicates whether web farm is enabled.
        /// </summary>
        public bool WebFarmEnabled
        {
            get
            {
                // Web farm reported as disabled by default
                return false;
            }
        }


        /// <summary>
        /// Gets the unique identifier of the server.
        /// </summary>
        public string ServerName
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="taskType">The type of task to register.</param>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="taskType"/> does not inherit <see cref="WebFarmTaskBase"/>.</exception>
        public void RegisterTask(Type taskType, bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None)
        {
            // Do nothing by default
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Indicates whether task number can be optimized by one of predefined ways.</param>
        public void RegisterTask<T>(bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None) where T : WebFarmTaskBase, new()
        {
            // Do nothing by default
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task with its data.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public bool CreateTask(WebFarmTaskBase task)
        {
            return false;
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task with its data.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public bool CreateIOTask(WebFarmTaskBase task)
        {
            return false;
        }


        /// <summary>
        /// Gets the list of names of servers to be updated. Current server is excluded.
        /// </summary>
        public IEnumerable<string> GetServerNamesToUpdate()
        {
            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets enumeration of names of all enabled web farm servers.
        /// </summary>
        public IEnumerable<string> GetEnabledServerNames()
        {
            return Enumerable.Empty<string>();
        }
    }
}
