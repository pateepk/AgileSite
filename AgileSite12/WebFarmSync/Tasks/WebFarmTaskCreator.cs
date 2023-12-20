using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Creates <see cref="ThreadSettings"/> object with empty context.
        /// </summary>
        protected override ThreadSettings CreateThreadSettings()
        {
            var originalSettings = base.CreateThreadSettings();
            originalSettings.UseEmptyContext = true;
            return originalSettings;
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

            if (WebFarmContext.WebFarmEnabled)
            {
                // Create server bindings only when web farms are enabled. When web farms are disabled, create anonymous tasks.
                CreateServerBindings(firstGuid, items.Count());
            }

            if (WebFarmContext.CreateAnonymousTasks)
            {
                // Notify web application to process web farm tasks
                AnonymousTasksProcessor.NotifyServer();
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
            foreach (var item in items.Where(t => WebFarmTaskManager.GetOptimizeAction(t.TaskType) == WebFarmTaskOptimizeActionEnum.None))
            {
                yield return item;
            }

            foreach (var itemsGroup in items.Where(t => WebFarmTaskManager.GetOptimizeAction(t.TaskType) == WebFarmTaskOptimizeActionEnum.GroupData).GroupBy(t => t.TaskType))
            {
                yield return OptimizeGroup(itemsGroup.ToList());
            }
        }


        private static void CreateServerBindings(Guid firstGuid, int count)
        {
            var parameters = new QueryDataParameters
            {
                {"@FirstGuid", firstGuid},
                {"@Count", count},
                {"@CurrentServerId", WebFarmContext.ServerId}
            };

            ConnectionHelper.ExecuteQuery("cms.WebFarmTask.InsertServerBindings", parameters);
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
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        internal static bool CreateTask(WebFarmTaskBase task)
        {
            return Current.CreateTaskInternal(task);
        }


        /// <summary>
        /// Creates task.
        /// </summary>
        /// <param name="task">Data associated with the task.</param>
        /// <returns>Returns true if the task was created (web farm is set up and task was allowed)</returns>
        internal bool CreateTaskInternal(WebFarmTaskBase task)
        {
            if (!DatabaseHelper.IsDatabaseAvailable)
            {
                return false;
            }

            var taskInfo = new WebFarmTaskInfo
            {
                TaskBinaryData = task.TaskBinaryData?.Data,
                TaskMachineName = SystemContext.MachineName,
                TaskTarget = task.TaskTarget,
                TaskIsAnonymous = WebFarmContext.CreateAnonymousTasks,
                TaskTextData = WebFarmTaskManager.SerializeData(task),
                TaskType = task.GetType().FullName,
                TaskIsMemory = WebFarmTaskManager.IsMemoryTask(task.GetType().FullName),
                TaskCreated = DateTime.Now,
                TaskFilePath = task.TaskFilePath
            };

            if (!WebFarmTaskManager.CanCreateTask(taskInfo))
            {
                return false;
            }

            WebFarmDebug.LogWebFarmOperation(task.GetType().FullName, taskInfo.TaskTextData, task.TaskBinaryData, task.TaskTarget);
            Enqueue(taskInfo);

            return true;
        }
    }
}
