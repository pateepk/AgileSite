using System;
using System.Linq;

using CMS.Core;
using CMS.Base;
using CMS.IO;

namespace CMS.WebFarmSync
{
    using TaskDictionary = StringSafeDictionary<WebFarmTask>;

    /// <summary>
    /// Manager of registered web farm tasks
    /// </summary>
    internal class WebFarmTaskManager
    {
        /// <summary>
        /// Dictionary of registered tasks [taskType -> WebFarmTask]
        /// </summary>
        internal static TaskDictionary Tasks = new TaskDictionary();


        /// <summary>
        /// Registers the given web farm task
        /// </summary>
        /// <param name="task">Task to register</param>
        /// <exception cref="System.ArgumentException">Thrown when task's execute action is not specified</exception>
        internal static void RegisterTask(WebFarmTask task)
        {
            if (task.Execute == null)
            {
                throw new ArgumentException("[WebFarmTaskManager.RegisterTask]: Task must have an Execute action specified.");
            }

            Tasks[task.Type] = task;
        }


        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Task target</param>
        /// <param name="taskData">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        internal static bool ExecuteTask(string taskType, string taskTarget, string[] taskData, BinaryData binaryData)
        {
            var task = Tasks[taskType];
            if (task == null)
            {
                return false;
            }

            task.Execute(taskTarget, taskData, binaryData);

            return true;
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task.
        /// </summary>
        /// <param name="task">Task to be checked.</param>
        internal static bool CanCreateTask(WebFarmTaskInfo task)
        {
            if (!CanCreateTaskType(task) || 
                task.TaskBinaryData != null && (task.TaskBinaryData.Length > WebFarmContext.MaxWebFarmFileSize) ||
                !String.IsNullOrEmpty(task.TaskFilePath) && StorageHelper.IsSharedStorage(task.TaskFilePath))
            {
                return false;
            }

            if (task.TaskIsAnonymous)
            {
                return WebFarmContext.CreateAnonymousTasks;
            }

            // Check for normal tasks considering the last update before web farm shutdown
            return WebFarmContext.CreateWebFarmTasks || WebFarmTaskCreator.Current.IsThreadRunning();
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task of specified type.
        /// </summary>
        /// <param name="taskInfo">Task info object.</param>
        /// <exception cref="System.NotSupportedException">Thrown when task type was not registered to the system using WebFarmHelper.RegisterTask method</exception>
        internal static bool CanCreateTaskType(IWebFarmTask taskInfo)
        {
            var task = Tasks[taskInfo.TaskType];
            if (task == null)
            {
                throw new NotSupportedException("[WebFarmTaskManager.CanCreateTask]: Task type '" + taskInfo.TaskType + "' is not supported. The task needs to be registered with WebFarmHelper.RegisterTask method.");
            }

            if (task.Condition == null)
            {
                return true;
            }

            return task.Condition(taskInfo);
        }


        /// <summary>
        /// Indicates if task serves only for memory synchronization.
        /// </summary>
        /// <param name="taskType">Type of the task to check</param>
        internal static bool IsMemoryTask(string taskType)
        {
            var task = Tasks[taskType];
            return (task != null) && task.IsMemoryTask;
        }
    }
}
