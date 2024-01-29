using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;

namespace CMS.ContactManagement
{    
    /// <summary>
    /// Class providing ActivityRecalculationQueueInfo management.
    /// </summary>
    internal class ActivityRecalculationQueueInfoProvider : AbstractInfoProvider<ActivityRecalculationQueueInfo, ActivityRecalculationQueueInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivityRecalculationQueueInfoProvider()
            : base(ActivityRecalculationQueueInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ActivityRecalculationQueueInfo objects.
        /// </summary>
        public static ObjectQuery<ActivityRecalculationQueueInfo> GetActivityRecalculationQueueInfos()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ActivityRecalculationQueueInfo with specified ID.
        /// </summary>
        /// <param name="id">ActivityRecalculationQueueInfo ID</param>
        public static ActivityRecalculationQueueInfo GetActivityRecalculationQueueInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ActivityRecalculationQueueInfo.
        /// </summary>
        /// <param name="infoObj">ActivityRecalculationQueueInfo to be set</param>
        public static void SetActivityRecalculationQueueInfo(ActivityRecalculationQueueInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ActivityRecalculationQueueInfo.
        /// </summary>
        /// <param name="infoObj">ActivityRecalculationQueueInfo to be deleted</param>
        public static void DeleteActivityRecalculationQueueInfo(ActivityRecalculationQueueInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ActivityRecalculationQueueInfo with specified ID.
        /// </summary>
        /// <param name="id">ActivityRecalculationQueueInfo ID</param>
        public static void DeleteActivityRecalculationQueueInfo(int id)
        {
            ActivityRecalculationQueueInfo infoObj = GetActivityRecalculationQueueInfo(id);
            DeleteActivityRecalculationQueueInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"
	
        /// <summary>
        /// Bulk inserts the given <paramref name="activityRecalculationQueueInfos"/>.
        /// </summary>
        /// <param name="activityRecalculationQueueInfos">Data table containing values to be inserted</param>
        internal static void BulkInsert(IEnumerable<IDataTransferObject> activityRecalculationQueueInfos)
        {
            using (new CMSConnectionScope(DatabaseSeparationHelper.OM_CONNECTION_STRING, true))
            {
                var dataTableContainer = Service.Resolve<IDataTableProvider>().ConvertToDataTable(activityRecalculationQueueInfos, PredefinedObjectType.ACTIVITYRECALCULATIONQUEUEINFO);
                ConnectionHelper.BulkInsert(dataTableContainer.DataTable, "OM_ActivityRecalculationQueue", new BulkInsertSettings
                {
                    Options = SqlBulkCopyOptions.CheckConstraints
                });
            }
        }

        #endregion
    }
}