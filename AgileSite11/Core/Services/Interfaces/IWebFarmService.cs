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
        /// Registers the given web farm task.
        /// </summary>
        /// <param name="task">Web farm task.</param>
        void RegisterTask(WebFarmTask task);


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        bool CreateTask(string taskType, string taskTarget = null, params string[] taskTextData);


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskFilePath">Task file path.</param>
        /// <param name="taskBinaryData">Task binary data.</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        bool CreateIOTask(string taskType, string taskFilePath, BinaryData taskBinaryData = null, string taskTarget = null, params string[] taskTextData);


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
