using System;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm helper
    /// </summary>
    public class WebFarmHelper
    {
        /// <summary>
        /// Returns true if the web farm is enabled
        /// </summary>
        public static bool WebFarmEnabled
        {
            get
            {
                // Web farm reported as disabled by default
                return CoreServices.WebFarm.WebFarmEnabled;
            }
        }


        /// <summary>
        /// Returns unique identifier of the server
        /// </summary>
        public static string ServerName
        {
            get
            {
                return CoreServices.WebFarm.ServerName;
            }
        }


        /// <summary>
        /// Registers the given web farm task.
        /// </summary>
        /// <param name="task">Web farm task.</param>
        public static void RegisterTask(WebFarmTask task)
        {
            CoreServices.WebFarm.RegisterTask(task);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public static bool CreateTask(string taskType, string taskTarget = null, params string[] taskTextData)
        {
            return CoreServices.WebFarm.CreateTask(taskType, taskTarget, taskTextData);
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
        public static bool CreateIOTask(string taskType, string taskFilePath, BinaryData taskBinaryData = null, string taskTarget = null, params string[] taskTextData)
        {
            return CoreServices.WebFarm.CreateIOTask(taskType, taskFilePath, taskBinaryData, taskTarget, taskTextData);
        }


        /// <summary>
        /// Gets the list of names of servers to be updated. Current server is excluded.
        /// </summary>
        public static IEnumerable<string> GetServerNamesToUpdate()
        {
            return CoreServices.WebFarm.GetServerNamesToUpdate();
        }
    }
}
