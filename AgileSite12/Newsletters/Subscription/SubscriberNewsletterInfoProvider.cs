using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing SubscriberNewsletterInfo management.
    /// </summary>
    public class SubscriberNewsletterInfoProvider : AbstractInfoProvider<SubscriberNewsletterInfo, SubscriberNewsletterInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the SubscriberNewsletterInfo objects.
        /// </summary>
        public static ObjectQuery<SubscriberNewsletterInfo> GetSubscriberNewsletters()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all approved SubscriberNewsletterInfo objects.
        /// </summary>
        public static ObjectQuery<SubscriberNewsletterInfo> GetApprovedSubscriberNewsletters()
        {
            return ProviderObject.GetApprovedSubscriberNewslettersInternal();
        }


        /// <summary>
        /// Returns the SubscriberNewsletterInfo structure for the specified subscriberID and newsletterID.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        public static SubscriberNewsletterInfo GetSubscriberNewsletterInfo(int subscriberId, int newsletterId)
        {
            return ProviderObject.GetSubscriberNewsletterInfoInternal(subscriberId, newsletterId);
        }


        /// <summary>
        /// Returns the SubscriberNewsletterInfo structure for the specified subscription hash.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash</param>
        public static SubscriberNewsletterInfo GetSubscriberNewsletterInfo(string subscriptionHash)
        {
            return ProviderObject.GetSubscriberNewsletterInfoInternal(subscriptionHash);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SubscriberNewsletterInfo.
        /// </summary>
        /// <param name="infoObj">SubscriberNewsletterInfo to be set</param>
        /// <exception cref="ArgumentNullException"><paramref name="infoObj"/> is null</exception>
        public static void SetSubscriberNewsletterInfo(SubscriberNewsletterInfo infoObj)
        {
            if (infoObj == null)
            {
                throw new ArgumentNullException("infoObj");
            }

            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified SubscriberNewsletterInfo.
        /// </summary>
        /// <param name="infoObj">SubscriberNewsletterInfo to be deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="infoObj"/> is null</exception>
        public static void DeleteSubscriberNewsletterInfo(SubscriberNewsletterInfo infoObj)
        {
            if (infoObj == null)
            {
                throw new ArgumentNullException("infoObj");
            }

            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes SubscriberNewsletterInfo specified by subscriberID and newsletterID.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        /// <exception cref="ArgumentException">No subscriber was found for provided subscriberID and newsletterID</exception>
        public static void DeleteSubscriberNewsletterInfo(int subscriberId, int newsletterId)
        {
            SubscriberNewsletterInfo infoObj = GetSubscriberNewsletterInfo(subscriberId, newsletterId);
            if (infoObj == null)
            {
                throw new ArgumentException("[SubscriberNewsletterInfoProvider.DeleteSubscriberNewsletterInfo]: No subscriber was found for provided subscriberID and newsletterID.");
            }

            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes SubscriberNewsletterInfo objects based on specified where condition.
        /// </summary>
        /// <param name="where">Where condition. Deletes all subscribers when where condition is set to null</param>
        public static void DeleteSubscriberNewsletterInfos(string where)
        {
            ProviderObject.DeleteSubscriberNewsletterInfosInternal(where);
        }


        /// <summary>
        /// Returns subscribers IDs for a specified newsletter.
        /// </summary>
        /// <param name="newsletterId">Id of newsletter</param>
        /// <returns>IDs of subscribers for given newsletter</returns>
        public static IList<int> GetNewsletterSubscribersIds(int newsletterId)
        {
            return ProviderObject.GetSubscribersIdsFromViewInternal(newsletterId);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns all existing subscriptions along with detailed information about the subscriber.
        /// </summary>
        /// <param name="where">WHERE parameter</param>
        /// <param name="orderBy">ORDER BY parameter</param>
        /// <param name="topN">TOP N parameter</param>
        /// <param name="columns">Selected columns</param>
        /// <returns>DataSet with subscriptions and their respective subscriber information</returns>
        public static DataSet GetSubscriptions(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Newsletter.SubscriberNewsletter.selectsubscriptions", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Removes specified subscriber from newsletter.
        /// </summary>
        /// <param name="subscriptionApprovalHash">Subscription approval hash.</param>
        /// <exception cref="ArgumentException">No subscriber was found for provided subscriptionApprovalHash</exception>
        public static void RemoveSubscriberFromNewsletter(string subscriptionApprovalHash)
        {
            var infoObj = GetSubscriberNewsletterInfo(subscriptionApprovalHash);
            if (infoObj == null)
            {
                throw new ArgumentException("[SubscriberNewsletterInfoProvider.RemoveSubscriberFromNewsletter]: No subscriber was found for provided subscriberID and newsletterID.");
            }

            DeleteSubscriberNewsletterInfo(infoObj);
        }


        /// <summary>
        /// Approve subscription.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <exception cref="ArgumentException">No subscriber was found for provided subscriberID and newsletterID</exception>
        public static void ApproveSubscription(int subscriberID, int newsletterID)
        {
            var info = GetSubscriberNewsletterInfo(subscriberID, newsletterID);
            if (info == null)
            {
                throw new ArgumentException("[SubscriberNewsletterInfoProvider.ApproveSubscription]: No subscriber was found for provided subscriberID and newsletterID.");
            }

            ProviderObject.ApproveSubscriptionInternal(info);
        }


        /// <summary>
        /// Reject subscription.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <exception cref="ArgumentException">No subscriber was found for provided subscriberID and newsletterID</exception>
        public static void RejectSubscription(int subscriberID, int newsletterID)
        {
            var info = GetSubscriberNewsletterInfo(subscriberID, newsletterID);
            if (info == null)
            {
                throw new ArgumentException("[SubscriberNewsletterInfoProvider.RejectSubscription]: No subscriber was found for provided subscriberID and newsletterID.");
            }

            ProviderObject.RejectSubscriptionInternal(info);
        }


        /// <summary>
        /// Set status of subscriber approval.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="status">Status of subscriber</param>
        /// <exception cref="ArgumentException">No subscriber was found for provided subscriberID and newsletterID</exception>
        public static void SetApprovalStatus(int subscriberID, int newsletterID, bool status)
        {
            if (status)
            {
                ApproveSubscription(subscriberID, newsletterID);
            }
            else
            {
                RejectSubscription(subscriberID, newsletterID);
            }
        }


        /// <summary>
        /// Adds specified subscriber to the newsletter.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        /// <param name="when">Date time</param>
        [Obsolete("Use CMS.Newsletters.AddSubscriberToNewsletter(int, int, DateTime, bool) instead.")]
        public static void AddSubscriberToNewsletter(int subscriberId, int newsletterId, DateTime when)
        {
            AddSubscriberToNewsletter(subscriberId, newsletterId, when, true);
        }


        /// <summary>
        /// Adds specified subscriber to the newsletter.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        /// <param name="when">Indication when subscription was created</param>
        /// <param name="approved">Subscription is approved</param>
        public static void AddSubscriberToNewsletter(int subscriberId, int newsletterId, DateTime when, bool approved)
        {
            ProviderObject.AddSubscriberToNewsletterInternal(subscriberId, newsletterId, when, approved);
        }


        /// <summary>
        /// Returns DataSet with subscribers by type for specified issue(newsletter).
        /// </summary>
        /// <param name="subscriberType">Type of the subscriber</param>
        /// <param name="newsletterID">Newsletter ID representing</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N </param>
        /// <param name="columns">Columns</param>
        /// <returns>Returns DataSet.</returns>
        public static DataSet GetSubscriptionsByType(string subscriberType, int newsletterID, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetSubscriptionsByTypeInternal(subscriberType, newsletterID, orderBy, topN, columns);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a query for all approved SubscriberNewsletterInfo objects.
        /// </summary>
        protected virtual ObjectQuery<SubscriberNewsletterInfo> GetApprovedSubscriberNewslettersInternal()
        {
            return GetObjectQuery()
                .Where(w => w.WhereTrue("SubscriptionApproved")
                    .Or()
                    .WhereNull("SubscriptionApproved"));
        }


        /// <summary>
        /// Returns SubscriberNewsletterInfo with specified ID.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        protected virtual SubscriberNewsletterInfo GetSubscriberNewsletterInfoInternal(int subscriberId, int newsletterId)
        {
            return GetObjectQuery().TopN(1)
                .Where(w => w.WhereEquals("SubscriberID", subscriberId)
                    .WhereEquals("NewsletterID", newsletterId)).FirstOrDefault();
        }


        /// <summary>
        /// Deletes SubscriberNewsletterInfo objects based on specified where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteSubscriberNewsletterInfosInternal(string where)
        {
            BulkDelete(new WhereCondition(where));
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns the SubscriberNewsletterInfo structure for the specified subscriptionHash.
        /// </summary>
        /// <param name="subscriptionHash">Subscription hash.</param>
        protected virtual SubscriberNewsletterInfo GetSubscriberNewsletterInfoInternal(string subscriptionHash)
        {
            return GetObjectQuery().WhereEquals("SubscriptionApprovalHash", subscriptionHash).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Approves subscription.
        /// </summary>
        /// <param name="infoObj">SubscriberNewsletter object</param>
        protected virtual void ApproveSubscriptionInternal(SubscriberNewsletterInfo infoObj)
        {
            if (!infoObj.SubscriptionApproved)
            {
                infoObj.SubscriptionApproved = true;
                infoObj.SubscriptionApprovedWhen = DateTime.Now;
                SetInfo(infoObj);
            }
        }


        /// <summary>
        /// Rejects subscription.
        /// </summary>
        /// <param name="infoObj">SubscriberNewsletter object</param>
        protected virtual void RejectSubscriptionInternal(SubscriberNewsletterInfo infoObj)
        {
            if (infoObj.SubscriptionApproved)
            {
                infoObj.SubscriptionApproved = false;
                SetInfo(infoObj);
            }
        }


        /// <summary>
        /// Adds specified subscriber to the newsletter.
        /// </summary>
        /// <param name="subscriberId">SubscriberID</param>
        /// <param name="newsletterId">NewsletterID</param>
        /// <param name="when">Indication when subscription was created</param>
        /// <param name="approved">Subscription is approved</param>
        protected virtual void AddSubscriberToNewsletterInternal(int subscriberId, int newsletterId, DateTime when, bool approved)
        {
            // Create new binding
            var infoObj = ProviderObject.CreateInfo();
            infoObj.NewsletterID = newsletterId;
            infoObj.SubscriberID = subscriberId;
            infoObj.SubscribedWhen = when;
            infoObj.SubscriptionApproved = approved;

            // Save to the database
            SetInfo(infoObj);
        }


        /// <summary>
        /// Returns DataSet with subscribers by type for specified issue(newsletter).
        /// </summary>
        /// <param name="subscriberType">Type of the subscriber</param>
        /// <param name="newsletterID">Newsletter ID representing</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N </param>
        /// <param name="columns">Columns</param>
        /// <returns>Returns DataSet.</returns>
        protected virtual DataSet GetSubscriptionsByTypeInternal(string subscriberType, int newsletterID, string orderBy, int topN, string columns)
        {
            return GetObjectQuery()
                .TopN(topN)
                .Columns(columns)
                .WhereIn("SubscriberID", new IDQuery(SubscriberInfo.OBJECT_TYPE)
                                            .Column("SubscriberID")
                                            .WhereEquals("SubscriberType", subscriberType)
                                            .WhereEquals("NewsletterID", newsletterID))
                .OrderBy(orderBy);
        }


        /// <summary>
        /// Returns subscribers IDs for a specified newsletter.
        /// </summary>
        /// <param name="newsletterId">Id of newsletter</param>
        /// <returns>DataQuery containing all IDs of subscribers with a given newsletter</returns>
        protected virtual IList<int> GetSubscribersIdsFromViewInternal(int newsletterId)
        {
            return new DataQuery(SubscriberNewsletterInfo.OBJECT_TYPE, "selectsubscriptions")
                .WhereEquals("NewsletterID", newsletterId)
                .Column("SubscriberID")
                .GetListResult<int>();
        }


        /// <summary>
        /// Updates the data in the database based on the given where condition.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value].</param>
        internal static void UpdateDataInternal(WhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values);
        }

        #endregion
    }
}