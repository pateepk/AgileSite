using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class providing WebFarmTaskInfo management.
    /// </summary>
    public class WebFarmTaskInfoProvider : AbstractInfoProvider<WebFarmTaskInfo, WebFarmTaskInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the query for all web farm server tasks.
        /// </summary>   
        public static ObjectQuery<WebFarmTaskInfo> GetWebFarmTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the query for all web farm server tasks.
        /// </summary>   
        internal static ObjectQuery<WebFarmTaskInfo> GetWebFarmTasksInternal()
        {
            return ProviderObject.GetObjectQuery(false);
        }


        /// <summary>
        /// Gets the task by given GUID.
        /// </summary>
        /// <param name="taskGuid">Task GUID</param>
        public static WebFarmTaskInfo GetWebFarmTaskInfo(Guid taskGuid)
        {
            return ProviderObject.GetInfoByGuid(taskGuid);
        }


        /// <summary>
        /// Returns the WebFarmTaskInfo structure for the specified webFarmTask.
        /// </summary>
        /// <param name="webFarmTaskId">WebFarmTask id</param>
        public static WebFarmTaskInfo GetWebFarmTaskInfo(int webFarmTaskId)
        {
            return ProviderObject.GetInfoById(webFarmTaskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified webFarmTask.
        /// </summary>
        /// <param name="webFarmTask">WebFarmTask to set</param>
        public static int SetWebFarmTaskInfo(WebFarmTaskInfo webFarmTask)
        {
            return ProviderObject.SetWebFarmTaskInfoInternal(webFarmTask);
        }


        /// <summary>
        /// Deletes specified webFarmTask.
        /// </summary>
        /// <param name="webFarmTaskObj">WebFarmTask object</param>
        public static void DeleteWebFarmTaskInfo(WebFarmTaskInfo webFarmTaskObj)
        {
            ProviderObject.DeleteInfo(webFarmTaskObj);
        }


        /// <summary>
        /// Deletes specified webFarmTask.
        /// </summary>
        /// <param name="webFarmTaskId">WebFarmTask id</param>
        public static void DeleteWebFarmTaskInfo(int webFarmTaskId)
        {
            WebFarmTaskInfo webFarmTaskObj = GetWebFarmTaskInfo(webFarmTaskId);
            if (webFarmTaskObj != null)
            {
                DeleteWebFarmTaskInfo(webFarmTaskObj);
            }
        }


        /// <summary>
        /// Deletes all task for specified server. Deletes all tasks if the server is not specified.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        public static void DeleteServerTasks(int serverId = 0)
        {
            ProviderObject.DeleteServerTasksInternal(serverId);
        }


        /// <summary>
        /// Deletes all memory task for specified server created before given date. 
        /// If server is not specified (equals zero), deletes tasks of all servers.
        /// If date is not specified, deletes tasks before current date time.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="taskCreated">Tasks created before this date time will be deleted. If <c>null</c>, current date time is used.</param>
        public static void DeleteServerMemoryTasks(int serverId = 0, DateTime? taskCreated = null)
        {
            ProviderObject.DeleteServerMemoryTasksInternal(serverId, taskCreated ?? DateTime.Now);
        }


        /// <summary>
        /// Deletes anonymous tasks.
        /// </summary>
        public static void DeleteAnonymousTasks(List<int> taskIds = null)
        {
            ProviderObject.DeleteAnonymousTasksInternal(taskIds);
        }


        /// <summary>
        /// Deletes the binding between task and the server.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="taskId">Task ID.</param>
        public static void DeleteServerTask(int serverId, int taskId)
        {
            ProviderObject.DeleteServerTaskInternal(serverId, taskId);
        }


        /// <summary>
        /// Deletes the bindings between given tasks and the server.
        /// </summary>
        /// <param name="serverId">ID of server</param>
        /// <param name="taskIds">ID of task</param>
        public static void DeleteServerTasks(int serverId, List<int> taskIds)
        {
            DeleteServerTasksInternal(serverId, taskIds);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified webFarmTask.
        /// </summary>
        /// <param name="webFarmTask">WebFarmTask to set</param>
        protected virtual int SetWebFarmTaskInfoInternal(WebFarmTaskInfo webFarmTask)
        {
            SetInfo(webFarmTask);

            return webFarmTask.TaskID;
        }


        /// <summary>
        /// Deletes all memory task for specified server created before given date. 
        /// If <paramref name="serverId"/> equals zero, deletes tasks of all servers.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="taskCreated">Tasks created before this date time will be deleted.</param>
        protected virtual void DeleteServerMemoryTasksInternal(int serverId, DateTime taskCreated)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ServerID", serverId);
            parameters.Add("@TaskCreated", taskCreated);

            // Delete server task bindings
            ConnectionHelper.ExecuteQuery("cms.WebFarmTask.DeleteMemoryTasks", parameters);

            // Delete old orphaned tasks
            DeleteOrphanedTasks();
        }


        /// <summary>
        /// Deletes all task for specified server.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        protected virtual void DeleteServerTasksInternal(int serverId)
        {
            // Delete server task bindings
            var query = WebFarmServerTaskInfoProvider.DeleteQuery();

            if (serverId > 0)
            {
                query = query.WhereEquals("ServerID", serverId);
            }

            query.Execute();

            // Delete old orphaned tasks
            DeleteOrphanedTasks();
        }


        /// <summary>
        /// Deletes the bindings between given tasks and the server.
        /// </summary>
        /// <param name="serverId">ID of server</param>
        /// <param name="taskIds">IDs of tasks</param>
        private static void DeleteServerTasksInternal(int serverId, List<int> taskIds)
        {
            if (!taskIds.Any())
            {
                return;
            }

            var taskIDsIntTable = SqlHelper.BuildOrderedIntTable(taskIds);

            var deleteQuery = new DataQuery
            {
                CustomQueryText = "DELETE CMS_WebFarmServerTask FROM ##SOURCE## WHERE ##WHERE##"
            };

            deleteQuery.From(
                new QuerySource("CMS_WebFarmServerTask")
                    .Join("@TaskIDs", "TaskID", "Value")
            )
            .WhereEquals("ServerID", serverId);

            deleteQuery.Parameters.Add("@TaskIDs", taskIDsIntTable, SqlHelper.OrderedIntegerTableType);

            deleteQuery.Execute();
        }


        /// <summary>
        /// Deletes anonymous tasks.
        /// </summary>
        protected virtual void DeleteAnonymousTasksInternal(List<int> taskIds = null)
        {
            // Delete anonymous tasks
            var where = new WhereCondition().WhereEquals("TaskIsAnonymous", 1);
            if (taskIds != null)
            {
                where.WhereIn("TaskID", taskIds);
            }

            BulkDelete(where);
        }


        /// <summary>
        /// Deletes the binding between task and the server.
        /// </summary>
        /// <param name="serverId">ID of server</param>
        /// <param name="taskId">ID of task</param>
        protected virtual void DeleteServerTaskInternal(int serverId, int taskId)
        {
            // Delete server task bindings
            WebFarmServerTaskInfoProvider.DeleteQuery().Where(new WhereCondition().WhereEquals("ServerID", serverId).And().WhereEquals("TaskID", taskId)).Execute();

            // Delete old orphaned tasks
            DeleteOrphanedTasks();
        }


        /// <summary>
        /// Deletes old orphaned tasks.
        /// </summary>
        /// <remarks>Orphaned task is task that does NOT have any server task bindings and is NOT anonymous.</remarks>
        internal static void DeleteOrphanedTasks()
        {
            ConnectionHelper.ExecuteQuery("cms.WebFarmTask.DeleteOrphanedTasks", null);
        }


        /// <summary>
        /// Sets given error message to task with given ID on server with given ID.
        /// </summary>
        /// <param name="taskId">Task Id to be set</param>
        /// <param name="errorMessage">Error message to be set</param>
        internal static void SetErrorInTask(int taskId, string errorMessage)
        {
            var condition = new WhereCondition().WhereEquals("TaskID", taskId);

            ProviderObject.UpdateData(condition, new Dictionary<string, object> { { "TaskErrorMessage", errorMessage } });
        }

        #endregion
    }
}