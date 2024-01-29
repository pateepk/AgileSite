using System;
using System.Collections.Generic;
using System.Security.Principal;

using CMS.EventLog;
using CMS.Membership;
using CMS.Base;
using CMS.MacroEngine;
using CMS.DataEngine;

namespace CMS.Notifications.Web.UI
{
    using GatewayTable = SafeDictionary<int, CMSNotificationGateway>;

    /// <summary>
    /// Object for asynchronous notification sending.
    /// </summary>
    public class NotificationSender
    {
        #region "Variables"

        private static WindowsIdentity mWindowsIdentity;

        /// <summary>
        /// Hashtable with CMSNotificationGateways indexed by GatewayID.
        /// </summary>
        private static readonly CMSStatic<GatewayTable> mGatewaysById = new CMSStatic<GatewayTable>(() => new GatewayTable());

        #endregion


        #region "Properties"

        /// <summary>
        /// Hashtable with CMSNotificationGateways indexed by GatewayID.
        /// </summary>
        private static GatewayTable GatewaysById
        {
            get
            {
                return mGatewaysById;
            }
        }


        /// <summary>
        /// Subscription event source.
        /// </summary>
        public string EventSource
        {
            get;
            set;
        }


        /// <summary>
        /// Subscription event code.
        /// </summary>
        public string EventCode
        {
            get;
            set;
        }


        /// <summary>
        /// Subscription event object ID.
        /// </summary>
        public int EventObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Subscription event data 1.
        /// </summary>
        public string EventData1
        {
            get;
            set;
        }


        /// <summary>
        /// Subscription event data 2.
        /// </summary>
        public string EventData2
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the site where the event belongs.
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Additional WHERE condition.
        /// </summary>
        public string Where
        {
            get;
            set;
        }


        /// <summary>
        /// Custom data for the macro resolver (IInfo, DataRow or DataClass object).
        /// </summary>
        public object ResolverCustomData
        {
            get;
            set;
        }


        /// <summary>
        /// Special macros for the resolver.
        /// </summary>
        public IDictionary<string, object> ResolverSourceData
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets notification gateway according to the specified NotificationGatewayInfo.
        /// </summary>
        private static CMSNotificationGateway GetNotificationGateway(int gatewayId)
        {
            // Try to get gateway from hashtable
            CMSNotificationGateway gateway = GatewaysById[gatewayId];

            // If gateway was not found try to load its assembly
            if (gateway == null)
            {
                gateway = CMSNotificationGateway.GetNotificationGateway(gatewayId);
                GatewaysById[gatewayId] = gateway;
            }

            // Always return clone of notification gateway
            return (gateway != null ? gateway.Clone() : null);
        }


        /// <summary>
        /// Sends all notifications matching the given criteria (properties of the object).
        /// </summary>
        private void SendNotifications()
        {
            if (NotificationGatewayInfoProvider.IsAnyNotificationGatewayEnabled)
            {
                // Generate proper where condition and get the data
                var completeWhere = NotificationSubscriptionInfoProvider.GetWhereConditionObject(EventSource, EventCode, EventObjectID, EventData1, EventData2, SiteID, new WhereCondition(Where));
                var subscriptions = NotificationSubscriptionInfoProvider.GetNotificationSubscriptions()
                    .Where(completeWhere);

                if (subscriptions.HasResults())
                {
                    foreach (NotificationSubscriptionInfo nsi in subscriptions.TypedResult)
                    {
                        // Try to get gateway assembly
                        CMSNotificationGateway gateway = GetNotificationGateway(nsi.SubscriptionGatewayID);
                        if ((gateway != null) && (gateway.NotificationGatewayObj != null) && (gateway.NotificationGatewayObj.GatewayEnabled))
                        {
                            // Initialize resolver with default data
                            gateway.Resolver = MacroResolver.GetInstance();
                            gateway.Resolver.SetNamedSourceData("notificationsubscription", nsi);
                            gateway.Resolver.SetNamedSourceData("notificationgateway", gateway.NotificationGatewayObj);
                            UserInfo ui = UserInfoProvider.GetUserInfo(nsi.SubscriptionUserID);
                            if (ui != null)
                            {
                                gateway.Resolver.SetNamedSourceData("notificationuser", ui);
                            }

                            // Initialize resolver with custom data
                            gateway.Resolver.SetNamedSourceData("notificationcustomdata", ResolverCustomData);
                            gateway.Resolver.SetNamedSourceData(ResolverSourceData, isPrioritized: false);

                            // Send notifications
                            gateway.NotificationSubscriptionObj = nsi;
                            gateway.SendNotification();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Runs the sender in an asynchronous thread.
        /// </summary>
        /// <param name="wi">Windows identity</param>
        public void SendAsync(WindowsIdentity wi)
        {
            // Store windows identity
            mWindowsIdentity = wi;

            CMSThread asyncEmailSend = new CMSThread(Send);
            asyncEmailSend.Start();
        }


        /// <summary>
        /// Sends the notifications.
        /// </summary>
        public void Send()
        {
            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                // Send the notifications
                SendNotifications();
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Notifications", "ERROR", ex);
            }
            finally
            {
                // Undo impersonation
                ctx.Undo();
            }
        }


        /// <summary>
        /// Clears hashtables with notification gateways' assemblies.
        /// </summary>
        public static void ClearHashtables()
        {
            GatewaysById.Clear();
        }

        #endregion
    }
}