using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing SynchronizationInfo management.
    /// </summary>
    public class SynchronizationInfoProvider : AbstractInfoProvider<SynchronizationInfo, SynchronizationInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Constant used to specify all the enabled servers.
        /// </summary>
        public const int ENABLED_SERVERS = LogObjectChangeSettings.ENABLED_SERVERS;

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all synchronization records.
        /// </summary>
        public static ObjectQuery<SynchronizationInfo> GetSynchronizations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the SynchronizationInfo structure for the specified synchronization.
        /// </summary>
        /// <param name="synchronizationId">Synchronization id</param>
        public static SynchronizationInfo GetSynchronizationInfo(int synchronizationId)
        {
            return ProviderObject.GetInfoById(synchronizationId);
        }


        /// <summary>
        /// Returns all the tasks synchronization info records based on given parameters.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method returns records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <param name="taskIds">Task IDs</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<SynchronizationInfo> GetTasksSynchronization(ICollection<int> taskIds, int serverId = 0, int siteId = 0)
        {
            return ProviderObject.GetTasksSynchronizationInternal(taskIds, serverId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified synchronization.
        /// </summary>
        /// <param name="synchronization">Synchronization to set</param>
        public static void SetSynchronizationInfo(SynchronizationInfo synchronization)
        {
            ProviderObject.SetInfo(synchronization);
        }


        /// <summary>
        /// Deletes specified synchronization.
        /// </summary>
        /// <param name="synchronizationObj">Synchronization object</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        public static void DeleteSynchronizationInfo(SynchronizationInfo synchronizationObj, bool deleteTask = true)
        {
            ProviderObject.DeleteSynchronizationInfoInternal(synchronizationObj, deleteTask);
        }


        /// <summary>
        /// Deletes specified synchronization.
        /// </summary>
        /// <param name="synchronizationId">Synchronization id</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        public static void DeleteSynchronizationInfo(int synchronizationId, bool deleteTask = true)
        {
            ProviderObject.DeleteSynchronizationInfoInternal(synchronizationId, deleteTask);
        }


        /// <summary>
        /// Deletes the synchronization info records.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method deletes records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <remarks>This site does not clean orphaned staging tasks. Call <see cref="StagingTaskInfoProvider.DeleteOrphanedTasks(ICollection{int})"/> if appropriate.</remarks>
        /// <param name="taskIds">Task IDs</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteSynchronizationInfos(ICollection<int> taskIds, int serverId, int siteId)
        {
            ProviderObject.DeleteSynchronizationInfoInternal(taskIds, serverId, siteId);
        }


        /// <summary>
        /// Deletes the synchronization info record.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method deletes records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        public static void DeleteSynchronizationInfo(int taskId, int serverId, int siteId, bool deleteTask = true)
        {
            ProviderObject.DeleteSynchronizationInfoInternal(taskId, serverId, siteId, deleteTask);
        }


        ///<summary>
        /// Deletes all synchronizations records for given server (instance) name except the given server.
        /// Global synchronization tasks are created for all servers. This method deletes records for all servers
        /// with given server name except one. 
        ///</summary>
        /// <remarks>This site does not clean orphaned staging tasks. Call <see cref="StagingTaskInfoProvider.DeleteOrphanedTasks(ICollection{int})"/> if appropriate.</remarks>
        ///<param name="taskIds">Task identifier</param>
        ///<param name="server">Server on which the tasks were processed</param>
        public static void DeleteInstanceGlobalTasks(ICollection<int> taskIds, ServerInfo server)
        {
            ProviderObject.DeleteInstanceGlobalTasksInternal(taskIds, server);
        }


        /// <summary>
        /// Creates synchronization records for given task and servers.
        /// </summary>
        /// <param name="taskId">Task ID.</param>
        /// <param name="serverIds">Server IDs</param>
        public static void CreateSynchronizationRecords(int taskId, IEnumerable<int> serverIds)
        {
            foreach (var serverId in serverIds)
            {
                var record = new SynchronizationInfo
                {
                    SynchronizationServerID = serverId,
                    SynchronizationTaskID = taskId
                };

                SetSynchronizationInfo(record);
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns all the tasks synchronization info records based on given parameters.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method returns records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<SynchronizationInfo> GetTaskSynchronizationInternal(int taskId, int serverId = 0, int siteId = 0)
        {
            return GetTasksSynchronizationInternal(new List<int> { taskId }, serverId, siteId);
        }


        /// <summary>
        /// Returns all the tasks synchronization info records based on given parameters.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method returns records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <param name="taskIds">Task IDs</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<SynchronizationInfo> GetTasksSynchronizationInternal(ICollection<int> taskIds, int serverId = 0, int siteId = 0)
        {
            var query = GetSynchronizations().WhereIn("SynchronizationTaskID", taskIds);

            if (serverId > 0)
            {
                query.Where("SynchronizationServerID", QueryOperator.Equals, serverId);
            }
            else if (siteId > 0)
            {
                query.WhereIn("SynchronizationServerID", ServerInfoProvider.GetServers().Where("ServerSiteID", QueryOperator.Equals, siteId).WhereTrue("ServerEnabled"));
            }

            return query;
        }


        /// <summary>
        /// Deletes specified synchronization.
        /// </summary>
        /// <param name="synchronizationObj">Synchronization object</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        protected virtual void DeleteSynchronizationInfoInternal(SynchronizationInfo synchronizationObj, bool deleteTask = true)
        {
            var taskId = synchronizationObj.SynchronizationTaskID;

            DeleteInfo(synchronizationObj);

            // Delete task if no more synchronization records left
            if (deleteTask && (GetTaskSynchronizationInternal(taskId).Count == 0))
            {
                StagingTaskInfoProvider.DeleteTaskInfo(taskId);
            }
        }


        /// <summary>
        /// Deletes specified synchronization.
        /// </summary>
        /// <param name="synchronizationId">Synchronization id</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        protected virtual void DeleteSynchronizationInfoInternal(int synchronizationId, bool deleteTask = true)
        {
            SynchronizationInfo synchronizationObj = GetSynchronizationInfo(synchronizationId);
            DeleteSynchronizationInfoInternal(synchronizationObj, deleteTask);
        }


        /// <summary>
        /// Deletes the synchronization info record.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method deletes records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <remarks>This site does not clean orphaned staging tasks. Call <see cref="StagingTaskInfoProvider.DeleteOrphanedTasks(ICollection{int})"/> if appropriate.</remarks>
        /// <param name="taskIds">Task IDs</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual void DeleteSynchronizationInfoInternal(ICollection<int> taskIds, int serverId, int siteId)
        {
            if (!taskIds.Any())
            {
                return;
            }

            var deleteWhereCondition = new WhereCondition().WhereIn("SynchronizationTaskID", taskIds);

            if (serverId > 0)
            {
                deleteWhereCondition.Where("SynchronizationServerID", QueryOperator.Equals, serverId);
            }
            else if (siteId > 0)
            {
                deleteWhereCondition.WhereIn("SynchronizationServerID", ServerInfoProvider.GetServers().Where("ServerSiteID", QueryOperator.Equals, siteId).WhereTrue("ServerEnabled"));
            }

            BulkDelete(deleteWhereCondition);
        }


        /// <summary>
        /// Deletes the synchronization info record.
        /// When serverId is specified, the siteId parameter is ignored. When siteId is specified
        /// and serverId is 0 method deletes records that belongs to all servers that are assigned to given site.
        /// </summary>
        /// <param name="taskId">Task ID</param>
        /// <param name="serverId">Server ID</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="deleteTask">When true and after delete there are no synchronization records with same task left, the task will be deleted as well.</param>
        protected virtual void DeleteSynchronizationInfoInternal(int taskId, int serverId, int siteId, bool deleteTask = true)
        {
            if (taskId <= 0)
            {
                return;
            }

            DeleteSynchronizationInfoInternal(new List<int> { taskId }, serverId, siteId);

            // Delete task if no more synchronization records left
            if (deleteTask && (GetTaskSynchronizationInternal(taskId).Count == 0))
            {
                StagingTaskInfoProvider.DeleteTaskInfo(taskId);
            }
        }


        /// <summary>
        /// Deletes all synchronizations records for given server (instance) name except the given server.
        /// Global synchronization tasks are created for all servers. This method deletes records for all servers
        /// with given server name except one. 
        /// </summary>
        /// <remarks>This site does not clean orphaned staging tasks. Call <see cref="StagingTaskInfoProvider.DeleteOrphanedTasks(ICollection{int})"/> if appropriate.</remarks>
        /// <param name="taskIds">Task identifier</param>
        /// <param name="server">Server on which the tasks were processed</param>
        protected virtual void DeleteInstanceGlobalTasksInternal(ICollection<int> taskIds, ServerInfo server)
        {
            if (!taskIds.Any())
            {
                return;
            }

            var deleteCondition = new WhereCondition().WhereIn("SynchronizationTaskID", taskIds);
            if (server != null)
            {
                deleteCondition.WhereIn("SynchronizationServerID", ServerInfoProvider.GetServers().Column("ServerID").Where("ServerName", QueryOperator.Equals, server.ServerName));
            }

            BulkDelete(deleteCondition);
        }

        #endregion
    }
}