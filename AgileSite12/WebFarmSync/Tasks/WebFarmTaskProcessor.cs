using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Processes web farm tasks.
    /// </summary>
    internal sealed class WebFarmTaskProcessor : ThreadWorker<WebFarmTaskProcessor>
    {
        #region "Variables"

        // Logging policy with 15 minutes period
        private static readonly Lazy<LoggingPolicy> webFarmloggingPolicy = new Lazy<LoggingPolicy>(() =>
        {
            return new LoggingPolicy(TimeSpan.FromMinutes(15));
        });
        private static int mTaskBatchSize;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the interval in milliseconds for the worker.
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return WebFarmContext.SyncInterval;
            }
        }


        /// <summary>
        /// 5 minutes maintenance interval.
        /// </summary>
        protected override int MaintenanceInterval
        {
            get
            {
                return 300000;
            }
        }


        /// <summary>
        /// Number of tasks that should be processed in one batch.
        /// </summary>
        internal static int TaskBatchSize
        {
            get
            {
                return mTaskBatchSize > 0 ? mTaskBatchSize : 10000;
            }
            set
            {
                mTaskBatchSize = value;
            }
        }

        #endregion


        #region "Methods"

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
        /// Maintenance clearing unneeded tasks.
        /// </summary>
        protected override void DoMaintenance()
        {
            try
            {
                WebFarmTaskInfoProvider.DeleteOrphanedTasks();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }


        /// <summary>
        /// Method processing actions.
        /// </summary>
        protected override void Process()
        {
            try
            {
                WebFarmServerInfoProvider.EnsureAutomaticServer();
                CheckNewTasks();
                if (!WebFarmContext.WebFarmEnabled)
                {
                    StopExecution();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }


        /// <summary>
        /// Checks periodically for notification flag and processes web farm tasks if necessary.
        /// </summary>
        internal void CheckNewTasks()
        {
            WebFarmDebug.DebugCurrentRequest = WebFarmDebug.Settings.Enabled;

            if (!WebFarmServerTaskInfoProvider.GetWebFarmServerTasksInternal()
                .WhereEquals("ServerId", WebFarmContext.ServerId)
                .WhereNull("ErrorMessage")
                .TopN(1)
                .HasResults())
            {
                return;
            }

            Func<DataSet> getTasks = () => GetServerTasks();

            // Check license
            if (!WebFarmLicenseHelper.IsWebFarmLicenseValid())
            {
                getTasks = () => GetSystemTasks();

                var message = ResHelper.GetString("webfarm.unsufficientdomainlicense");
                EventLogProvider.LogEvent(EventType.ERROR, "WebFarmTaskProcessor", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, loggingPolicy: webFarmloggingPolicy.Value);
            }

            while (ProcessTasks(getTasks(), tasks => WebFarmTaskInfoProvider.DeleteServerTasks(WebFarmContext.ServerId, tasks)))
            {
            }
        }


        /// <summary>
        /// Returns dataset with tasks for specified server with additional information about presence of binary data of the tasks.
        /// </summary>
        private ObjectQuery<WebFarmTaskInfo> GetServerTasks()
        {
            var q = new ObjectQuery<WebFarmTaskInfo>().From(
                new QuerySource(
                    new QuerySourceTable(new ObjectSource<WebFarmTaskInfo>(), "T", SqlHints.NOLOCK)).Join(
                        new QuerySourceTable(new ObjectSource<WebFarmServerTaskInfo>(), "ST", SqlHints.NOLOCK),
                        "TaskID", "TaskID"));

            return q.Columns("T.TaskID", "TaskType", "TaskTextData", "TaskTarget", "CAST(CASE WHEN TaskBinaryData IS NULL THEN 0 ELSE 1 END AS bit) as TaskHasBinaryData")
                .WhereEquals("ServerID", WebFarmContext.ServerId)
                .WhereNull("ErrorMessage")
                .TopN(TaskBatchSize);
        }


        /// <summary>
        /// Returns all system tasks for the given server. System tasks are executed even when license check for web farms does not pass.
        /// </summary>
        /// <remarks>Web farm task type is registered by its full name. Therefore retrieving specific task is executed with 'typeof(CLASSNAME).FullName'.</remarks>
        private ObjectQuery<WebFarmTaskInfo> GetSystemTasks()
        {
            return GetServerTasks()
                .Where(w => w
                    .WhereContains("TaskTextData", WebFarmServerInfo.OBJECT_TYPE)
                    .Or()
                    .WhereContains("TaskTarget", WebFarmServerInfo.OBJECT_TYPE)
                    .Or()
                    .WhereContains("TaskTarget", typeof(RestartApplicationWebFarmTask).FullName)
                    .Or()
                    .WhereContains("TaskTarget", LicenseKeyInfo.OBJECT_TYPE)
                    .Or()
                    .WhereContains("TaskTarget", SiteInfo.OBJECT_TYPE)
                    .Or()
                    .Where(w2 => w2
                        .WhereEquals("TaskType", typeof(TouchCacheItemWebFarmTask).FullName)
                        .And()
                        .Where(w3 => w3
                            .WhereContains("TaskTextData", $"{LicenseKeyInfo.OBJECT_TYPE}|all")
                            .Or()
                            .WhereContains("TaskTextData", $"{SiteInfo.OBJECT_TYPE}|all")
                            .Or()
                            .WhereContains("TaskTextData", $"{WebFarmServerLogInfo.OBJECT_TYPE}|all")
                )));
        }


        /// <summary>
        /// Processes tasks that are provided by the specified function.
        /// </summary>
        /// <param name="tasks">Data set with web farm tasks to be processed.</param>
        /// <param name="deleteTasks">Action used for deleting processed tasks.</param>
        internal bool ProcessTasks(DataSet tasks, Action<List<int>> deleteTasks)
        {
            if (DataHelper.DataSourceIsEmpty(tasks))
            {
                return false;
            }

            var processedTaskIds = new List<int>();
            foreach (DataRow task in tasks.Tables[0].Rows)
            {
                int taskId = ValidationHelper.GetInteger(task["TaskID"], 0);
                string type = ValidationHelper.GetString(task["TaskType"], "");
                string target = ValidationHelper.GetString(task["TaskTarget"], "");
                string data = ValidationHelper.GetString(task["TaskTextData"], "");
                bool hasData = ValidationHelper.GetBoolean(task["TaskHasBinaryData"], false);

                try
                {
                    ParseTaskTextData(target, data).ToList().ForEach(d => ProcessTask(type, d.Item1, d.Item2, taskId, hasData));

                    // Add task to list to be deleted
                    processedTaskIds.Add(taskId);
                }
                catch (Exception e)
                {
                    ReportTaskError(taskId, WebFarmContext.ServerId, e);

                    var loggingPolicy = new LoggingPolicy(TimeSpan.FromMinutes(30));
                    EventLogProvider.LogException("WebFarmTaskProcessor", "ProcessTasks", e, loggingPolicy: loggingPolicy);
                }
            }

            deleteTasks(processedTaskIds);
            return true;
        }


        /// <summary>
        /// Processes the specified task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="hasData">Whether task has binary data in database.</param>
        private void ProcessTask(string taskType, string target, string data, int taskId, bool hasData)
        {
            // Disable logging of tasks while processing
            using (new CMSActionContext { LogWebFarmTasks = false })
            {
                // Log the operation
                DataRow ldr = WebFarmDebug.LogWebFarmOperation("DO: " + taskType, data, null, target);

                var binaryData = new BinaryData(null as byte[]);
                if (hasData)
                {
                    binaryData = new BinaryData(() => GetTaskBinaryData(taskId));
                }

                // Execute through registered task
                bool succeeded = WebFarmTaskManager.ExecuteTask(taskType, target, data, binaryData);

                // Add binary data info to the debug log
                if (ldr == null)
                {
                    return;
                }
                if (!succeeded)
                {
                    ldr["TaskType"] = "PROCESSING FAILED: " + taskType;
                }
                if (hasData)
                {
                    ldr["BinaryData"] = binaryData.IsLoaded ? DataHelper.GetSizeString(binaryData.Length) : "NOT USED";
                }
            }
        }


        /// <summary>
        /// Returns task binary data.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        private byte[] GetTaskBinaryData(int taskId)
        {
            return WebFarmTaskInfoProvider.GetWebFarmTasksInternal()
                .Column("TaskBinaryData")
                .WhereEquals("TaskID", taskId)
                .WhereNotNull("TaskBinaryData")
                .GetScalarResult<byte[]>();
        }


        /// <summary>
        /// Reports given error in task obtained by given ID and given server ID.
        /// </summary>
        private static void ReportTaskError(int taskId, int serverId, Exception e)
        {
            var isAnonymousTask = WebFarmContext.ServerId == 0;
            if (isAnonymousTask)
            {
                WebFarmTaskInfoProvider.SetErrorInTask(taskId, e.Message);
                return;
            }

            WebFarmServerTaskInfoProvider.SetErrorInTask(taskId, serverId, e.Message);
        }


        /// <summary>
        /// Logs the given exception based on some internal logic.
        /// </summary>
        /// <param name="ex">Exception.</param>
        private void LogError(Exception ex)
        {
            if (ex is LicenseException)
            {
                EventLogProvider.LogException("WebFarmTaskProcessor", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, ex, loggingPolicy: webFarmloggingPolicy.Value);
            }
            else
            {
                EventLogProvider.LogException("WebFarmTaskProcessor", ex.GetType() + "_" + ex.Source, ex, loggingPolicy: webFarmloggingPolicy.Value);
            }
        }


        /// <summary>
        /// Finishes the worker process. Implement this method to specify what the worker must do in order to not lose its internal data when being finished. Leave empty if no extra action is required.
        /// </summary>
        protected override void Finish()
        {
        }


        /// <summary>
        /// Parse task text data to deserialize grouped tasks
        /// </summary>
        /// <param name="target">Target to parse for separated tasks</param>
        /// <param name="data">Data to parse for separated tasks</param>
        internal IEnumerable<Tuple<string, string>> ParseTaskTextData(string target, string data)
        {
            string[] parsedTarget = new string[] { };
            string[] parsedData = new string[] { };

            if (target != null)
            {
                parsedTarget = target.Split(new[] { WebFarmTaskInfo.MULTIPLE_TASK_DATA_SEPARATOR }, StringSplitOptions.None);
            }

            if (data != null)
            {
                parsedData = data.Split(new[] { WebFarmTaskInfo.MULTIPLE_TASK_DATA_SEPARATOR }, StringSplitOptions.None);
            }

            if (parsedTarget.Count() != parsedData.Count())
            {
                // Number of targets has to be the same as number of data
                throw new ArgumentException("[WebFarmTaskProcessor.ParseTaskTextData]: Number of targets is not equal to number of text data.");
            }

            return parsedTarget.Select((t, i) => new Tuple<string, string>(t, parsedData[i]));
        }

        #endregion
    }
}
