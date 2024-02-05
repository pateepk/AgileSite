using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Asynchronous thread for bulk creating web farm tasks.
    /// </summary>
    internal sealed class WebFarmTaskCreator : ThreadQueueWorker<WebFarmTaskInfo, WebFarmTaskCreator>
    {
        /// <summary>
        /// Default interval of inserting cycle.
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return WebFarmContext.SyncInterval;
            }
        }


        /// <summary>
        /// Method that inserts all the tasks.
        /// </summary>
        /// <param name="items">Collection of tasks to be inserted.</param>
        protected override int ProcessItems(IEnumerable<WebFarmTaskInfo> items)
        {
            items = OptimizeItems(items.ToList()).ToList();
            if (!items.Any())
            {
                return 0;
            }

            var firstGuid = items.First().TaskGUID = Guid.NewGuid();
            WebFarmTaskInfoProvider.ProviderObject.BulkInsertInfos(items);

            var parameters = new QueryDataParameters
            {
                {"@FirstGuid", firstGuid},
                {"@Count", items.Count()},
                {"@CurrentServerId", WebFarmContext.ServerId}
            };
            ConnectionHelper.ExecuteQuery("cms.WebFarmTask.InsertServerBindings", parameters);

            if (WebFarmContext.CreateAnonymousTasks)
            {
                // Notify web application to process web farm tasks
                AnonymousTasksProcesor.NotifyServer(AnonymousTasksProcesor.GetNotificationPath(SystemContext.WebApplicationPhysicalPath));
            }
            else if (!WebFarmContext.CreateWebFarmTasks)
            {
                StopExecution();
            }

            return items.Count();
        }


        /// <summary>
        /// Optimize number of generated tasks according to optimization available for particular task type.
        /// </summary>
        /// <remarks>Optimizable tasks will be processed after non-optimizable.</remarks>
        /// <remarks>Non-optimizable tasks preserves the order.</remarks>
        /// <param name="items">Collection of non-optimized web farm tasks to store to database</param>
        /// <returns>Optimized collection of web farm tasks to store to database</returns>
        internal IEnumerable<WebFarmTaskInfo> OptimizeItems(IList<WebFarmTaskInfo> items)
        {
            foreach (var item in items.Where(t => WebFarmTaskManager.Tasks[t.TaskType].OptimizationType == WebFarmTaskOptimizeActionEnum.None))
            {
                yield return item;
            }

            foreach (var itemsGroup in items.Where(t => WebFarmTaskManager.Tasks[t.TaskType].OptimizationType == WebFarmTaskOptimizeActionEnum.GroupData).GroupBy(t => t.TaskType))
            {
                yield return OptimizeGroup(itemsGroup.ToList());
            }
        }


        /// <summary>
        /// Optimize group of tasks with the same type.
        /// </summary>
        /// <param name="items">Collection of non-optimized web farm tasks with the same type</param>
        /// <returns>Web farm task contains optimized collection of web farm tasks</returns>
        private static WebFarmTaskInfo OptimizeGroup(IList<WebFarmTaskInfo> items)
        {
            var first = items.First();

            first.TaskTextData = String.Join(WebFarmTaskInfo.MULTIPLE_TASK_DATA_SEPARATOR, items.Select(t => t.TaskTextData));
            first.TaskTarget = String.Join(WebFarmTaskInfo.MULTIPLE_TASK_DATA_SEPARATOR, items.Select(t => t.TaskTarget));

            return first;
        }


        /// <summary>
        /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
        /// </summary>
        protected override void Finish()
        {
            // Run the process for the one last time
            RunProcess();
        }


        /// <summary>
        /// Processing of single item. In this implementation this method is not called at all.
        /// </summary>
        /// <param name="item">Item to be processed.</param>
        protected override void ProcessItem(WebFarmTaskInfo item)
        {
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <param name="taskBinaryData">Task binary data (for attachments etc.)</param>
        /// <param name="taskFilePath">Task file path.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        internal static bool CreateTask(string taskType, string taskTarget = null, string[] taskTextData = null, BinaryData taskBinaryData = null, string taskFilePath = null)
        {
            return Current.CreateTaskInternal(taskType, taskTarget, taskTextData, taskBinaryData, taskFilePath);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="taskTarget">Target</param>
        /// <param name="taskTextData">Task text data (such as code name, ID etc.)</param>
        /// <param name="taskBinaryData">Task binary data (for attachments etc.)</param>
        /// <param name="taskFilePath">Task file path.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        internal bool CreateTaskInternal(string taskType, string taskTarget = null, string[] taskTextData = null, BinaryData taskBinaryData = null, string taskFilePath = null)
        {
            if (!DatabaseHelper.IsDatabaseAvailable)
            {
                return false;
            }

            var task = new WebFarmTaskInfo
            {
                TaskBinaryData = (taskBinaryData != null) ? taskBinaryData.Data : null,
                TaskMachineName = SystemContext.MachineName,
                TaskTarget = taskTarget,
                TaskIsAnonymous = WebFarmContext.CreateAnonymousTasks,
                TaskTextData = PrepareTaskTextData(taskTextData),
                TaskType = taskType,
                TaskIsMemory = WebFarmTaskManager.IsMemoryTask(taskType),
                TaskCreated = DateTime.Now,
                TaskFilePath = taskFilePath
            };

            if (!WebFarmTaskManager.CanCreateTask(task))
            {
                return false;
            }

            WebFarmDebug.LogWebFarmOperation(taskType, task.TaskTextData, taskBinaryData, taskTarget);
            Enqueue(task);

            return true;
        }


        /// <summary>
        /// Serialize web farm task text data.
        /// </summary>
        /// <param name="taskData">Task data to be serialized</param>
        internal static string PrepareTaskTextData(string[] taskData)
        {
            if ((taskData == null) || !taskData.Any())
            {
                return String.Empty;
            }

            return String.Join(WebFarmTaskInfo.TASK_DATA_SEPARATOR, taskData);
        }
    }
}
