using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Notifications.Web.UI;

[assembly: RegisterObjectType(typeof(NotificationGatewayInfo), NotificationGatewayInfo.OBJECT_TYPE)]

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// NotificationGatewayInfo data container class.
    /// </summary>
    public class NotificationGatewayInfo : AbstractInfo<NotificationGatewayInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "notification.gateway";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(NotificationGatewayInfoProvider), OBJECT_TYPE, "Notification.Gateway", "GatewayID", "GatewayLastModified", "GatewayGUID", "GatewayName", "GatewayDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            MacroCollectionName = "CMS.NotificationGateway",
            AssemblyNameColumn = "GatewayAssemblyName",
            EnabledColumn = "GatewayEnabled",
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gateway supports email.
        /// </summary>
        public virtual bool GatewaySupportsEmail
        {
            get
            {
                return GetBooleanValue("GatewaySupportsEmail", false);
            }
            set
            {
                SetValue("GatewaySupportsEmail", value);
            }
        }


        /// <summary>
        /// Gateway class name.
        /// </summary>
        public virtual string GatewayClassName
        {
            get
            {
                return GetStringValue("GatewayClassName", "");
            }
            set
            {
                SetValue("GatewayClassName", value);
            }
        }


        /// <summary>
        /// Gateway name.
        /// </summary>
        public virtual string GatewayName
        {
            get
            {
                return GetStringValue("GatewayName", "");
            }
            set
            {
                SetValue("GatewayName", value);
            }
        }


        /// <summary>
        /// Gateway supports plain text.
        /// </summary>
        public virtual bool GatewaySupportsPlainText
        {
            get
            {
                return GetBooleanValue("GatewaySupportsPlainText", false);
            }
            set
            {
                SetValue("GatewaySupportsPlainText", value);
            }
        }


        /// <summary>
        /// Gateway supports HTML text.
        /// </summary>
        public virtual bool GatewaySupportsHTMLText
        {
            get
            {
                return GetBooleanValue("GatewaySupportsHTMLText", false);
            }
            set
            {
                SetValue("GatewaySupportsHTMLText", value);
            }
        }


        /// <summary>
        /// Gateway display name.
        /// </summary>
        public virtual string GatewayDisplayName
        {
            get
            {
                return GetStringValue("GatewayDisplayName", "");
            }
            set
            {
                SetValue("GatewayDisplayName", value);
            }
        }


        /// <summary>
        /// Gateway description.
        /// </summary>
        public virtual string GatewayDescription
        {
            get
            {
                return GetStringValue("GatewayDescription", "");
            }
            set
            {
                SetValue("GatewayDescription", value);
            }
        }


        /// <summary>
        /// Indicates if gateway is enabled or not.
        /// </summary>
        public virtual bool GatewayEnabled
        {
            get
            {
                return GetBooleanValue("GatewayEnabled", false);
            }
            set
            {
                SetValue("GatewayEnabled", value);
            }
        }


        /// <summary>
        /// Gateway id.
        /// </summary>
        public virtual int GatewayID
        {
            get
            {
                return GetIntegerValue("GatewayID", 0);
            }
            set
            {
                SetValue("GatewayID", value);
            }
        }


        /// <summary>
        /// Gateway last modified.
        /// </summary>
        public virtual DateTime GatewayLastModified
        {
            get
            {
                return GetDateTimeValue("GatewayLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GatewayLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gateway GUID.
        /// </summary>
        public virtual Guid GatewayGUID
        {
            get
            {
                return GetGuidValue("GatewayGUID", Guid.Empty);
            }
            set
            {
                SetValue("GatewayGUID", value);
            }
        }


        /// <summary>
        /// Gateway assembly name.
        /// </summary>
        public virtual string GatewayAssemblyName
        {
            get
            {
                return GetStringValue("GatewayAssemblyName", "");
            }
            set
            {
                SetValue("GatewayAssemblyName", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            NotificationGatewayInfoProvider.DeleteNotificationGatewayInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            NotificationGatewayInfoProvider.SetNotificationGatewayInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty NotificationGatewayInfo object.
        /// </summary>
        public NotificationGatewayInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new NotificationGatewayInfo object from the given DataRow.
        /// </summary>
        public NotificationGatewayInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}