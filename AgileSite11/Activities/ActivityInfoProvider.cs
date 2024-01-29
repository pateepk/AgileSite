using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Activities
{
    /// <summary>
    /// Class providing ActivityInfo management.
    /// </summary>
    public class ActivityInfoProvider : AbstractInfoProvider<ActivityInfo, ActivityInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ActivityInfo objects.
        /// </summary>
        public static ObjectQuery<ActivityInfo> GetActivities()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns activity with specified ID.
        /// </summary>
        /// <param name="activityId">Activity ID</param>        
        public static ActivityInfo GetActivityInfo(int activityId)
        {
            return ProviderObject.GetInfoById(activityId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified activity. If contact set as ActivityContactID is merged, this method will not update it to its parent. It is up to called to ensure that activities are
        /// property assigned to not merged contact.
        /// </summary>
        /// <param name="activityObj">Activity to be set</param>
        /// <remarks>
        /// This method should not be used directly. For inserting new activity info use implementation of IActivityRepository.
        /// </remarks>
        public static void SetActivityInfo(ActivityInfo activityObj)
        {
            ProviderObject.SetInfo(activityObj);
        }


        /// <summary>
        /// Deletes specified activity.
        /// </summary>
        /// <param name="activityObj">Activity to be deleted</param>
        public static void DeleteActivityInfo(ActivityInfo activityObj)
        {
            ProviderObject.DeleteInfo(activityObj);
        }


        /// <summary>
        /// Deletes activity with specified ID.
        /// </summary>
        /// <param name="activityId">Activity ID</param>
        public static void DeleteActivityInfo(int activityId)
        {
            ActivityInfo activityObj = GetActivityInfo(activityId);
            DeleteActivityInfo(activityObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns last contact's activity, may be restricted to specific activity type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="activityType">Activity type - optional</param>
        public static ActivityInfo GetContactsLastActivity(int contactId, string activityType)
        {
            return ProviderObject.GetContactsEdgeActivity(contactId, activityType, OrderDirection.Descending);
        }


        /// <summary>
        /// Returns first contact's activity, may be restricted to specific activity type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="activityType">Activity type - optional</param>
        public static ActivityInfo GetContactsFirstActivity(int contactId, string activityType)
        {
            return ProviderObject.GetContactsEdgeActivity(contactId, activityType, OrderDirection.Ascending);
        }


        /// <summary>
        /// Returns true if the contact did a specified activity.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="activityType">Name of the activity to check (can specify more than one separated with semicolon, all of the types match than)</param>
        /// <param name="cancelActivityType">Name of the activity which cancels the original activity (for example UnsubscribeNewsletter is a canceling event for SubscribeNewsletter etc.)</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="whereCondition">Additional WHERE condition</param>
        public static bool ContactDidActivity(int contactId, string activityType, string cancelActivityType, int lastXDays, string whereCondition)
        {
            string where = GetActivityWhere(activityType, cancelActivityType, lastXDays, whereCondition, contactId);

            var activity = GetActivities()
                .Where(where)
                .TopN(1)
                .Column("ActivityID")
                .FirstOrDefault();

            return (activity != null);
        }


        /// <summary>
        /// Returns activities of specified type for contact.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="activityType">Name of the activity to check (can specify more than one separated with semicolon, all of the types match than)</param>
        /// <param name="cancelActivityType">Name of the activity which cancels the original activity (for example UnsubscribeNewsletter is a canceling event for SubscribeNewsletter etc.)</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        /// <param name="whereCondition">Additional WHERE condition</param>
        public static ObjectQuery<ActivityInfo> GetContactActivities(int contactId, string activityType, string cancelActivityType, int lastXDays, string whereCondition)
        {
            string where = GetActivityWhere(activityType, cancelActivityType, lastXDays, whereCondition, contactId);

            return GetActivities().Where(@where);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ActivityInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            // When inserting new record set creation time
            if (info.ActivityCreated == DateTimeHelper.ZERO_TIME)
            {
                info.ActivityCreated = DateTime.Now;
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns TOP 1 from the contact's activities ordered by <see cref="ActivityInfo.ActivityCreated"/>.
        /// ActivityType filter may be specified by setting value of <paramref name="activityType"/>. 
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="activityType">Activity type - optional</param>
        /// <param name="orderBy">Order by</param>
        /// <remarks>
        /// Use <see cref="OrderDirection.Ascending"/> to obtain first contact's activity or 
        /// <see cref="OrderDirection.Descending"/> to obtain last contact's activity.
        /// </remarks>
        /// <remarks>If activityType is set to null/empty, returns first/last activity among all activities</remarks>
        protected virtual ActivityInfo GetContactsEdgeActivity(int contactId, string activityType, OrderDirection orderBy)
        {
            if (contactId > 0)
            {
                var query = GetActivities().WhereEquals("ActivityContactID", contactId)
                                           .WhereNotNull("ActivityCreated")
                                           .WhereNotEquals("ActivityCreated", DateTimeHelper.ZERO_TIME);

                if (!String.IsNullOrEmpty(activityType))
                {
                    // Specify activity type to restrict the result
                    query.WhereEquals("ActivityType", activityType);
                }

                return query.OrderBy(orderBy, "ActivityCreated")
                            .TopN(1)
                            .FirstOrDefault();
            }

            return null;
        }

        #endregion


        #region "Private methods"

        private static string GetActivityWhere(string activityType, string cancelActivityType, int lastXDays, string whereCondition, int contactId)
        {
            string where = "ActivityContactID = " + contactId;
            if (!string.IsNullOrEmpty(activityType))
            {
                string[] types = activityType.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string typeWhere = null;
                foreach (string type in types)
                {
                    typeWhere = SqlHelper.AddWhereCondition(typeWhere, "ActivityType = N'" + SqlHelper.EscapeQuotes(type) + "'", "OR");
                }
                @where = SqlHelper.AddWhereCondition(@where, typeWhere);
            }
            if (!string.IsNullOrEmpty(cancelActivityType))
            {
                // Exclude activities which has been canceled by its counter activity (subscribe vs. unsubscribe)
                @where = SqlHelper.AddWhereCondition(@where, "NOT EXISTS(SELECT ActivityID FROM OM_Activity AS A2 WHERE A2.ActivityType = N'" + SqlHelper.EscapeQuotes(cancelActivityType) + "' AND OM_Activity.ActivityItemID = A2.ActivityItemID AND A2.ActivityCreated > OM_Activity.ActivityCreated AND OM_Activity.ActivityContactID = A2.ActivityContactID)");
            }
            if (lastXDays > 0)
            {
                var dateTimeNow = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                @where = SqlHelper.AddWhereCondition(@where, "ActivityCreated >= cast('" + dateTimeNow.AddDays(-lastXDays).ToString(CultureInfo.InvariantCulture) + "' as datetime)");
            }
            if (!string.IsNullOrEmpty(whereCondition))
            {
                @where = SqlHelper.AddWhereCondition(@where, whereCondition);
            }
            return @where;
        }

        #endregion


        #region "Bulk methods"

        /// <summary>
        /// Moves all activities assigned to the contact identified by given <paramref name="sourceContactID"/> to the contact identified by <paramref name="targetContactID"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only in the merging process. Note that there is no consistency check on whether the contacts with given IDs exist or not (nor is the 
        /// foreign key check in DB). Caller of this method should perform all the necessary checks prior to the method invocation.
        /// </remarks>
        /// <param name="sourceContactID">Identifier of the contact the activities are moved from</param>
        /// <param name="targetContactID">Identifier of the contact the activities are moved to</param>
        public static void BulkMoveActivitiesToAnotherContact(int sourceContactID, int targetContactID)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                {"ActivityContactID", targetContactID}
            };

            var whereCondition = new WhereCondition().WhereEquals("ActivityContactID", sourceContactID);

            ProviderObject.UpdateData(whereCondition, updateDictionary);
        }


        /// <summary>
        /// Bulk inserts the given <paramref name="activities"/>.
        /// </summary>
        /// <param name="activities">Data table containing values to be inserted</param>
        public static void BulkInsert(IEnumerable<IDataTransferObject> activities)
        {
            using (new CMSConnectionScope(DatabaseSeparationHelper.OM_CONNECTION_STRING, true))
            {
                var dataTableContainer = Service.Resolve<IDataTableProvider>().ConvertToDataTable(activities, PredefinedObjectType.ACTIVITY);
                ConnectionHelper.BulkInsert(dataTableContainer.DataTable, "OM_Activity", new BulkInsertSettings
                {
                    Options = SqlBulkCopyOptions.CheckConstraints
                });
            }
        }


        /// <summary>
        /// Bulk inserts the given <paramref name="activities"/> and returns the last inserted ActivityID.
        /// </summary>
        /// <param name="activities">Data table containing values to be inserted</param>
        public static int BulkInsertAndGetLastID(IEnumerable<IDataTransferObject> activities)
        {
            int lastID = 0;
            var connectionString = ConnectionHelper.GetConnectionString(DatabaseSeparationHelper.OM_CONNECTION_STRING, true) ?? ConnectionHelper.ConnectionString;
            var dataTableContainer = Service.Resolve<IDataTableProvider>().ConvertToDataTable(activities, PredefinedObjectType.ACTIVITY);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("Proc_OM_Activity_BulkInsertActivities", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameterCollection param = cmd.Parameters;
                    param.Add("@LastID", SqlDbType.Int).Direction = ParameterDirection.Output;
                    var rt = param.AddWithValue("@Activities", dataTableContainer.DataTable);
                    rt.SqlDbType = SqlDbType.Structured;
                    rt.TypeName = "Type_OM_ActivityTable";

                    try
                    {
                        cmd.ExecuteNonQuery();
                        lastID = param["@LastID"].Value.ToInteger(0);
                    }
                    catch (SqlException ex)
                    {
                        EventLogProvider.LogException("Activity", "BULKINSERT", ex);
                    }
                }
            }

            return lastID;
        }

        #endregion
    }
}