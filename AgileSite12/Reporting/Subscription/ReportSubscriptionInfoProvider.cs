using System;

using CMS.DataEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing ReportSubscriptionInfo management.
    /// </summary>
    public class ReportSubscriptionInfoProvider : AbstractInfoProvider<ReportSubscriptionInfo, ReportSubscriptionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all report subscriptions.
        /// </summary>
        public static ObjectQuery<ReportSubscriptionInfo> GetSubscriptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns subscription with specified ID.
        /// </summary>
        /// <param name="subscriptionId">Subscription ID.</param>
        public static ReportSubscriptionInfo GetReportSubscriptionInfo(int subscriptionId)
        {
            return ProviderObject.GetInfoById(subscriptionId);
        }


        /// <summary>
        /// Returns subscription with specified GUID.
        /// </summary>
        /// <param name="subscriptionGUID">Subscription GUID.</param>
        public static ReportSubscriptionInfo GetReportSubscriptionInfo(Guid subscriptionGUID)
        {
            return ProviderObject.GetInfoByGuid(subscriptionGUID);
        }


        /// <summary>
        /// Sets (updates or inserts) specified subscription.
        /// </summary>
        /// <param name="subscriptionObj">Subscription to be set.</param>
        public static void SetReportSubscriptionInfo(ReportSubscriptionInfo subscriptionObj)
        {
            ProviderObject.SetInfo(subscriptionObj);
        }


        /// <summary>
        /// Deletes specified subscription.
        /// </summary>
        /// <param name="subscriptionObj">Subscription to be deleted.</param>
        public static void DeleteReportSubscriptionInfo(ReportSubscriptionInfo subscriptionObj)
        {
            ProviderObject.DeleteInfo(subscriptionObj);
        }


        /// <summary>
        /// Deletes subscription with specified ID.
        /// </summary>
        /// <param name="subscriptionId">Subscription ID.</param>
        public static void DeleteReportSubscriptionInfo(int subscriptionId)
        {
            ReportSubscriptionInfo subscriptionObj = GetReportSubscriptionInfo(subscriptionId);
            DeleteReportSubscriptionInfo(subscriptionObj);
        }


        /// <summary>
        /// Deletes the subscriptions based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        public static void DeleteSubscriptions(string where)
        {
            ProviderObject.BulkDelete(new WhereCondition(where));
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ReportSubscriptionInfo info)
        {
            if (info != null)
            {
                // Set custom data
                info.SetValue("ReportSubscriptionSettings", info.ReportSubscriptionSettings.GetData());

                base.SetInfo(info);
            }
        }

        #endregion
    }
}