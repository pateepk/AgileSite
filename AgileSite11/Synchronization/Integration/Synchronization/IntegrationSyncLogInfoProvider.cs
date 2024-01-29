using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing IntegrationSyncLogInfo management.
    /// </summary>
    public class IntegrationSyncLogInfoProvider : AbstractInfoProvider<IntegrationSyncLogInfo, IntegrationSyncLogInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all integration synchronization logs.
        /// </summary>
        public static ObjectQuery<IntegrationSyncLogInfo> GetIntegrationSynchronizationLogs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns integration synchronization log with specified ID.
        /// </summary>
        /// <param name="logId">Integration synchronization log ID</param>        
        public static IntegrationSyncLogInfo GetIntegrationSyncLogInfo(int logId)
        {
            return ProviderObject.GetInfoById(logId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified integration synchronization log.
        /// </summary>
        /// <param name="logObj">Integration synchronization log to be set</param>
        public static void SetIntegrationSyncLogInfo(IntegrationSyncLogInfo logObj)
        {
            ProviderObject.SetInfo(logObj);
        }


        /// <summary>
        /// Deletes specified integration synchronization log.
        /// </summary>
        /// <param name="logObj">Integration synchronization log to be deleted</param>
        public static void DeleteIntegrationSyncLogInfo(IntegrationSyncLogInfo logObj)
        {
            ProviderObject.DeleteInfo(logObj);
        }


        /// <summary>
        /// Deletes integration synchronization log with specified ID.
        /// </summary>
        /// <param name="logId">Integration synchronization log ID</param>
        public static void DeleteIntegrationSyncLogInfo(int logId)
        {
            IntegrationSyncLogInfo logObj = GetIntegrationSyncLogInfo(logId);
            DeleteIntegrationSyncLogInfo(logObj);
        }


        /// <summary>
        /// Deletes integration synchronization log specified by task and connector identifiers.
        /// </summary>
        /// <param name="synchronizationId">Synchronization identifier</param>
        public static void DeleteIntegrationSyncLogs(int synchronizationId)
        {
            ProviderObject.DeleteIntegrationSyncLogsInternal(synchronizationId);
        }

        #endregion


        #region "Public methods - Advanced"

        // Here come advanced public methods. If there are no advanced public methods, remove the block.

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Deletes integration synchronization log specified by task and connector identifiers.
        /// </summary>
        /// <param name="synchronizationId">Synchronization identifier</param>
        protected virtual void DeleteIntegrationSyncLogsInternal(int synchronizationId)
        {
            string where = "(SyncLogSynchronizationID = " + synchronizationId + ")";

            ConnectionHelper.ExecuteQuery("integration.synclog.deleteall", null, where);
        }

        #endregion
    }
}