using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing CreditEventInfo management.
    /// </summary>
    public class CreditEventInfoProvider : AbstractInfoProvider<CreditEventInfo, CreditEventInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all credit events.
        /// </summary>
        public static ObjectQuery<CreditEventInfo> GetCreditEvents()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns credit event with specified ID.
        /// </summary>
        /// <param name="eventId">Credit event ID</param>        
        public static CreditEventInfo GetCreditEventInfo(int eventId)
        {
            return ProviderObject.GetInfoById(eventId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified credit event.
        /// </summary>
        /// <param name="eventObj">Credit event to be set</param>
        public static void SetCreditEventInfo(CreditEventInfo eventObj)
        {
            ProviderObject.SetInfo(eventObj);
        }


        /// <summary>
        /// Deletes specified credit event.
        /// </summary>
        /// <param name="eventObj">Credit event to be deleted</param>
        public static void DeleteCreditEventInfo(CreditEventInfo eventObj)
        {
            ProviderObject.DeleteInfo(eventObj);
        }


        /// <summary>
        /// Deletes credit event with specified ID.
        /// </summary>
        /// <param name="eventId">Credit event ID</param>
        public static void DeleteCreditEventInfo(int eventId)
        {
            var eventObj = GetCreditEventInfo(eventId);
            DeleteCreditEventInfo(eventObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns the query for all credit events for given site. Reflects use global credit setting.
        /// </summary>
        /// <param name="siteId">Id of the site to get credit events for.</param>
        public static ObjectQuery<CreditEventInfo> GetCreditEvents(int siteId)
        {
            return ProviderObject.GetCreditEventsInternal(siteId);
        }


        /// <summary>
        /// Returns query for all customers' credit events on given site.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<CreditEventInfo> GetCreditEvents(int customerId, int siteId)
        {
            return ProviderObject.GetCreditEventsInternal(customerId, siteId);
        }


        /// <summary>
        /// Deletes all customers' credit events.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param> 
        public static void DeleteCreditEvents(int customerId, int siteId)
        {
            ProviderObject.DeleteCreditEventsInternal(customerId, siteId);
        }


        /// <summary>
        /// Returns customer's total credit.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param> 
        public static decimal GetTotalCredit(int customerId, int siteId)
        {
            return ProviderObject.GetTotalCreditInternal(customerId, siteId);
        }


        /// <summary>
        /// Returns credit event name built from order data.
        /// </summary>
        /// <param name="orderObj">Order data</param>
        public static string GetCreditEventName(OrderInfo orderObj)
        {
            return ProviderObject.GetCreditEventNameInternal(orderObj);
        }


        /// <summary>
        /// Returns credit event description built from order data.
        /// </summary>
        /// <param name="orderObj">Order data</param>
        public static string GetCreditEventDescription(OrderInfo orderObj)
        {
            return ProviderObject.GetCreditEventDescriptionInternal(orderObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all credit events for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<CreditEventInfo> GetCreditEventsInternal(int siteId)
        {
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_CREDIT);

            return GetCreditEvents().OnSite(siteId);
        }


        /// <summary>
        /// Returns dataset of all customers' credit events for specified site.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<CreditEventInfo> GetCreditEventsInternal(int customerId, int siteId)
        {
            return GetCreditEvents(siteId).WhereEquals("EventCustomerID", customerId);
        }


        /// <summary>
        /// Deletes all customer's credit events.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual void DeleteCreditEventsInternal(int customerId, int siteId)
        {
            // Prepare where condition
            var where = new WhereCondition("EventCustomerID", QueryOperator.Equals, customerId);

            // Site condition
            var siteWhere = GetSiteWhereCondition(siteId);

            ProviderObject.BulkDelete(where.And(siteWhere));
        }


        /// <summary>
        /// Returns customer's total credit.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual decimal GetTotalCreditInternal(int customerId, int siteId)
        {
            return GetCreditEvents(customerId, siteId)
                                  .Column(new AggregatedColumn(AggregationType.Sum, "EventCreditChange"))
                                  .GetScalarResult(0.0m);
        }


        /// <summary>
        /// Returns credit event name built from order data.
        /// </summary>
        /// <param name="orderObj">Order data</param>
        protected virtual string GetCreditEventNameInternal(OrderInfo orderObj)
        {
            if (orderObj != null)
            {
                return String.Format(ResHelper.GetString("CreditPaymentFinished.CreditEventName"), orderObj.OrderID);
            }

            return "";
        }


        /// <summary>
        /// Returns credit event description built from order data.
        /// </summary>
        /// <param name="orderObj">Order data</param>
        protected virtual string GetCreditEventDescriptionInternal(OrderInfo orderObj)
        {
            return "";
        }


        private IWhereCondition GetSiteWhereCondition(int siteId)
        {
            var condition = new WhereCondition();

            // Allow global objects if global objects are requested
            var allowGlobal = siteId <= 0;

            // Allow global objects according to the site settings
            var siteName = SiteInfoProvider.GetSiteName(siteId);
            if (!string.IsNullOrEmpty(siteName))
            {
                allowGlobal = SettingsKeyInfoProvider.GetBoolValue(siteName + "." + ECommerceSettings.USE_GLOBAL_CREDIT);
            }

            var siteIdColumn = TypeInfo.SiteIDColumn;
            if (allowGlobal)
            {
                // Include global objects
                condition.WhereNull(siteIdColumn);
            }
            else if (siteId > 0)
            {
                // Only site objects
                condition.WhereEquals(siteIdColumn, siteId);
            }

            return condition;
        }

        #endregion
    }
}