using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Notifications.Web.UI;

[assembly: RegisterObjectType(typeof(NotificationSubscriptionInfo), NotificationSubscriptionInfo.OBJECT_TYPE)]

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Notification subscription info data container class.
    /// </summary>
    public class NotificationSubscriptionInfo : AbstractInfo<NotificationSubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "notification.subscription";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(NotificationSubscriptionInfoProvider), OBJECT_TYPE, "Notification.Subscription", "SubscriptionID", "SubscriptionLastModified", "SubscriptionGUID", null, null, null, "SubscriptionSiteID", "SubscriptionTemplateID", NotificationTemplateInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
            { 
                new ObjectDependency("SubscriptionGatewayID", NotificationGatewayInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("SubscriptionUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) 
            },
            MacroCollectionName = "CMS.NotificationSubscription",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Subscription site ID.
        /// </summary>
        public virtual int SubscriptionSiteID
        {
            get
            {
                return GetIntegerValue("SubscriptionSiteID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("SubscriptionSiteID", value);
                }
                else
                {
                    SetValue("SubscriptionSiteID", null);
                }
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to use HTML format of the template for the subscription.
        /// </summary>
        public virtual bool SubscriptionUseHTML
        {
            get
            {
                return GetBooleanValue("SubscriptionUseHTML", false);
            }
            set
            {
                SetValue("SubscriptionUseHTML", value);
            }
        }


        /// <summary>
        /// Event data field 1.
        /// </summary>
        public virtual string SubscriptionEventData1
        {
            get
            {
                return GetStringValue("SubscriptionEventData1", "");
            }
            set
            {
                SetValue("SubscriptionEventData1", value);
            }
        }


        /// <summary>
        /// Event data field 2.
        /// </summary>
        public virtual string SubscriptionEventData2
        {
            get
            {
                return GetStringValue("SubscriptionEventData2", "");
            }
            set
            {
                SetValue("SubscriptionEventData2", value);
            }
        }


        /// <summary>
        /// Subscription event source.
        /// </summary>
        public virtual string SubscriptionEventSource
        {
            get
            {
                return GetStringValue("SubscriptionEventSource", "");
            }
            set
            {
                SetValue("SubscriptionEventSource", value);
            }
        }


        /// <summary>
        /// Subscription last modified.
        /// </summary>
        public virtual DateTime SubscriptionLastModified
        {
            get
            {
                return GetDateTimeValue("SubscriptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionLastModified", value);
            }
        }


        /// <summary>
        /// Subscription target.
        /// </summary>
        public virtual string SubscriptionTarget
        {
            get
            {
                return GetStringValue("SubscriptionTarget", "");
            }
            set
            {
                SetValue("SubscriptionTarget", value);
            }
        }


        /// <summary>
        /// Subscription ID.
        /// </summary>
        public virtual int SubscriptionID
        {
            get
            {
                return GetIntegerValue("SubscriptionID", 0);
            }
            set
            {
                SetValue("SubscriptionID", value);
            }
        }


        /// <summary>
        /// Subscription event code.
        /// </summary>
        public virtual string SubscriptionEventCode
        {
            get
            {
                return GetStringValue("SubscriptionEventCode", "");
            }
            set
            {
                SetValue("SubscriptionEventCode", value);
            }
        }


        /// <summary>
        /// Subscription event display name.
        /// </summary>
        public virtual string SubscriptionEventDisplayName
        {
            get
            {
                return GetStringValue("SubscriptionEventDisplayName", "");
            }
            set
            {
                SetValue("SubscriptionEventDisplayName", value);
            }
        }


        /// <summary>
        /// Subscription template ID.
        /// </summary>
        public virtual int SubscriptionTemplateID
        {
            get
            {
                return GetIntegerValue("SubscriptionTemplateID", 0);
            }
            set
            {
                SetValue("SubscriptionTemplateID", value);
            }
        }


        /// <summary>
        /// Subscription event object ID.
        /// </summary>
        public virtual int SubscriptionEventObjectID
        {
            get
            {
                return GetIntegerValue("SubscriptionEventObjectID", 0);
            }
            set
            {
                SetValue("SubscriptionEventObjectID", value);
            }
        }


        /// <summary>
        /// Subscription user ID.
        /// </summary>
        public virtual int SubscriptionUserID
        {
            get
            {
                return GetIntegerValue("SubscriptionUserID", 0);
            }
            set
            {
                SetValue("SubscriptionUserID", value);
            }
        }


        /// <summary>
        /// Subscription GUID.
        /// </summary>
        public virtual Guid SubscriptionGUID
        {
            get
            {
                return GetGuidValue("SubscriptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("SubscriptionGUID", value);
            }
        }


        /// <summary>
        /// Subscription gateway ID.
        /// </summary>
        public virtual int SubscriptionGatewayID
        {
            get
            {
                return GetIntegerValue("SubscriptionGatewayID", 0);
            }
            set
            {
                SetValue("SubscriptionGatewayID", value);
            }
        }


        /// <summary>
        /// Subscription time.
        /// </summary>
        public virtual DateTime SubscriptionTime
        {
            get
            {
                return GetDateTimeValue("SubscriptionTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionTime", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            NotificationSubscriptionInfoProvider.DeleteNotificationSubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            NotificationSubscriptionInfoProvider.SetNotificationSubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty NotificationSubscriptionInfo object.
        /// </summary>
        public NotificationSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new NotificationSubscriptionInfo object from the given DataRow.
        /// </summary>
        public NotificationSubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}