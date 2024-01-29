using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing TaskInfo management.
    /// </summary>
    public class StagingTaskInfoProvider : AbstractInfoProvider<StagingTaskInfo, StagingTaskInfoProvider>
    {
        /// <summary>
        /// Context name for AsyncControl log filtering to synchronization API calls only.
        /// </summary>
        public const string LOGCONTEXT_SYNCHRONIZATION = "Synchronization";


        #region "Variables"

        /// <summary>
        /// Object tree.
        /// </summary>
        private static ObjectTypeTreeNode mObjectTree;


        /// <summary>
        /// Indicates if older tasks should be kept.
        /// </summary>
        public static BoolAppSetting KeepOlderTasks = new BoolAppSetting("CMSStagingKeepOlderTasks");


        /// <summary>
        /// If true, tasks for global objects are logged only for the sites assigned to the object.
        /// </summary>
        public static BoolAppSetting LogGlobalObjectsOnlyForAssignedSites = new BoolAppSetting("CMSStagingLogGlobalObjectsOnlyForAssignedSites");


        /// <summary>
        /// Indicates whether staging is enabled globally based on AppSettings value. It is enabled by default.
        /// </summary>
        public static BoolAppSetting StagingEnabled = new BoolAppSetting("CMSStagingEnabled", true);

        /// <summary>
        /// Current server name.
        /// </summary>
        public static StringAppSetting ServerName = new StringAppSetting("CMSStagingServerName", "");


        private static readonly object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Object tree.
        /// </summary>
        public static ObjectTypeTreeNode ObjectTree
        {
            get
            {
                if (mObjectTree == null)
                {
                    lock (lockObject)
                    {
                        if (mObjectTree == null)
                        {
                            var tree = ObjectTypeTreeNode.NewObjectTree("");

                            // Remove global settings group
                            tree.RemoveNode("cms.settingskey", false);

                            // Register object types
                            ObjectTypeManager.RegisterTypesToObjectTree(tree, info => (!info.IsListingObjectTypeInfo && !info.IsVirtualObject) ? info.SynchronizationSettings.ObjectTreeLocations : null);

                            mObjectTree = tree;
                        }
                    }
                }

                return mObjectTree;
            }
        }


        /// <summary>
        /// Task server list.
        /// </summary>
        public static string TaskServerList
        {
            get
            {
                return ValidationHelper.GetString(RequestStockHelper.GetItem("StagingTaskServerList"), string.Empty);
            }
            set
            {
                RequestStockHelper.Add("StagingTaskServerList", value);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Indicates if logging object changes staging tasks is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LogObjectChanges(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSStagingLogObjectChanges") && StagingEnabled;
        }


        /// <summary>
        /// Indicates if logging data changes is enabled.
        /// </summary>
        /// <returns>TRUE if logging data changes is enabled.</returns>
        public static bool LogDataChanges()
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSStagingLogDataChanges") && StagingEnabled;
        }


        /// <summary>
        /// Indicates if logging staging tasks for content is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LogContentChanges(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSStagingLogChanges") && StagingEnabled;
        }


        /// <summary>
        /// Returns true if settings for logging staging task are enabled for given site.
        /// If site is String.Empty, then value is returned for global keys.
        /// </summary>
        public static bool LoggingOfStagingTasksEnabled(string siteName)
        {
            return (LogDataChanges() || LogObjectChanges(siteName) || LogContentChanges(siteName));
        }


        /// <summary>
        /// Returns the TaskInfo structure for the specified task.
        /// </summary>
        /// <param name="taskId">Task id</param>
        public static StagingTaskInfo GetTaskInfo(int taskId)
        {
            return ProviderObject.GetInfoById(taskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified task.
        /// </summary>
        /// <param name="task">Task to set</param>
        public static void SetTaskInfo(StagingTaskInfo task)
        {
            ProviderObject.SetInfo(task);
        }


        /// <summary>
        /// Deletes specified task.
        /// </summary>
        /// <param name="taskObj">Task object</param>
        public static void DeleteTaskInfo(StagingTaskInfo taskObj)
        {
            ProviderObject.DeleteInfo(taskObj);
        }


        /// <summary>
        /// Deletes synchronization tasks without any binding to a server (i.e. there is no record in Staging_Synchronization table for such task).
        /// </summary>
        public static void DeleteRedundantTasks()
        {
            ProviderObject.DeleteRedundantTasksInternal();
        }


        /// <summary>
        /// Deletes specified task.
        /// </summary>
        /// <param name="taskId">Task id</param>
        public static void DeleteTaskInfo(int taskId)
        {
            StagingTaskInfo taskObj = GetTaskInfo(taskId);
            DeleteTaskInfo(taskObj);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        public static ObjectQuery<StagingTaskInfo> SelectTaskList(int siteId, int serverId, string where, string orderBy)
        {
            return SelectTaskList(siteId, serverId, where, orderBy, -1, null);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static ObjectQuery<StagingTaskInfo> SelectTaskList(int siteId, int serverId, string where, string orderBy, int topN, string columns)
        {
            int totalRecords = 0;
            return SelectTaskList(siteId, serverId, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Selects all the tasks with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static ObjectQuery<StagingTaskInfo> SelectTaskList(int siteId, int serverId, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            return ProviderObject.SelectTaskListInternal(siteId, serverId, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="aliasPath">Tasks alias path</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        public static ObjectQuery<StagingTaskInfo> SelectDocumentTaskList(int siteId, int serverId, string aliasPath, string where, string orderBy)
        {
            return SelectDocumentTaskList(siteId, serverId, aliasPath, where, orderBy, -1, null);
        }


        /// <summary>
        /// Deletes old orphaned tasks.
        /// </summary>
        /// <remarks>Orphaned task is task that does NOT have any <see cref="SynchronizationInfo"/> bindings.</remarks>
        /// <param name="taskIds">IDs from which the orphaned tasks should be removed.</param>
        public static void DeleteOrphanedTasks(ICollection<int> taskIds)
        {
            ProviderObject.DeleteOrphanedTasksInternal(taskIds);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="aliasPath">Tasks alias path</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static ObjectQuery<StagingTaskInfo> SelectDocumentTaskList(int siteId, int serverId, string aliasPath, string where, string orderBy, int topN, string columns)
        {
            int totalRecords = 0;
            return SelectDocumentTaskList(siteId, serverId, aliasPath, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="aliasPath">Tasks alias path</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static ObjectQuery<StagingTaskInfo> SelectDocumentTaskList(int siteId, int serverId, string aliasPath, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            // Get where condition which returns NodeIDs for subtree because TaskNodeAliasPath may be different than actual NodeAliasPath for same documents
            WhereCondition whereCondition = new WhereCondition()
                                                    // Apply where condition for deleted documents too, for example by filter
                                                    .Where(where)
                                                    .And()
                                                    .Where(
                                                    new WhereCondition().WhereIn("TaskNodeID", new IDQuery("cms.document", "NodeID")
                                                                                        .WhereEquals("NodeAliasPath", aliasPath)
                                                                                        .Or()
                                                                                        .WhereStartsWith("NodeAliasPath", aliasPath.TrimEnd('/') + "/"))
                                                    .Or()
                                                    // Include delete tasks for deleted documents as well
                                                    .WhereStartsWith("TaskNodeAliasPath", aliasPath.TrimEnd('/') + "/"));


            return SelectTaskList(siteId, serverId, whereCondition.ToString(true), orderBy, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        public static ObjectQuery<StagingTaskInfo> SelectObjectTaskList(int siteId, int serverId, string objectTypes, string where, string orderBy)
        {
            return SelectObjectTaskList(siteId, serverId, objectTypes, where, orderBy, -1, null);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static ObjectQuery<StagingTaskInfo> SelectObjectTaskList(int siteId, int serverId, string objectTypes, string where, string orderBy, int topN, string columns)
        {
            int totalRecords = 0;
            return SelectObjectTaskList(siteId, serverId, objectTypes, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Selects all the tasks within subtree of given alias path, with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static ObjectQuery<StagingTaskInfo> SelectObjectTaskList(int siteId, int serverId, string objectTypes, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            // Where condition to select tasks for specified type
            string typeWhere = "TaskDocumentID IS NULL";
            if (!string.IsNullOrEmpty(objectTypes))
            {
                if (objectTypes.Contains(";"))
                {
                    string[] types = SqlHelper.GetSafeQueryString(objectTypes, false).Split(';');
                    typeWhere += " AND TaskObjectType IN ('" + String.Join("', '", types) + "')";
                }
                else
                {
                    typeWhere += " AND TaskObjectType = N'" + SqlHelper.GetSafeQueryString(objectTypes, false) + "'";
                }
            }

            where = SqlHelper.AddWhereCondition(where, typeWhere);

            return SelectTaskList(siteId, serverId, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Returns query with all tasks filtered by where condition.
        /// </summary>
        /// <param name="whereCondition">Where condition statement</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="getAllData">If false, only limited set of columns is selected</param>
        /// <param name="topN">Top N tasks</param>
        /// <param name="columns">Selected columns</param>
        public static ObjectQuery<StagingTaskInfo> GetTasks(string whereCondition, string orderBy, bool getAllData, int topN, string columns)
        {
            return ProviderObject.GetTasksInternal(whereCondition, orderBy, getAllData, topN, columns);
        }


        /// <summary>
        /// Get all staging tasks.
        /// </summary>
        /// <returns>Returns all staging tasks, does not depends on server, site or type.</returns>
        public static ObjectQuery<StagingTaskInfo> GetTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Deletes older synchronization tasks.
        /// </summary>
        /// <param name="taskObj">Current task object</param>
        public static void DeleteOlderTasks(StagingTaskInfo taskObj)
        {
            ProviderObject.DeleteOlderTasksInternal(taskObj);
        }


        /// <summary>
        /// Returns all the tasks that need to be synchronized (including older tasks that needs to be processed before the tasks with given IDs).
        /// </summary>
        /// <param name="taskIDs">Current task object</param>
        /// <param name="serverId">Server ID</param>
        public static InfoDataSet<StagingTaskInfo> GetTasksForSynchronization(IEnumerable<int> taskIDs, int serverId)
        {
            return ProviderObject.GetTasksForSynchronizationInternal(taskIDs, serverId);
        }


        /// <summary>
        /// Logs the synchronization task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="siteName">Object site name</param>
        /// <param name="siteId">Site ID of the servers to synchronize</param>
        /// <param name="serverId">Server ID to synchronize</param>
        /// <returns>Returns new synchronization task</returns>
        internal static StagingTaskInfo LogSynchronization(GeneralizedInfo infoObj, TaskTypeEnum taskType, string siteName, int siteId, int serverId)
        {
            return ProviderObject.LogSynchronizationInternal(infoObj, taskType, siteName, siteId, serverId);
        }


        /// <summary>
        /// Gets task title.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="th">Translation helper</param>
        public static string GetTaskTitle(TaskTypeEnum taskType, GeneralizedInfo infoObj, TranslationHelper th)
        {
            return ProviderObject.GetTaskTitleInternal(taskType, infoObj, th);
        }


        /// <summary>
        /// Gets the list of server IDs for which the staging task should be logged.
        /// </summary>
        /// <param name="task">Synchronization task.</param>
        /// <param name="siteId">Site ID.</param>
        /// <param name="serverId">Server ID.</param>
        public static IList<int> GetServerIdsToLogTaskTo(StagingTaskInfo task, int siteId, int serverId)
        {
            return ProviderObject.GetServerIdsToLogTaskToInternal(task, siteId, serverId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes old orphaned tasks.
        /// </summary>
        /// <remarks>Orphaned task is task that does NOT have any <see cref="SynchronizationInfo"/> bindings.</remarks>
        /// <param name="taskIds">IDs from which the orphaned tasks should be removed.</param>
        protected virtual void DeleteOrphanedTasksInternal(ICollection<int> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
            {
                return;
            }

            BulkDelete(
                new WhereCondition()
                    .WhereIn("TaskID", taskIds)
                    .WhereNotIn(
                        "TaskID",
                        SynchronizationInfoProvider.GetSynchronizations().Column("SynchronizationTaskID")
                    ),
                new BulkDeleteSettings { RemoveDependencies = true });
        }


        /// <summary>
        /// Deletes synchronization tasks without any binding to a server (i.e. there is no record in Staging_Synchronization table for such task).
        /// </summary>
        protected virtual void DeleteRedundantTasksInternal()
        {
            BulkDelete(
                new WhereCondition()
                    .WhereNotIn(
                        "TaskID",
                        SynchronizationInfoProvider.GetSynchronizations().Columns("SynchronizationTaskID")
                    ),
                new BulkDeleteSettings { RemoveDependencies = true }
            );
        }


        /// <summary>
        /// Selects all the tasks with information on the count of the failed attempts and without the task data.
        /// </summary>
        /// <param name="siteId">Tasks site ID</param>
        /// <param name="serverId">Tasks server ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        protected virtual ObjectQuery<StagingTaskInfo> SelectTaskListInternal(int siteId, int serverId, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            var selectTaskList = ProviderObject.GetObjectQuery()
                                        .Where(new WhereCondition()
                                               .WhereNull("TaskSiteID")
                                               .Or()
                                               .WhereEquals("TaskSiteID", siteId))
                                        .And()
                                        .WhereIn("TaskID", GetTasksFromSynchronizationTable(siteId, serverId));

            // Add another parameters
            selectTaskList.And().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
            selectTaskList.MaxRecords = maxRecords;
            selectTaskList.Offset = offset;

            if (!selectTaskList.Parameters.Any(dp => dp.Name == "ServerID"))
            {
                // This parameter is used only because of Unigrid in Tasks.aspx, 
                // and all pages with staging tasks where gridTasks.Columns is set
                selectTaskList.Parameters.Add("ServerID", serverId);
            }

            totalRecords = selectTaskList.TotalRecords;

            return selectTaskList;
        }


        private ObjectQuery<SynchronizationInfo> GetTasksFromSynchronizationTable(int siteId, int serverId)
        {
            var query = SynchronizationInfoProvider.GetSynchronizations().Column("SynchronizationTaskID");

            if (serverId > 0)
            {
                // When server ID is specified, return tasks for the given server
                query.WhereEquals("SynchronizationServerID", serverId);
            }
            else
            {
                if (siteId <= 0)
                {
                    // When site ID is not provided, return all synchronizations
                    // This condition is applied to the parent query
                }
                else if (siteId > 0)
                {
                    // When site ID is specified, return tasks for servers on the given site
                    query.WhereIn(
                        "SynchronizationServerID",
                        ServerInfoProvider.GetServers()
                                          .WhereEquals("ServerSiteID", siteId)
                                          .And()
                                          .WhereTrue("ServerEnabled")
                        );
                }
            }

            return query;
        }



        /// <summary>
        /// Returns dataset of all tasks filtered by where condition.
        /// </summary>
        /// <param name="whereCondition">Where condition statement</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="getAllData">If false, only limited set of columns is selected</param>
        /// <param name="topN">Top N tasks</param>
        /// <param name="columns">Selected columns</param>
        protected virtual ObjectQuery<StagingTaskInfo> GetTasksInternal(string whereCondition, string orderBy, bool getAllData, int topN, string columns)
        {
            // Get the data
            if (!getAllData && string.IsNullOrEmpty(columns))
            {
                // Default set of columns
                columns = "TaskID, TaskSiteID, TaskDocumentID, TaskNodeAliasPath, TaskTitle, TaskTime, TaskType, TaskNodeID";
            }

            return ProviderObject.GetObjectQuery().TopN(topN).Columns(columns).Where(whereCondition).OrderBy(orderBy);
        }


        /// <summary>
        /// Deletes older synchronization tasks of same object.
        /// </summary>
        /// <param name="taskObj">Current task object</param>
        protected virtual void DeleteOlderTasksInternal(StagingTaskInfo taskObj)
        {
            // Do not delete older tasks if should be kept
            if (!KeepOlderTasks)
            {
                // If taskObj is corrupted
                if (taskObj == null || ((taskObj.TaskDocumentID <= 0) && (taskObj.TaskObjectID <= 0)))
                {
                    return;
                }

                WhereCondition where = new WhereCondition();
                where.Where(new WhereCondition()
                                    .WhereNull("TaskRunning")
                                    .Or()
                                    .WhereNotEquals("TaskRunning", 1));

                bool getData = false;

                string createObj = TaskHelper.GetTaskTypeString(TaskTypeEnum.CreateObject);
                string updateObj = TaskHelper.GetTaskTypeString(TaskTypeEnum.UpdateObject);
                string createDoc = TaskHelper.GetTaskTypeString(TaskTypeEnum.CreateDocument);
                string updateDoc = TaskHelper.GetTaskTypeString(TaskTypeEnum.UpdateDocument);
                string publishDoc = TaskHelper.GetTaskTypeString(TaskTypeEnum.PublishDocument);
                string archiveDoc = TaskHelper.GetTaskTypeString(TaskTypeEnum.ArchiveDocument);
                string breakAcl = TaskHelper.GetTaskTypeString(TaskTypeEnum.BreakACLInheritance);
                string restoreAcl = TaskHelper.GetTaskTypeString(TaskTypeEnum.RestoreACLInheritance);
                string addToSite = TaskHelper.GetTaskTypeString(TaskTypeEnum.AddToSite);
                string removeFromSite = TaskHelper.GetTaskTypeString(TaskTypeEnum.RemoveFromSite);

                if (taskObj.TaskDocumentID > 0)
                {
                    // Document task
                    where.And()
                         .WhereEquals("TaskDocumentID", taskObj.TaskDocumentID)
                         .WhereEquals("TaskNodeID", taskObj.TaskNodeID)
                         .WhereNotEquals("TaskID", taskObj.TaskID);

                    switch (taskObj.TaskType)
                    {
                        // Document update - delete previous update tasks
                        case TaskTypeEnum.UpdateDocument:
                            getData = true;
                            where.And().WhereEquals("TaskType", updateDoc);
                            break;

                        // On delete, delete all, update tasks
                        case TaskTypeEnum.DeleteDocument:
                            getData = true;
                            where.And().WhereIn("TaskType", new List<string> { updateDoc, publishDoc, archiveDoc, breakAcl, restoreAcl });
                            break;

                        // Delete all cultures - delete all other tasks
                        case TaskTypeEnum.DeleteAllCultures:
                            getData = true;
                            break;
                    }
                }
                else if (taskObj.TaskObjectID > 0)
                {
                    // Object task - Delete only tasks on the same site as the new task
                    where.And()
                         .WhereEquals("TaskObjectID", taskObj.TaskObjectID)
                         .WhereEquals("TaskObjectType", taskObj.TaskObjectType)
                         .WhereNotEquals("TaskID", taskObj.TaskID);
                    switch (taskObj.TaskType)
                    {
                        // Object update - delete previous update tasks
                        case TaskTypeEnum.UpdateObject:
                            getData = true;
                            where.And().WhereEquals("TaskType", updateObj);
                            break;

                        // On delete, delete all, update and create tasks
                        case TaskTypeEnum.DeleteObject:
                            getData = true;
                            where.And().WhereIn("TaskType", new List<string> { updateObj, createObj, addToSite, removeFromSite });
                            break;

                        // Delete add to site on another add to site
                        case TaskTypeEnum.AddToSite:
                        case TaskTypeEnum.RemoveFromSite:
                            getData = true;
                            where.And().WhereIn("TaskType", new List<string> { addToSite, removeFromSite })
                                .And().WhereEquals("TaskSiteID", taskObj.TaskSiteID);
                            break;
                    }

                    // Get data
                    if (getData)
                    {
                        // For the class types, make sure that there was no operation before with objects influenced by the data structure
                        DataSet dependentDS = null;
                        switch (taskObj.TaskObjectType)
                        {
                            case PredefinedObjectType.DOCUMENTTYPE:
                                // Get any closest previous document task
                                dependentDS = GetTasks("TaskType IN (N'" + createDoc + "', N'" + updateDoc + "', N'" + publishDoc + "', N'" + archiveDoc + "')", "TaskID DESC", false, 1, "TaskID");
                                break;

                            case DataClassInfo.OBJECT_TYPE_SYSTEMTABLE:
                                // Get any closest previous object change (System table can be potentially any object except for classes)
                                dependentDS = GetTasks("TaskType IN (N'" + updateObj + "', N'" + createObj + "') AND TaskObjectType NOT IN (N'cms.systemtable', N'cms.class', N'cms.documenttype', N'cms.customtable')", "TaskID DESC", false, 1, "TaskID");
                                break;

                            case PredefinedObjectType.CUSTOMTABLECLASS:
                                // Get any closest previous custom table item task
                                dependentDS = GetTasks("TaskType IN (N'" + updateObj + "', N'" + createObj + "') AND TaskObjectType LIKE N'customtableitem.%'", "TaskID DESC", false, 1, "TaskID");
                                break;

                            case PredefinedObjectType.CUSTOMER:
                                // Get any closest previous order item task
                                dependentDS = GetTasks("TaskType IN (N'" + updateObj + "', N'" + createObj + "') AND TaskObjectType LIKE N'" + PredefinedObjectType.ORDER + "'", "TaskID DESC", false, 1, "TaskID");
                                break;
                        }

                        if (dependentDS != null)
                        {
                            // Make sure that only older tasks are deleted
                            int newerThanTaskId = ValidationHelper.GetInteger(DataHelper.GetScalarValue(dependentDS), 0);
                            if (newerThanTaskId > 0)
                            {
                                where.WhereGreaterThan("TaskID", newerThanTaskId);
                            }
                        }
                    }
                }

                // Get tasks
                if (getData)
                {
                    // getData tell us if there exists staging tasks, that are represented with where condition that will be deleted, 
                    // because of creation of new task, so all the old binding should be redirected to new task
                    using (var cts = BeginTransaction())
                    {
                        // Materialize query, get all tasks that will be deleted and their bindings should be recreated for a new task
                        var tasks = GetTasks(where.ToString(true), null, false, 0, "TaskID").TypedResult.ToArray();
                        var taskIDs = tasks.Select(sti => sti.TaskID).ToArray();

                        if (taskIDs.Count() > 0)
                        {
                            var oldTasksWhere = new WhereCondition().WhereIn("TaskID", taskIDs).Immutable();

                            AssociateOriginalUsersWithNewTask(taskObj, oldTasksWhere);
                            AssociateOriginalTaskGroupsWithNewTask(taskObj, oldTasksWhere);
                        }
                        DeleteOlderTasksInternal(taskObj, where);
                        cts.Commit();
                    }
                }
            }
        }


        /// <summary>
        /// Deletes older synchronization tasks of same object.
        /// </summary>
        /// <param name="currentTask">Current task object</param>
        /// <param name="whereCondition">Where condition selecting old tasks</param>
        protected virtual void DeleteOlderTasksInternal(StagingTaskInfo currentTask, WhereCondition whereCondition)
        {
            if (currentTask == null)
            {
                throw new ArgumentNullException("currentTask");
            }

            string where = (whereCondition == null) ? "" : whereCondition.ToString(true);
            var tasksToDelete = GetTasks(where, null, false, 0, null);
            tasksToDelete.Execute();

            // Delete selected tasks
            if (tasksToDelete.Count > 0)
            {
                // Get enabled servers for staging task, which will be deleted
                var serversCountQuery = ServerInfoProvider.GetServers();
                if (currentTask.TaskSiteID != 0)
                {
                    serversCountQuery = serversCountQuery.WhereEquals("ServerSiteID", currentTask.TaskSiteID);
                }

                // We have to recreate synchronizations tasks created by Staging Task for all servers,
                // which can be deleted when recreating update task for only one server
                if (serversCountQuery.Count > 1)
                {

                    // Get list of servers to which current task is synched
                    var syncedServers = SynchronizationInfoProvider.GetSynchronizations()
                                        .Column("SynchronizationServerID")
                                        .WhereEquals("SynchronizationTaskID", currentTask.TaskID);

                    // Server ids for which synchronization tasks must be recreated
                    var serversToBeRecreated = SynchronizationInfoProvider.GetSynchronizations()
                                                .WhereIn("SynchronizationTaskID", tasksToDelete.AsEnumerable().Select(t => t.TaskID).ToList())
                                                .WhereNotIn("SynchronizationServerID", syncedServers)
                                                .Distinct()
                                                .Select(sync => sync.SynchronizationServerID);

                    // Sync tasks should be recreated for the rest of the servers that belongs to old deleted task
                    SynchronizationInfoProvider.CreateSynchronizationRecords(currentTask.TaskID, serversToBeRecreated);
                }

                foreach (var task in tasksToDelete)
                {
                    DeleteTaskInfo(task);
                }
            }
        }


        /// <summary>
        /// Returns all the tasks that need to be synchronized (including older tasks that needs to be processed before the tasks with given IDs).
        /// </summary>
        /// <param name="taskIDs">Current task object</param>
        /// <param name="serverId">Server ID</param>
        protected virtual InfoDataSet<StagingTaskInfo> GetTasksForSynchronizationInternal(IEnumerable<int> taskIDs, int serverId)
        {
            if (taskIDs == null)
            {
                return new InfoDataSet<StagingTaskInfo>();
            }

            var taskIDsList = taskIDs.ToList();
            if (taskIDsList.Count == 0)
            {
                return new InfoDataSet<StagingTaskInfo>();
            }

            var taskIDsIntTable = SqlHelper.BuildOrderedIntTable(taskIDsList);

            var parameters = new QueryDataParameters
            {
                {"@TaskIDs", taskIDsIntTable, SqlHelper.OrderedIntegerTableType},
                {"@ServerId", serverId}
            };
            var data = ConnectionHelper.ExecuteQuery("Staging.Task.SelectTasksForSynchronization", parameters);
            return new InfoDataSet<StagingTaskInfo>(data);
        }


        /// <summary>
        /// Logs the synchronization task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="siteName">Object site name</param>
        /// <param name="siteId">Site ID of the servers to synchronize</param>
        /// <param name="serverId">Server ID to synchronize</param>
        /// <returns>Returns new synchronization task</returns>
        protected virtual StagingTaskInfo LogSynchronizationInternal(GeneralizedInfo infoObj, TaskTypeEnum taskType, string siteName, int siteId, int serverId)
        {
            // Check object instance
            if (infoObj == null)
            {
                throw new Exception("[TaskInfoProvider.LogSynchronization]: Missing object instance.");
            }

            // Log only if synchronization enabled
            if ((serverId != SynchronizationInfoProvider.ENABLED_SERVERS) || SynchronizationHelper.CheckStagingLogging(infoObj))
            {
                // Excluded tasks combinations
                var ti = infoObj.TypeInfo;

                switch (taskType)
                {
                    case TaskTypeEnum.DeleteObject:
                        // Do not log site delete - Not supported
                        if (ti.ObjectType == PredefinedObjectType.SITE)
                        {
                            return null;
                        }
                        break;
                }

                try
                {
                    // Lock on the object instance to ensure only single running logging for the object
                    lock (infoObj.GetLockObject())
                    {
                        using (new CMSConnectionScope())
                        {
                            // Get task site ID
                            int taskSiteId = infoObj.ObjectSiteID;
                            // Add to site and Remove from site tasks are always site related
                            if ((taskType == TaskTypeEnum.AddToSite) || (taskType == TaskTypeEnum.RemoveFromSite))
                            {
                                taskSiteId = siteId;
                            }

                            // Get task type
                            if (ti.IsBinding)
                            {
                                // Change task type and object for site binding
                                if (SynchronizationHelper.ChangeSiteBindingObject(ref infoObj, ref siteId, ref taskType))
                                {
                                    // Log only for that specific site
                                    taskSiteId = siteId;
                                }
                            }
                            else
                            {
                                // ### Special case - Site ###
                                if (ti.ObjectType == PredefinedObjectType.SITE)
                                {
                                    // Log only for that specific site
                                    siteId = infoObj.ObjectID;
                                    taskSiteId = siteId;
                                }
                            }

                            List<int> siteIDs = null;

                            // Initialize settings
                            var settings = new SynchronizationObjectSettings();
                            settings.Operation = OperationTypeEnum.Synchronization;
                            settings.IncludeSiteBindings = false;
                            settings.IncludeOtherBindings = false;
                            settings.TranslationHelper = new TranslationHelper();

                            // Add additional data based on the task type
                            switch (taskType)
                            {
                                // Object update - full DataSet
                                case TaskTypeEnum.UpdateObject:
                                case TaskTypeEnum.CreateObject:
                                case TaskTypeEnum.AddToSite:
                                    {
                                        // For global objects which have site binding, synchronize only to assigned sites
                                        if ((siteId == 0) && LogGlobalObjectsOnlyForAssignedSites && (ti.SiteBinding != null))
                                        {
                                            GeneralizedInfo bindingObj = ModuleManager.GetReadOnlyObject(ti.SiteBinding);
                                            if (bindingObj != null)
                                            {
                                                // Get the assigned sites
                                                var bindingTypeInfo = bindingObj.TypeInfo;
                                                DataSet dsSites = bindingObj.GetData(null, bindingTypeInfo.ParentIDColumn + " = " + infoObj.ObjectID, null, 0, bindingTypeInfo.SiteIDColumn, false);
                                                siteIDs = new List<int>();
                                                if (!DataHelper.DataSourceIsEmpty(dsSites))
                                                {
                                                    // Build the list of site IDs
                                                    foreach (DataRow dr in dsSites.Tables[0].Rows)
                                                    {
                                                        siteIDs.Add((int)dr[bindingTypeInfo.SiteIDColumn]);
                                                    }
                                                }

                                                // Do not continue logging if no sites assigned
                                                if (siteIDs.Count == 0)
                                                {
                                                    return null;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                // Object deletion - only the main deleted object
                                case TaskTypeEnum.DeleteObject:
                                case TaskTypeEnum.RemoveFromSite:
                                    {
                                        settings.IncludeCategories = false;
                                        settings.IncludeChildren = false;
                                        settings.Binary = false;
                                    }
                                    break;

                                default:
                                    throw new Exception("[SynchronizationHelper.GetObjectDataSet]: Unknown task type '" + taskType + "'.");
                            }

                            // Narrow down only enabled servers
                            if (serverId == SynchronizationInfoProvider.ENABLED_SERVERS)
                            {
                                if (siteIDs == null)
                                {
                                    if (!ServerInfoProvider.IsEnabledServer(siteId))
                                    {
                                        // There is no enabled server to log task to
                                        return null;
                                    }
                                }
                                else
                                {
                                    siteIDs = ServerInfoProvider.FilterEnabledServers(siteIDs);

                                    if (!siteIDs.Any())
                                    {
                                        // There is no enabled server to log task to
                                        return null;
                                    }
                                }
                            }

                            // Get object XML
                            string xml = SynchronizationHelper.GetObjectXml(settings, infoObj, taskType);

                            // Prepare task title
                            string taskTitle = GetTaskTitle(taskType, infoObj, settings.TranslationHelper);

                            // Create synchronization task
                            var sti = new StagingTaskInfo();

                            sti.TaskData = xml;
                            sti.TaskObjectID = infoObj.ObjectID;
                            sti.TaskObjectType = ti.ObjectType;
                            sti.TaskTime = DateTime.Now;
                            sti.TaskSiteID = taskSiteId;
                            sti.TaskTitle = taskTitle;
                            sti.TaskType = taskType;

                            UpdateTaskServers(sti);

                            using (var h = StagingEvents.LogTask.StartEvent(sti, infoObj))
                            {
                                if (h.CanContinue())
                                {
                                    var serverIds = siteIDs != null ?
                                        GetServerIdsToLogTaskTo(sti, siteIDs, serverId == SynchronizationInfoProvider.ENABLED_SERVERS) :
                                        GetServerIdsToLogTaskTo(sti, siteId, serverId);

                                    if (serverIds.Count > 0)
                                    {
                                        // Log task preparation
                                        var message = string.Format(ResHelper.GetAPIString("synchronization.preparing", "Preparing '{0}' task"), HTMLHelper.HTMLEncode(taskTitle));
                                        LogContext.AppendLine(message, LOGCONTEXT_SYNCHRONIZATION);

                                        // Save within transaction to keep multi-threaded consistency in DB
                                        using (var tr = new CMSTransactionScope())
                                        {
                                            // Save the task
                                            SetTaskInfo(sti);

                                            SynchronizationInfoProvider.CreateSynchronizationRecords(sti.TaskID, serverIds);

                                            // Commit the transaction
                                            tr.Commit();
                                        }

                                        // Delete older tasks
                                        DeleteOlderTasks(sti);
                                    }
                                }

                                h.FinishEvent();
                            }

                            return sti;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    EventLogProvider.LogException("Staging", "LogObject", ex);
                }
            }

            return null;
        }


        /// <summary>
        /// Updates the task servers with the current server name
        /// </summary>
        /// <param name="ti">Task info</param>
        public static void UpdateTaskServers(StagingTaskInfo ti)
        {
            // Set current list of servers as already processed - it is crucial to concatenate those servers so bi-directional staging works well
            ti.TaskServers = TaskServerList;

            string taskServerName = ServerName;
            if (!String.IsNullOrEmpty(taskServerName))
            {
                // Update the server list
                if (!ti.TaskServers.ToLowerCSafe().Contains(";" + taskServerName.ToLowerCSafe() + ";"))
                {
                    ti.TaskServers = ti.TaskServers.TrimEnd(';') + ";" + taskServerName + ";";
                }
            }
        }


        /// <summary>
        /// Gets task title.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="th">Translation helper</param>
        protected virtual string GetTaskTitleInternal(TaskTypeEnum taskType, GeneralizedInfo infoObj, TranslationHelper th)
        {
            if (infoObj == null)
            {
                throw new Exception("[TaskInfoProvider.GetTaskTitle]: Info object is expected.");
            }

            var ti = infoObj.TypeInfo;

            bool isBinding = ti.IsBinding;
            string cultureCode = CultureHelper.PreferredUICultureCode;
            string objectTypeName = ti.GetNiceObjectTypeName();
            string name;

            if (isBinding && (th != null))
            {
                // Binding name
                string parentType = infoObj.ParentObjectType;
                name = th.GetCodeName(parentType, infoObj.ObjectParentID);

                switch (taskType)
                {
                    case TaskTypeEnum.CreateObject:
                    case TaskTypeEnum.DeleteObject:
                        {
                            // Get parent info
                            GeneralizedInfo parent = ModuleManager.GetReadOnlyObject(parentType);
                            if (parent == null)
                            {
                                // ### Special case: get first dependency instead of parent
                                var firstDependency = ti.ObjectDependencies.FirstOrDefault();
                                if (firstDependency != null)
                                {
                                    parentType = firstDependency.DependencyObjectType;
                                    parent = ModuleManager.GetReadOnlyObject(parentType);
                                }
                            }
                            string parentTypeName = ResHelper.GetAPIString((parent != null) ? TypeHelper.GetObjectTypeResourceKey(parent.TypeInfo.ObjectType) : parentType, parentType);
                            string dependencyTypeName;
                            string dependencyName;

                            if (ti.IsSiteBinding)
                            {
                                // Get site dependency info
                                GeneralizedInfo dependencyObj = ModuleManager.GetReadOnlyObject(PredefinedObjectType.SITE);

                                dependencyTypeName = dependencyObj.TypeInfo.GetNiceObjectTypeName();
                                dependencyName = th.GetCodeName(PredefinedObjectType.SITE, infoObj.ObjectSiteID);
                            }
                            else
                            {
                                // Get dependency info
                                var firstDependency = ti.ObjectDependencies.First();

                                string dependencyColumn = firstDependency.DependencyColumn;
                                string dependencyType = firstDependency.DependencyObjectType;

                                GeneralizedInfo dependencyObj = ModuleManager.GetReadOnlyObject(dependencyType);

                                dependencyTypeName = dependencyObj.TypeInfo.GetNiceObjectTypeName();
                                dependencyName = th.GetCodeName(dependencyType, ValidationHelper.GetInteger(infoObj.GetValue(dependencyColumn), 0));
                            }

                            // Get full string
                            string strFormat;

                            if (taskType == TaskTypeEnum.CreateObject)
                            {
                                strFormat = ResHelper.GetAPIString("TaskTitle.CreateBindingName", "{0} '{1}' to {2} '{3}'");
                            }
                            else
                            {
                                strFormat = ResHelper.GetAPIString("TaskTitle.DeleteBindingName", "{0} '{1}' from {2} '{3}'");
                            }
                            name = string.Format(strFormat, parentTypeName, ResHelper.LocalizeString(name), dependencyTypeName, dependencyName);
                        }
                        break;

                    default:
                        // Especially for bindings with attributes
                        name = objectTypeName + " '" + ResHelper.LocalizeString(name) + "'";
                        break;
                }
            }
            else
            {
                // Get display name or at least identifier
                string objectName = !string.IsNullOrEmpty(infoObj.ObjectDisplayName) ? infoObj.ObjectDisplayName : infoObj.ObjectID.ToString();

                // Get resource string for standard object
                name = objectTypeName + " '" + ResHelper.LocalizeString(objectName) + "'";
            }

            // Task title
            string title;
            switch (taskType)
            {
                case TaskTypeEnum.CreateObject:
                    // Insert
                    if (isBinding)
                    {
                        title = ResHelper.GetAPIString("TaskTitle.CreateBinding", cultureCode, "Add {0}");
                    }
                    else
                    {
                        title = ResHelper.GetAPIString("TaskTitle.CreateObject", cultureCode, "Create {0}");
                    }
                    break;

                case TaskTypeEnum.UpdateObject:
                    // Update
                    title = ResHelper.GetAPIString("TaskTitle.UpdateObject", cultureCode, "Update {0}");
                    break;

                case TaskTypeEnum.DeleteObject:
                    // Delete
                    if (isBinding)
                    {
                        title = ResHelper.GetAPIString("TaskTitle.DeleteBinding", cultureCode, "Remove {0}");
                    }
                    else
                    {
                        title = ResHelper.GetAPIString("TaskTitle.DeleteObject", cultureCode, "Delete {0}");
                    }
                    break;

                case TaskTypeEnum.AddToSite:
                    // Add to site
                    title = ResHelper.GetAPIString("TaskTitle.AddToSite", cultureCode, "Add {0} to site");
                    break;

                case TaskTypeEnum.RemoveFromSite:
                    // Remove from site
                    title = ResHelper.GetAPIString("TaskTitle.RemoveFromSite", cultureCode, "Remove {0} from site");
                    break;

                default:
                    // Default (unknown) task
                    title = ResHelper.GetAPIString("TaskTitle.Unknown", cultureCode, "[Unknown] {0}");
                    break;
            }

            return TextHelper.LimitLength(String.Format(title, name), 450);
        }


        /// <summary>
        /// Gets the list of server IDs for which the staging task should be logged.
        /// </summary>
        /// <param name="task">Synchronization task.</param>
        /// <param name="siteId">Site ID.</param>
        /// <param name="serverId">Server ID.</param>
        protected virtual IList<int> GetServerIdsToLogTaskToInternal(StagingTaskInfo task, int siteId, int serverId)
        {
            if (serverId > 0)
            {
                var server = ServerInfoProvider.GetServerInfo(serverId);
                if (server != null && !task.WasProcessed(server.ServerName))
                {
                    return new[] { server.ServerID };
                }
            }
            else
            {
                var enabledServers = serverId == SynchronizationInfoProvider.ENABLED_SERVERS;
                return GetServerIdsToLogTaskTo(task, new[] { siteId }, enabledServers);
            }

            return new List<int>();
        }


        /// <summary>
        /// Gets the list of server IDs for which the staging task should be logged.
        /// </summary>
        /// <param name="task">Synchronization task.</param>
        /// <param name="siteIds">List of site IDs to limit the set of staging servers to.</param>
        /// <param name="enabledServers">Indicates if only enabled servers should be included.</param>
        internal static IList<int> GetServerIdsToLogTaskTo(StagingTaskInfo task, IList<int> siteIds, bool enabledServers)
        {
            var where = GetServersToLogWhereCondition(siteIds, enabledServers);

            return ServerInfoProvider.GetServers()
                                     .Columns("ServerID", "ServerName")
                                     .Where(where)
                                     .TypedResult
                                     .Where(server => !task.WasProcessed(server.ServerName))
                                     .Select(server => server.ServerID)
                                     .ToList();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Updates <see cref="TaskGroupTaskInfo"/> binding, by switching old task ID to a newly created task that replaces the old one.
        /// </summary>
        /// <param name="taskObj">New staging task</param>
        /// <param name="oldTasksWhere">Where condition for old tasks that will be deleted, e.g. UpdateObj is deleted when DeleteObj task is created for same object</param>
        private void AssociateOriginalTaskGroupsWithNewTask(StagingTaskInfo taskObj, WhereCondition oldTasksWhere)
        {
            // We need recreate bindings between task and task group for newly created staging task, get all old bindings
            // and recreate them for all other task groups except the current one, because that one was already created
            if (SynchronizationActionContext.CurrentTaskGroups.Any())
            {
                var taskGroupsIDs = SynchronizationActionContext.CurrentTaskGroups.Select(tg => tg.TaskGroupID);
                oldTasksWhere = oldTasksWhere.WhereNotIn("TaskGroupID", taskGroupsIDs.ToArray<int>());
            }


            TaskGroupTaskInfoProvider.UpdateTaskGroupTask(oldTasksWhere, new Dictionary<string, object>() { { "TaskID", taskObj.TaskID }, });
        }


        /// <summary>
        /// Updates <see cref="StagingTaskUserInfo"/> binding, by switching old task ID to a newly created task that replaces the old one.
        /// </summary>
        /// <param name="taskObj">New staging task</param>
        /// <param name="oldTasksWhere">Where condition for old tasks that will be deleted, e.g. UpdateObj is deleted when DeleteObj task is created for same object</param>
        private static void AssociateOriginalUsersWithNewTask(StagingTaskInfo taskObj, WhereCondition oldTasksWhere)
        {
            // We need recreate bindings between user and staging task for newly created staging task, get all old bindings
            // and recreate them for all other users except the current one, because that one was already created
            StagingTaskUserInfoProvider.UpdateStagingTaskUsers(oldTasksWhere.WhereNotEquals("UserID", CMSActionContext.CurrentUser.UserID), new Dictionary<string, object>() { { "TaskID", taskObj.TaskID }, });
        }


        private static IWhereCondition GetServersToLogWhereCondition(IList<int> siteIds, bool onlyEnabledServers)
        {
            var where = new WhereCondition();
            var sites = siteIds.Where(siteId => siteId != 0).ToList();

            if (sites.Count > 0)
            {
                where.WhereIn("ServerSiteID", sites);
            }

            if (onlyEnabledServers)
            {
                where.WhereTrue("ServerEnabled");
            }

            return where;
        }

        #endregion
    }
}