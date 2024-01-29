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
        /// Gets or sets value that indicates whether file delete synchronization is enabled.
        /// </summary>
        public bool SynchronizeDeleteFiles
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Returns true if the web farm is enabled.
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
        /// Returns unique identifier of the server.
        /// </summary>
        public string ServerName
        {
            get
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Registers the given web farm task.
        /// </summary>
        /// <param name="task">Web farm task.</param>
        public void RegisterTask(WebFarmTask task)
        {
            // Do nothing by default
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
            return false;
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
