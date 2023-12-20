using System;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Class providing notification subscription info management.
    /// </summary>
    public class NotificationSubscriptionInfoProvider : AbstractInfoProvider<NotificationSubscriptionInfo, NotificationSubscriptionInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the query for all notification subscriptions.
        /// </summary>
        public static ObjectQuery<NotificationSubscriptionInfo> GetNotificationSubscriptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the notification subscription info structure for the specified notification subscription.
        /// </summary>
        /// <param name="notificationSubscriptionId">Notification subscription ID</param>
        public static NotificationSubscriptionInfo GetNotificationSubscriptionInfo(int notificationSubscriptionId)
        {
            return ProviderObject.GetInfoById(notificationSubscriptionId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified notification subscription.
        /// </summary>
        /// <param name="notificationSubscription">Notification subscription to set</param>
        public static void SetNotificationSubscriptionInfo(NotificationSubscriptionInfo notificationSubscription)
        {
            // Check license for notifications
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Notifications);
            }

            ProviderObject.SetInfo(notificationSubscription);
        }


        /// <summary>
        /// Deletes specified notificationSubscription.
        /// </summary>
        /// <param name="infoObj">Notification subscription object</param>
        public static void DeleteNotificationSubscriptionInfo(NotificationSubscriptionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified notification subscription.
        /// </summary>
        /// <param name="notificationSubscriptionId">Notification subscription ID</param>
        public static void DeleteNotificationSubscriptionInfo(int notificationSubscriptionId)
        {
            NotificationSubscriptionInfo infoObj = GetNotificationSubscriptionInfo(notificationSubscriptionId);
            DeleteNotificationSubscriptionInfo(infoObj);
        }


         /// <summary>
        /// Returns complete WHERE condition for subscriptions which should be searched according to the given parameters.
        /// </summary>
        /// <param name="eventSource">Subscription event source</param>
        /// <param name="eventCode">Subscription event code</param>
        /// <param name="eventObjectId">Subscription event object ID</param>
        /// <param name="eventData1">Subscription event data 1</param>
        /// <param name="eventData2">Subscription event data 2</param>
        /// <param name="siteId">ID of the site where the event belongs</param>
        /// <param name="where">Additional WHERE condition</param>
        public static IWhereCondition GetWhereConditionObject(string eventSource, string eventCode, int eventObjectId, string eventData1, string eventData2, int siteId, IWhereCondition where)
        {
            var whereObj = new WhereCondition();

            if (!String.IsNullOrEmpty(eventSource))
            {
                whereObj.WhereEquals("SubscriptionEventSource", eventSource);
            }

            if (!String.IsNullOrEmpty(eventCode))
            {
                whereObj.WhereEquals("SubscriptionEventCode", eventCode);
            }

            if (eventObjectId > 0)
            {
                whereObj.WhereEquals("SubscriptionEventObjectID", eventObjectId);
            }

#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used.
            if (!String.IsNullOrEmpty(eventData1))
            {
                whereObj.WhereLike(eventData1.AsValue(), "SubscriptionEventData1".AsColumn());
            }

            if (!String.IsNullOrEmpty(eventData2))
            {
                whereObj.WhereLike(eventData2.AsValue(), "SubscriptionEventData2".AsColumn());
            }
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used.

            if (siteId > 0)
            {
                whereObj.WhereEqualsOrNull("SubscriptionSiteID", siteId);
            }
            else
            {
                whereObj.WhereNull("SubscriptionSiteID");
            }

            if (where != null)
            {
                whereObj.Where(new WhereCondition(where) { WhereIsComplex = true });
            }

            return whereObj;
        }

        #endregion
    }
}