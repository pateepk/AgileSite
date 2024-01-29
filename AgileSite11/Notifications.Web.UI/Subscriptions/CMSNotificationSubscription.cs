using System;
using System.Collections.Generic;

using CMS.UIControls;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base class for the subscription controls.
    /// </summary>
    public class CMSNotificationSubscription : CMSUserControl
    {
        #region "Variables"

        private string mGatewayNames;
        private List<CMSNotificationGateway> mNotificationGateways;

        #endregion


        #region "Properties"

        /// <summary>
        /// Array of SubscriptionInfos (used when multple subscriptions should be created).
        /// </summary>
        public NotificationSubscriptionInfo[] Subscriptions
        {
            get;
            set;
        }


        /// <summary>
        /// If specified, user is subscribed to site specific event. If zero, user is subscribed to global event.
        /// </summary>
        public int SubscriptionSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value which determines whether to use HTML format of the template for the subscription.
        /// </summary>
        public bool SubscriptionUseHTML
        {
            get;
            set;
        }


        /// <summary>
        /// Event data field 1.
        /// </summary>
        public string EventData1
        {
            get;
            set;
        }


        /// <summary>
        /// Event data field 2.
        /// </summary>
        public string EventData2
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the text which will be displayed above the notification gateway forms.
        /// </summary>
        public string EventDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the code names of the notification gateways separated with semicolon.
        /// </summary>
        public string GatewayNames
        {
            get
            {
                return mGatewayNames;
            }
            set
            {
                mGatewayNames = value;

                // Force to reload the notification gateways
                mNotificationGateways = null;
            }
        }


        /// <summary>
        /// Gets or sets the notification template name in following format:
        /// [sitename].[templatename] for site specific template, [templatename] for global template.
        /// </summary>
        public string NotificationTemplateName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the event source.
        /// </summary>
        public string EventSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the event code.
        /// </summary>
        public string EventCode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the event object ID.
        /// </summary>
        public int EventObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets localizable string or plain text which describes event and which is visible to the users.
        /// </summary>
        public string EventDisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the Event user ID.
        /// </summary>
        public int EventUserID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the list of the notification gateway providers, which will be loaded according to the GatewayNames.
        /// </summary>
        public List<CMSNotificationGateway> NotificationGateways
        {
            get
            {
                // Dynamically load the notification gateways if not loaded yet
                if (mNotificationGateways == null)
                {
                    mNotificationGateways = new List<CMSNotificationGateway>();
                    if (!String.IsNullOrEmpty(GatewayNames))
                    {
                        string[] gateways = GatewayNames.Split(';');
                        foreach (string g in gateways)
                        {
                            CMSNotificationGateway gw = CMSNotificationGateway.GetNotificationGateway(g);
                            if ((gw != null) && (gw.NotificationGatewayObj != null) && (gw.NotificationGatewayObj.GatewayEnabled))
                            {
                                gw.NotificationSubscriptionControl = this;
                                mNotificationGateways.Add(gw);
                            }
                        }
                    }
                }
                return mNotificationGateways;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// For each notification gateway provider validates its notification form data.
        ///	If validation fails returns concatenated error messages from each form otherwise returns empty string.
        /// </summary>
        public virtual string Validate()
        {
            return String.Empty;
        }


        /// <summary>
        /// Calls Validate() method, if validation fails returns error message, 
        /// otherwise creates subscriptions and returns empty string.
        /// </summary>
        public virtual string Subscribe()
        {
            return String.Empty;
        }

        #endregion
    }
}