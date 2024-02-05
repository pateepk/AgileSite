using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Scheduler
{
    using TypedDataSet = InfoDataSet<TaskInfo>;

    /// <summary>
    /// Class providing TaskInfo management.
    /// </summary>
    public class TaskInfoProvider : AbstractInfoProvider<TaskInfo, TaskInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// Maximum number of tasks fetched for each call of <see cref="FetchTasksToRunInternal"/>.
        /// </summary>
        private const int TASKS_FETCH_BATCH_SIZE = 50;


        /// <summary>
        /// No time selected.
        /// </summary>
        private static DateTime mNoTime = DateTimeHelper.ZERO_TIME;

        #endregion


        #region "Properties"

        /// <summary>
        /// No time selected.
        /// </summary>
        public static DateTime NO_TIME
        {
            get
            {
                return mNoTime;
            }
            set
            {
                mNoTime = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static TaskInfo GetTaskInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Returns the TaskInfo structure for the specified task.
        /// </summary>
        /// <param name="taskId">Task id</param>
        public static TaskInfo GetTaskInfo(int taskId)
        {
            return ProviderObject.GetInfoById(taskId);
        }


        /// <summary>
        /// Reset executions count of all tasks
        /// </summary>
        /// <param name="siteID">SiteID of tasks. 0 for global tasks</param>
        public static void ResetAllTasks(int siteID)
        {
            ProviderObject.ResetAllTasksInternal(siteID);
        }


        /// <summary>
        /// Returns the TaskInfo structure for the specified task.
        /// </summary>
        /// <param name="taskName">Task name</param>
        /// <param name="siteName">Site name</param>
        public static TaskInfo GetTaskInfo(string taskName, string siteName)
        {
            return ProviderObject.GetTaskInfoInternal(taskName, siteName);
        }


        /// <summary>
        /// Returns the TaskInfo structure for the specified task.
        /// </summary>
        /// <param name="taskName">Task name</param>
        /// <param name="siteId">Site ID</param>
        public static TaskInfo GetTaskInfo(string taskName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(taskName, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified task.
        /// </summary>
        /// <param name="task">Task to set</param>
        public static void SetTaskInfo(TaskInfo task)
        {
            ProviderObject.SetInfo(task);
        }


        /// <summary>
        /// Creates the tasks for all the web farm servers.
        /// </summary>
        /// <param name="task">Task data</param>
        public static void CreateWebFarmTasks(TaskInfo task)
        {
            ProviderObject.CreateWebFarmTasksInternal(task);
        }


        /// <summary>
        /// Gets the query for all tasks
        /// </summary>
        public static ObjectQuery<TaskInfo> GetTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns DataSet with tasks which are running or are new and have next run time not set.
        /// </summary>
        public static ObjectQuery<TaskInfo> GetTaskToReInit()
        {
            return ProviderObject.GetTaskToReInitInternal();
        }


        /// <summary>
        /// Deletes specified task.
        /// </summary>
        /// <param name="taskObj">Task object</param>
        public static void DeleteTaskInfo(TaskInfo taskObj)
        {
            ProviderObject.DeleteInfo(taskObj);
        }


        /// <summary>
        /// Deletes specified task.
        /// </summary>
        /// <param name="taskId">Task id</param>
        public static void DeleteTaskInfo(int taskId)
        {
            TaskInfo taskInfo = GetTaskInfo(taskId);
            DeleteTaskInfo(taskInfo);
        }


        /// <summary>
        /// Removes scheduled tasks associated to selected objects of given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">List of IDs</param>
        public static void DeleteObjectsTasks(string objectType, IList<int> ids)
        {
            var where = new WhereCondition()
                .WhereEquals("TaskObjectType", objectType)
                .WhereIn("TaskObjectID", ids);

            DeleteObjectsTasks(where);
        }


        /// <summary>
        /// Removes scheduled tasks. Other than objects' tasks might require individual handling.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteObjectsTasks(IWhereCondition where)
        {
            // Only allow to delete object tasks, not normal tasks
            ProviderObject.BulkDelete(where, new BulkDeleteSettings { ObjectType = TaskInfo.OBJECT_TYPE_OBJECTTASK });
        }


        /// <summary>
        /// Gets scheduled tasks to run, the process is expected to set the next run time for next task iteration.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="serverName">Server name</param>
        /// <param name="lastProcessedTaskId">Identifier of last processed task. IDs of all fetched tasks will be higher than this one.</param>
        /// <param name="useExternalService">Indicates whether get tasks for external service. If <c>null</c>, both tasks for external service and all other tasks are returned.</param>
        /// <returns>Dataset with information about scheduled tasks</returns>
        /// <remarks>Only certain small number of tasks is fetched by the method.</remarks>
        public static TypedDataSet FetchTasksToRun(string siteName, string serverName, int lastProcessedTaskId = 0, bool? useExternalService = null)
        {
            return ProviderObject.FetchTasksToRunInternal(siteName, serverName, lastProcessedTaskId, useExternalService);
        }

        /// <summary>
        /// Updates all specified tasks.
        /// </summary>
        /// <param name="updateExpression">Data to be update</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="where">WHERE condition</param>
        public static void UpdateAllTasks(string updateExpression, QueryDataParameters parameters, string where)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the TaskInfo structure for the specified task.
        /// </summary>
        /// <param name="taskName">Task name</param>
        /// <param name="siteName">Site name</param>
        protected virtual TaskInfo GetTaskInfoInternal(string taskName, string siteName)
        {
            int siteId = 0;
            // Get the site info
            if (siteName != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si == null)
                {
                    throw new Exception("[TaskInfoProvider.GetTaskInfo]: Site name '" + siteName + "' not found.");
                }
                siteId = si.SiteID;
            }

            return GetTaskInfo(taskName, siteId);
        }


        /// <summary>
        /// Reset executions count of all non-system tasks
        /// </summary>
        /// <param name="siteID">SiteID of tasks. 0 for global tasks</param>
        protected virtual void ResetAllTasksInternal(int siteID)
        {
            // Add now parameter
            QueryDataParameters qp = new QueryDataParameters();
            qp.Add("@Now", DateTime.Now);

            // Create SiteID where
            string where = String.Format("(TaskType IS NULL OR TaskType != {0}) AND {1}", (int)ScheduledTaskTypeEnum.System, ((siteID == 0) ? "TaskSiteID IS NULL" : "TaskSiteID = " + siteID));

            // Execute query
            ConnectionHelper.ExecuteQuery("CMS.ScheduledTask.ResetAllTasks", qp, where, String.Empty);
        }


        /// <summary>
        /// Creates the tasks for all the web farm servers.
        /// </summary>
        /// <param name="task">Task data</param>
        protected virtual void CreateWebFarmTasksInternal(TaskInfo task)
        {
            int counter = 1;
            string taskName = String.Empty;

            // Create tasks for all the configured servers
            var servers = CoreServices.WebFarm.GetEnabledServerNames();

            foreach (string serverName in servers)
            {
                task.TaskID = 0;
                task.TaskServerName = serverName;

                // Indicates whether task name is unique
                bool isUnique = false;

                // get unique task name
                while (!isUnique)
                {
                    TaskInfo existing = GetTaskInfo(task.TaskName, task.TaskSiteID);
                    if (existing == null)
                    {
                        isUnique = true;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(taskName))
                        {
                            taskName = TextHelper.LimitLength(task.TaskName, 195, String.Empty);
                        }
                        task.TaskName = taskName + "_" + counter;
                        counter++;
                    }
                }

                // Create the task
                SetTaskInfo(task);
            }
        }



        /// <summary>
        /// Returns DataSet with tasks which are running or are new and have next run time not set.
        /// </summary>
        protected virtual ObjectQuery<TaskInfo> GetTaskToReInitInternal()
        {
            return ProviderObject
                .GetObjectQuery()
                .Where(new WhereCondition()
                    .WhereTrue("TaskIsRunning")
                    .Or()
                    .Where(new WhereCondition()
                        .WhereNull("TaskNextRunTime")
                        .WhereEquals("TaskEnabled", 1)
                        .WhereEqualsOrNull("TaskExecutions", 0))
                );
        }


        /// <summary>
        /// Gets scheduled tasks to run, the process is expected to set the next run time for next task iteration.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="serverName">Server name</param>
        /// <param name="lastProcessedTaskID">Identifier of last processed task. IDs of all fetched tasks will be higher than this one.</param>
        /// <param name="useExternalService">Indicates if tasks is processed by service. If null, all tasks are fetched.</param>
        /// <returns>Dataset with information about scheduled tasks</returns>
        protected virtual TypedDataSet FetchTasksToRunInternal(string siteName, string serverName, int lastProcessedTaskID, bool? useExternalService)
        {
            // Get the site
            SiteInfo siteInfo = null;
            if (!string.IsNullOrEmpty(siteName))
            {
                siteInfo = SiteInfoProvider.GetSiteInfo(siteName);
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@TaskSiteID", siteInfo == null ? (int?)null : siteInfo.SiteID);
            parameters.Add("@DateTime", DateTime.Now);
            parameters.Add("@UseExternalService", useExternalService);
            parameters.Add("@TaskServerName", serverName ?? string.Empty);
            parameters.Add("@BatchSize", TASKS_FETCH_BATCH_SIZE);
            parameters.Add("@LastProcessedId", lastProcessedTaskID);
            parameters.EnsureDataSet<TaskInfo>();

            // Get the tasks
            return ConnectionHelper.ExecuteQuery("CMS.ScheduledTask.FetchTasksToRun", parameters).As<TaskInfo>();
        }

        #endregion
    }
}