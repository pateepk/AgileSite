using System;

using CMS.DataEngine;

namespace CMS.Notifications.Web.UI
{
    using TypedDataSet = InfoDataSet<NotificationGatewayInfo>;

    /// <summary>
    /// Class providing NotificationGatewayInfo management.
    /// </summary>
    public class NotificationGatewayInfoProvider : AbstractInfoProvider<NotificationGatewayInfo, NotificationGatewayInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// Determines if at least one notification gateway is enabled, if so, returns true, otherwise returns false.
        /// </summary>
        private static bool? mIsAnyNotificationGatewayEnabled;

        #endregion


        #region "Properties"

        /// <summary>
        /// Determines if at least one notification gateway is enabled, if so, returns true, otherwise returns false.
        /// </summary>
        public static bool IsAnyNotificationGatewayEnabled
        {
            get
            {
                if (mIsAnyNotificationGatewayEnabled == null)
                {
                    mIsAnyNotificationGatewayEnabled = GetNotificationGateways().WhereEquals("GatewayEnabled", 1).Columns("GatewayID").TopN(1).HasResults();
                }
                return mIsAnyNotificationGatewayEnabled.Value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationGatewayInfoProvider()
            : base(NotificationGatewayInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="NotificationGatewayInfo"/> objects.
        /// </summary>
        public static ObjectQuery<NotificationGatewayInfo> GetNotificationGateways()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the NotificationGatewayInfo structure for the specified notificationGateway.
        /// </summary>
        /// <param name="notificationGatewayId">NotificationGateway ID</param>
        public static NotificationGatewayInfo GetNotificationGatewayInfo(int notificationGatewayId)
        {
            return ProviderObject.GetInfoById(notificationGatewayId);
        }


        /// <summary>
        /// Returns the NotificationGatewayInfo structure for the specified notificationGateway.
        /// </summary>
        /// <param name="gatewayName">Gateway codename</param>        
        public static NotificationGatewayInfo GetNotificationGatewayInfo(string gatewayName)
        {
            return ProviderObject.GetInfoByCodeName(gatewayName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified notificationGateway.
        /// </summary>
        /// <param name="notificationGateway">NotificationGateway to set</param>
        public static void SetNotificationGatewayInfo(NotificationGatewayInfo notificationGateway)
        {
            ProviderObject.SetInfo(notificationGateway);
        }


        /// <summary>
        /// Deletes specified notificationGateway.
        /// </summary>
        /// <param name="infoObj">NotificationGateway object</param>
        public static void DeleteNotificationGatewayInfo(NotificationGatewayInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified notificationGateway.
        /// </summary>
        /// <param name="notificationGatewayId">NotificationGateway ID</param>
        public static void DeleteNotificationGatewayInfo(int notificationGatewayId)
        {
            NotificationGatewayInfo infoObj = GetNotificationGatewayInfo(notificationGatewayId);
            DeleteNotificationGatewayInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(NotificationGatewayInfo info)
        {
            base.SetInfo(info);

            if ((info != null) && (info.GatewayID > 0))
            {
                // Force reloading hashtable for notification sender
                NotificationSender.ClearHashtables();
            }

            mIsAnyNotificationGatewayEnabled = null;
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(NotificationGatewayInfo info)
        {
            if ((info != null) && info.GatewayEnabled)
            {
                mIsAnyNotificationGatewayEnabled = null;
            }

            base.DeleteInfo(info);

            // Force reloading hashtable for notification sender
            NotificationSender.ClearHashtables();
        }

        #endregion
    }
}