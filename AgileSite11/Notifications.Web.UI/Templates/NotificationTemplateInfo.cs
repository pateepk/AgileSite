using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.Notifications.Web.UI;

[assembly: RegisterObjectType(typeof(NotificationTemplateInfo), NotificationTemplateInfo.OBJECT_TYPE)]

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Notification template info data container class.
    /// </summary>
    public class NotificationTemplateInfo : AbstractInfo<NotificationTemplateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NOTIFICATIONTEMPLATE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(NotificationTemplateInfoProvider), OBJECT_TYPE, "Notification.Template", "TemplateID", "TemplateLastModified", "TemplateGUID", "TemplateName", "TemplateDisplayName", null, "TemplateSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            MacroCollectionName = "CMS.NotificationTemplate",
            AllowRestore = false,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SupportsGlobalObjects = true,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Template last modified.
        /// </summary>
        public virtual DateTime TemplateLastModified
        {
            get
            {
                return GetDateTimeValue("TemplateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TemplateLastModified", value);
            }
        }


        /// <summary>
        /// Template ID.
        /// </summary>
        public virtual int TemplateID
        {
            get
            {
                return GetIntegerValue("TemplateID", 0);
            }
            set
            {
                SetValue("TemplateID", value);
            }
        }


        /// <summary>
        /// Template GUID.
        /// </summary>
        public virtual Guid TemplateGUID
        {
            get
            {
                return GetGuidValue("TemplateGUID", Guid.Empty);
            }
            set
            {
                SetValue("TemplateGUID", value);
            }
        }


        /// <summary>
        /// Template name.
        /// </summary>
        public virtual string TemplateName
        {
            get
            {
                return GetStringValue("TemplateName", "");
            }
            set
            {
                SetValue("TemplateName", value);
            }
        }


        /// <summary>
        /// Template display name.
        /// </summary>
        public virtual string TemplateDisplayName
        {
            get
            {
                return GetStringValue("TemplateDisplayName", "");
            }
            set
            {
                SetValue("TemplateDisplayName", value);
            }
        }


        /// <summary>
        /// Template site ID.
        /// </summary>
        public virtual int TemplateSiteID
        {
            get
            {
                return GetIntegerValue("TemplateSiteID", 0);
            }
            set
            {
                SetValue("TemplateSiteID", value, 0);
            }
        }


        /// <summary>
        /// Gets the template full name.
        /// </summary>
        public string TemplateFullName
        {
            get
            {
                if (TemplateSiteID > 0)
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(TemplateSiteID);
                    if (si != null)
                    {
                        return si.SiteName + "." + TemplateName;
                    }
                }

                return TemplateName;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            NotificationTemplateInfoProvider.DeleteNotificationTemplateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            NotificationTemplateInfoProvider.SetNotificationTemplateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty NotificationTemplateInfo object.
        /// </summary>
        public NotificationTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new NotificationTemplateInfo object from the given DataRow.
        /// </summary>
        public NotificationTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}