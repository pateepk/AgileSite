using System;
using System.Collections.Generic;
using System.Security.Principal;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Base;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base class for all notification gateways.
    /// </summary>
    public abstract class CMSNotificationGateway
    {
        #region "Private fields"

        private CMSNotificationGatewayForm mNotificationGatewayForm;

        private NotificationGatewayInfo mNotificationGatewayObj;
        private string mGatewayName;

        /// <summary>
        /// Indicates if notifications are sent when specified event occurs.
        /// </summary>
        private static bool mEnableNotifications = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if notifications are sent when specified event occurs, by default it is set to TRUE.
        /// </summary>
        public static bool EnableNotifications
        {
            get
            {
                return mEnableNotifications;
            }
            set
            {
                mEnableNotifications = value;
            }
        }


        /// <summary>
        /// Macro resolver for resolving macros in notification messages.
        /// </summary>
        public MacroResolver Resolver
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the code name of the notification gateway.
        /// </summary>
        public string GatewayName
        {
            get
            {
                return mGatewayName;
            }
            set
            {
                mGatewayName = value;
                mNotificationGatewayObj = null;
            }
        }


        /// <summary>
        /// Gets the notification gateway object.
        /// </summary>
        public NotificationGatewayInfo NotificationGatewayObj
        {
            get
            {
                return mNotificationGatewayObj ?? (mNotificationGatewayObj = NotificationGatewayInfoProvider.GetNotificationGatewayInfo(mGatewayName));
            }
        }


        /// <summary>
        /// Subscription information the gateway use when sending notification.
        /// </summary>
        public NotificationSubscriptionInfo NotificationSubscriptionObj
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets notification subscription control where the notification gateway form is placed on.
        /// </summary>
        public CMSNotificationSubscription NotificationSubscriptionControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the notification gateway form.
        /// </summary>
        public CMSNotificationGatewayForm NotificationGatewayForm
        {
            get
            {
                return mNotificationGatewayForm ?? (mNotificationGatewayForm = GetNotificationGatewayForm());
            }
            set
            {
                mNotificationGatewayForm = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets notification gateway according to the specified NotificationGatewayInfo.
        /// </summary>
        private static CMSNotificationGateway GetNotificationGateway(NotificationGatewayInfo gatewayObj)
        {
            if ((gatewayObj != null) && (gatewayObj.GatewayAssemblyName != "") && (gatewayObj.GatewayClassName != ""))
            {
                try
                {
                    // Get notification gateway provider instance
                    CMSNotificationGateway gateway = ClassHelper.GetClass<CMSNotificationGateway>(gatewayObj.GatewayAssemblyName, gatewayObj.GatewayClassName);
                    if (gateway != null)
                    {
                        gateway.GatewayName = gatewayObj.GatewayName;
                        return gateway;
                    }
                }
                catch (Exception ex)
                {
                    LogEvent(ex);
                }
            }
            return null;
        }


        /// <summary>
        /// Gets notification gateway assembly according to the specified gateway ID.
        /// </summary>
        /// <param name="gatewayId">ID of the gateway to obtain</param>        
        public static CMSNotificationGateway GetNotificationGateway(int gatewayId)
        {
            if (gatewayId > 0)
            {
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo(gatewayId);
                return GetNotificationGateway(gateway);
            }
            return null;
        }


        /// <summary>
        /// Gets notification gateway assembly according to the specified gateway name.
        /// </summary>
        /// <param name="gatewayName">Name of the gateway to obtain</param>        
        public static CMSNotificationGateway GetNotificationGateway(string gatewayName)
        {
            if (!string.IsNullOrEmpty(gatewayName))
            {
                // Get gateway according specified name
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo(gatewayName);
                if (gateway != null)
                {
                    return GetNotificationGateway(gateway);
                }
            }
            return null;
        }


        /// <summary>
        /// Raises event with specified parameters and performs sending notifications to all subscribers who were subscribed to such event.
        /// </summary>
        /// <param name="eventSource">Subscription event source</param>
        /// <param name="eventCode">Subscription event code</param>
        /// <param name="eventObjectId">Subscription event object ID</param>
        /// <param name="eventData1">Subscription event data 1</param>
        /// <param name="eventData2">Subscription event data 2</param>
        /// <param name="siteId">ID of the site where the event belongs</param>
        /// <param name="where">Additional WHERE condition</param>
        /// <param name="resolverData">Custom data for macro resolver (DataRow or DataClass object)</param>
        /// <param name="resolverSourceParameters">Resolver special macros</param>
        public static void RaiseEvent(string eventSource, string eventCode, int eventObjectId, string eventData1, string eventData2, int siteId, string where, object resolverData, IDictionary<string, object> resolverSourceParameters)
        {
            // Raise event only when notifications are allowed to be sent
            if (EnableNotifications)
            {
                // Check license for notifications
                if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
                {
                    LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Notifications);
                }

                NotificationSender sender = new NotificationSender();
                sender.EventCode = eventCode;
                sender.EventData1 = eventData1;
                sender.EventData2 = eventData2;
                sender.EventObjectID = eventObjectId;
                sender.EventSource = eventSource;
                sender.SiteID = siteId;
                sender.Where = where;
                sender.ResolverCustomData = resolverData;
                sender.ResolverSourceData = resolverSourceParameters;
                sender.SendAsync(WindowsIdentity.GetCurrent());
            }
        }


        /// <summary>
        /// Returns notification gateway form with custom control(s) which are used to enter/select user target(s) the provider should send notification to. You will need to override this method for your custom gateway.
        /// </summary>        
        public virtual CMSNotificationGatewayForm GetNotificationGatewayForm()
        {
            return null;
        }


        /// <summary>
        /// Sends notification. It is automatically called after the specified event is raised. You will need to override this method for your custom gateway.
        /// </summary>        
        public virtual void SendNotification()
        {
        }


        /// <summary>
        /// Returns clone of current notification gateway instance.
        /// </summary>
        public virtual CMSNotificationGateway Clone()
        {
            CMSNotificationGateway clone = (CMSNotificationGateway)Activator.CreateInstance(GetType());

            clone.GatewayName = GatewayName;
            clone.NotificationGatewayForm = NotificationGatewayForm;
            clone.NotificationSubscriptionControl = NotificationSubscriptionControl;
            clone.NotificationSubscriptionObj = NotificationSubscriptionObj;
            clone.Resolver = Resolver;

            return clone;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Logs an event if error occurred during action.
        /// </summary>
        /// <param name="ex">Exception occuring</param>
        private static void LogEvent(Exception ex)
        {
            try
            {
                // Log the event
                EventLogProvider.LogEvent(EventType.ERROR, "CMSNotificationGateway", "EXCEPTION", EventLogProvider.GetExceptionLogMessage(ex));
            }
            catch
            {
                // Unable to log the event
            }
        }

        #endregion
    }
}