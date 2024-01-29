using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    using TypedDataSet = InfoDataSet<IntegrationSynchronizationInfo>;

    /// <summary>
    /// Class providing IntegrationSynchronizationInfo management.
    /// </summary>
    public class IntegrationSynchronizationInfoProvider : AbstractInfoProvider<IntegrationSynchronizationInfo, IntegrationSynchronizationInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all integration synchronization records.
        /// </summary>
        public static ObjectQuery<IntegrationSynchronizationInfo> GetIntegrationSynchronizations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns integration synchronization with specified ID.
        /// </summary>
        /// <param name="synchronizationId">Integration synchronization ID</param>        
        public static IntegrationSynchronizationInfo GetIntegrationSynchronizationInfo(int synchronizationId)
        {
            return ProviderObject.GetInfoById(synchronizationId);
        }


        /// <summary>
        /// Gets synchronization specified by connector and task identifiers.
        /// </summary>
        /// <param name="connectorId">Connector identifier</param>
        /// <param name="taskId">Task identifier</param>      
        public static IntegrationSynchronizationInfo GetIntegrationSynchronizationInfo(int connectorId, int taskId)
        {
            var where = new WhereCondition().WhereEquals("SynchronizationConnectorID", connectorId).WhereEquals("SynchronizationTaskID", taskId);

            return GetIntegrationSynchronizations().Where(where).TopN(1).BinaryData(true).FirstObject;
        }


        /// <summary>
        /// Gets list of connector IDs for pending external tasks.
        /// </summary>
        /// <returns>List of connector IDs for pending external tasks</returns>
        public static List<int> GetConnectorIdsForExternalTasks()
        {
            DataSet ds = ConnectionHelper.ExecuteQuery("integration.synchronization.selectconnectors", null, "TaskIsInbound = 1");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return (from DataRow row in ds.Tables[0].Rows select (int)row["SynchronizationConnectorID"]).ToList();
            }
            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified integration synchronization.
        /// </summary>
        /// <param name="synchronizationObj">Integration synchronization to be set</param>
        public static void SetIntegrationSynchronizationInfo(IntegrationSynchronizationInfo synchronizationObj)
        {
            ProviderObject.SetInfo(synchronizationObj);
        }


        /// <summary>
        /// Deletes specified integration synchronization.
        /// </summary>
        /// <param name="synchronizationObj">Integration synchronization to be deleted</param>
        public static void DeleteIntegrationSynchronizationInfo(IntegrationSynchronizationInfo synchronizationObj)
        {
            ProviderObject.DeleteInfo(synchronizationObj);
        }


        /// <summary>
        /// Deletes integration synchronization with specified ID.
        /// </summary>
        /// <param name="synchronizationId">Integration synchronization ID</param>
        public static void DeleteIntegrationSynchronizationInfo(int synchronizationId)
        {
            IntegrationSynchronizationInfo synchronizationObj = GetIntegrationSynchronizationInfo(synchronizationId);
            DeleteIntegrationSynchronizationInfo(synchronizationObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Sets a flag indicating that task is being processed.
        /// </summary>
        /// <param name="connectorId">Connector identifier</param>
        /// <param name="taskId">Task identifier</param>
        /// <param name="isRunning">Flag indicating that task is being processed</param>
        public static void SetIsRunning(int connectorId, int taskId, bool isRunning)
        {
            IntegrationSynchronizationInfo synchronizationObj = GetIntegrationSynchronizationInfo(connectorId, taskId);
            if (synchronizationObj != null)
            {
                synchronizationObj.SynchronizationIsRunning = isRunning;
                SetIntegrationSynchronizationInfo(synchronizationObj);
            }
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(IntegrationSynchronizationInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                // Delete task if deleted synchronization was the last one
                if (GetIntegrationSynchronizations().WhereID("SynchronizationTaskID", info.SynchronizationTaskID).Count <= 0)
                {
                    IntegrationTaskInfoProvider.DeleteIntegrationTaskInfo(info.SynchronizationTaskID);
                }
            }
        }

        #endregion
    }
}