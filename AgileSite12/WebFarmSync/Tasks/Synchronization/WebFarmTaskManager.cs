using System;

using CMS.Core;
using CMS.Base;
using CMS.Helpers;
using CMS.IO;

using Newtonsoft.Json;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Manager of registered web farm tasks
    /// </summary>
    internal class WebFarmTaskManager
    {
        private static readonly Lazy<WebFarmTaskManager> instance = new Lazy<WebFarmTaskManager>(() => new WebFarmTaskManager());


        /// <summary>
        /// Gets current instance of the <see cref="WebFarmTaskManager"/> class.
        /// </summary>
        internal static WebFarmTaskManager Instance => instance.Value;


        /// <summary>
        /// Represents registered web farm task.
        /// </summary>
        private class WebFarmTaskConfiguration
        {
            /// <summary>
            /// Gets or sets the type of the task.
            /// </summary>
            public Type TaskType { get; set; }


            /// <summary>
            /// Gets or sets a value that indicates whether the task modifies only memory.
            /// Memory tasks can be deleted on application start.
            /// </summary>
            public bool IsMemoryTask { get; set; }


            /// <summary>
            /// Gets or sets an optimization action which is used for reducing the number of generated web farm tasks of particular type.
            /// </summary>
            public WebFarmTaskOptimizeActionEnum OptimizeAction { get; set; }
        }


        /// <summary>
        /// Dictionary of registered tasks indexed by task type.
        /// </summary>
        private readonly StringSafeDictionary<WebFarmTaskConfiguration> Tasks = new StringSafeDictionary<WebFarmTaskConfiguration>();


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="taskType">The type of task to register.</param>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="taskType"/> does not inherit <see cref="WebFarmTaskBase"/> or does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task with type <paramref name="taskType"/> has been already registered.</exception>
        internal static void RegisterTask(Type taskType, bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None)
        {
            Instance.RegisterTaskInternal(taskType, isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="InvalidOperationException">Thrown when task has been already registered.</exception>
        internal static void RegisterTask<T>(bool isMemoryTask = false, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize = WebFarmTaskOptimizeActionEnum.None) where T : WebFarmTaskBase, new()
        {
            Instance.RegisterTaskInternal<T>(isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="WebFarmTaskBase.ExecuteTask()"/> method while passing it current class' properties as its parameters.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Task target</param>
        /// <param name="taskData">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        internal static bool ExecuteTask(string taskType, string taskTarget, string taskData, BinaryData binaryData)
        {
            return Instance.ExecuteTaskInternal(taskType, taskTarget, taskData, binaryData);
        }


        /// <summary>
        /// Serializes the task with its data.
        /// </summary>
        /// <param name="data">Data to be serialized or null.</param>
        /// <returns>Returns serialized <paramref name="data"/> or null when <paramref name="data"/> are null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="data"/> are of type which is incompatible with task type or when serialization fails.</exception>
        internal static string SerializeData(WebFarmTaskBase data)
        {
            return Instance.SerializeDataInternal(data);
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task.
        /// </summary>
        /// <param name="task">Task to be checked.</param>
        internal static bool CanCreateTask(WebFarmTaskInfo task)
        {
            return Instance.CanCreateTaskInternal(task);
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task of specified type.
        /// </summary>
        /// <param name="taskInfo">Task info object.</param>
        /// <exception cref="NotSupportedException">Thrown when task type was not registered to the system using WebFarmHelper.RegisterTask method</exception>
        internal static bool CanCreateTaskType(IWebFarmTask taskInfo)
        {
            return Instance.CanCreateTaskTypeInternal(taskInfo);
        }


        /// <summary>
        /// Indicates if task serves only for memory synchronization.
        /// </summary>
        /// <param name="taskType">Type of the task to check</param>
        internal static bool IsMemoryTask(string taskType)
        {
            return Instance.IsMemoryTaskInternal(taskType);
        }


        /// <summary>
        /// Returns optimization action for given <paramref name="taskType"/> to reduce number of created web farm tasks in database.
        /// </summary>
        /// <param name="taskType">Type of the task for which to retrieve its <see cref="WebFarmTaskOptimizeActionEnum"/>.</param>
        internal static WebFarmTaskOptimizeActionEnum GetOptimizeAction(string taskType)
        {
            return Instance.GetOptimizeActionInternal(taskType);
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
        protected virtual void RegisterTaskInternal(Type taskType, bool isMemoryTask, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize)
        {
            if (taskType == null)
            {
                throw new ArgumentNullException(nameof(taskType));
            }

            if (!typeof(WebFarmTaskBase).IsAssignableFrom(taskType))
            {
                throw new ArgumentException($"Task type '{taskType}' is required to be inherited from '{typeof(WebFarmTaskBase)}'.");
            }

            if (taskType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException($"Task type '{taskType}' is required to have parameterless constructor.");
            }

            if (Tasks.ContainsKey(taskType.FullName))
            {
                throw new InvalidOperationException($"Task type '{taskType}' has been already registered.");
            }

            Tasks[taskType.FullName] = new WebFarmTaskConfiguration
            {
                TaskType = taskType,
                IsMemoryTask = isMemoryTask,
                OptimizeAction = webFarmTaskOptimize
            };
        }


        /// <summary>
        /// Registers a new web farm task type to allow its processing within the system. The type must inherit <see cref="WebFarmTaskBase"/>.
        /// </summary>
        /// <param name="isMemoryTask">Indicates if the task modifies memory only. Memory tasks can be deleted on application start.</param>
        /// <param name="webFarmTaskOptimize">Determines type of optimization which is used for reducing number of generated web farm tasks of particular type.</param>
        /// <exception cref="ArgumentException">Thrown when task to be registered does not have parameterless constructor.</exception>
        /// <exception cref="InvalidOperationException">Thrown when task has been already registered.</exception>
        protected virtual void RegisterTaskInternal<T>(bool isMemoryTask, WebFarmTaskOptimizeActionEnum webFarmTaskOptimize) where T : WebFarmTaskBase, new()
        {
            RegisterTask(typeof(T), isMemoryTask, webFarmTaskOptimize);
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="WebFarmTaskBase.ExecuteTask()"/> method while passing it current class' properties as its parameters.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Task target</param>
        /// <param name="taskData">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        protected virtual bool ExecuteTaskInternal(string taskType, string taskTarget, string taskData, BinaryData binaryData)
        {
            var task = Tasks[taskType];
            if (task == null)
            {
                return false;
            }

            try
            {
                WebFarmTaskBase webFarmTask = GetWebFarmTaskWithData(task.TaskType, taskTarget, taskData, binaryData, "");
                webFarmTask.ExecuteTask();
            }
            catch (JsonReaderException ex)
            {
                EventLog.EventLogProvider.LogException("Synchronization", "EXECUTETASK", ex, SiteProvider.SiteContext.CurrentSiteID, "Logged web farm task data are not deserialized correctly.");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task.
        /// </summary>
        /// <param name="task">Task to be checked.</param>
        protected virtual bool CanCreateTaskInternal(WebFarmTaskInfo task)
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
        /// Serializes the task with its data.
        /// </summary>
        /// <param name="data">Data to be serialized or null.</param>
        /// <returns>Returns serialized <paramref name="data"/> or null when <paramref name="data"/> are null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="data"/> are of type which is incompatible with task type or when serialization fails.</exception>
        protected virtual string SerializeDataInternal(WebFarmTaskBase data)
        {
            var task = Tasks[data.GetType().FullName];
            if (task == null)
            {
                throw new NotSupportedException($"Task type '{data.GetType().FullName}' is not supported. The task needs to be registered using {typeof(WebFarmHelper).FullName}.{nameof(RegisterTask)} method.");
            }

            return JsonConvert.SerializeObject(data);
        }


        /// <summary>
        /// Returns true if the system is allowed to create the task of specified type.
        /// </summary>
        /// <param name="taskInfo">Task info object.</param>
        /// <exception cref="NotSupportedException">Thrown when task type was not registered to the system using WebFarmHelper.RegisterTask method</exception>
        protected virtual bool CanCreateTaskTypeInternal(IWebFarmTask taskInfo)
        {
            var task = Tasks[taskInfo.TaskType];
            if (task == null)
            {
                throw new NotSupportedException($"Task type '{taskInfo.TaskType}' is not supported. The task needs to be registered with {typeof(WebFarmHelper).FullName}.{nameof(RegisterTask)} method.");
            }

            try
            {
                var webFarmTask = GetWebFarmTaskWithData(task.TaskType, taskInfo.TaskTarget, taskInfo.TaskTextData, taskInfo.TaskBinaryData, taskInfo.TaskFilePath);
                return webFarmTask.ConditionMethod();
            }
            catch (JsonReaderException ex)
            {
                EventLog.EventLogProvider.LogException("Synchronization", "CANCREATETASK", ex, SiteProvider.SiteContext.CurrentSiteID, "Logged web farm task data are not deserialized correctly.");
                return false;
            }
        }


        /// <summary>
        /// Indicates if task serves only for memory synchronization.
        /// </summary>
        /// <param name="taskType">Type of the task to check</param>
        protected virtual bool IsMemoryTaskInternal(string taskType)
        {
            var task = Tasks[taskType];
            if (task == null)
            {
                return false;
            }

            return Tasks[taskType].IsMemoryTask;
        }


        /// <summary>
        /// Returns optimization action for given <paramref name="taskType"/> to reduce number of created web farm tasks in database.
        /// </summary>
        /// <param name="taskType">Type of the task for which to retrieve its <see cref="WebFarmTaskOptimizeActionEnum"/>.</param>
        protected virtual WebFarmTaskOptimizeActionEnum GetOptimizeActionInternal(string taskType)
        {
            var task = Tasks[taskType];
            if (task == null)
            {
                return WebFarmTaskOptimizeActionEnum.None;
            }

            return Tasks[taskType].OptimizeAction;
        }


        /// <summary>
        /// Returns web farm task with deserialized data if any.
        /// </summary>
        /// <param name="taskType">Data type of the task.</param>
        /// <param name="taskTarget">Task target.</param>
        /// <param name="taskData">Task custom data.</param>
        /// <param name="binaryData">Task binary data.</param>
        /// <param name="taskFilePath">Task file path.</param>
        private WebFarmTaskBase GetWebFarmTaskWithData(Type taskType, string taskTarget, string taskData, BinaryData binaryData, string taskFilePath)
        {
            var webFarmTask = (String.IsNullOrEmpty(taskData) ? Activator.CreateInstance(taskType) : JsonConvert.DeserializeObject(taskData, taskType)) as WebFarmTaskBase;

            webFarmTask.TaskBinaryData = binaryData;
            webFarmTask.TaskTarget = taskTarget;
            webFarmTask.TaskFilePath = taskFilePath;

            return webFarmTask;
        }
    }
}
