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
        /// <param name="type">Type of the web farm task.</param>
        /// <param name="isMemoryTask">Indicates if the task modifies only memory. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> does not inherit <see cref="WebFarmTaskBase"/> or does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task with type <paramref name="type"/> has been already registered.</exception>
        public static void RegisterTask(Type type, bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None)
        {
            CoreServices.WebFarm.RegisterTask(type);
        }


        /// <summary>
        /// Registers the given web farm task.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies only memory. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentException">Thrown when task to be registered does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task has been already registered.</exception>
        public static void RegisterTask<T>(bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None) where T : WebFarmTaskBase, new()
        {
            CoreServices.WebFarm.RegisterTask<T>(isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task with its data to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public static bool CreateTask(WebFarmTaskBase task)
        {
            return CoreServices.WebFarm.CreateTask(task);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Web farm task with its data to be created.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        public static bool CreateIOTask(WebFarmTaskBase task)
        {
            return CoreServices.WebFarm.CreateIOTask(task);
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
