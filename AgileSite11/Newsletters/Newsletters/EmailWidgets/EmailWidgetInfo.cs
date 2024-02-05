using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(EmailWidgetInfo), EmailWidgetInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// EmailWidgetInfo data container class.
    /// </summary>
	[Serializable]
    public class EmailWidgetInfo : AbstractInfo<EmailWidgetInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.emailwidget";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailWidgetInfoProvider), OBJECT_TYPE, "Newsletter.EmailWidget", "EmailWidgetID", "EmailWidgetLastModified", "EmailWidgetGuid", "EmailWidgetName", "EmailWidgetDisplayName", null, "EmailWidgetSiteID", null, null)
        {
            TouchCacheDependencies = true,
            LogEvents = true,
            SupportsVersioning = true,
            ModuleName = ModuleName.NEWSLETTER,
            FormDefinitionColumn = "EmailWidgetProperties",
            ThumbnailGUIDColumn = "EmailWidgetThumbnailGUID",
            HasMetaFiles = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("EmailWidgetProperties")
                }
            },
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Email widget ID.
        /// </summary>
		[DatabaseField]
        public virtual int EmailWidgetID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailWidgetID"), 0);
            }
            set
            {
                SetValue("EmailWidgetID", value);
            }
        }


        /// <summary>
        /// Email widget GUID.
        /// </summary>
		[DatabaseField]
        public virtual Guid EmailWidgetGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("EmailWidgetGuid"), Guid.Empty);
            }
            set
            {
                SetValue("EmailWidgetGuid", value);
            }
        }


        /// <summary>
        /// Email widget last modified timestamp.
        /// </summary>
		[DatabaseField]
        public virtual DateTime EmailWidgetLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("EmailWidgetLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailWidgetLastModified", value);
            }
        }


        /// <summary>
        /// Email widget site ID.
        /// </summary>
		[DatabaseField]
        public virtual int EmailWidgetSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailWidgetSiteID"), 0);
            }
            set
            {
                SetValue("EmailWidgetSiteID", value);
            }
        }


        /// <summary>
        /// Email widget display name.
        /// </summary>
		[DatabaseField]
        public virtual string EmailWidgetDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailWidgetDisplayName"), String.Empty);
            }
            set
            {
                SetValue("EmailWidgetDisplayName", value);
            }
        }


        /// <summary>
        /// Email widget code name.
        /// </summary>
		[DatabaseField]
        public virtual string EmailWidgetName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailWidgetName"), String.Empty);
            }
            set
            {
                SetValue("EmailWidgetName", value);
            }
        }


        /// <summary>
        /// Email widget description.
        /// </summary>
		[DatabaseField]
        public virtual string EmailWidgetDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailWidgetDescription"), String.Empty);
            }
            set
            {
                SetValue("EmailWidgetDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// Email widget code.
        /// </summary>
		[DatabaseField]
        public virtual string EmailWidgetCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailWidgetCode"), String.Empty);
            }
            set
            {
                SetValue("EmailWidgetCode", value, String.Empty);
            }
        }


        /// <summary>
        /// Email widget properties.
        /// </summary>
		[DatabaseField]
        public virtual string EmailWidgetProperties
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailWidgetProperties"), String.Empty);
            }
            set
            {
                SetValue("EmailWidgetProperties", value, String.Empty);
            }
        }


        /// <summary>
        /// Email widget thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid EmailWidgetThumbnailGUID
        {
            get
            {
                return GetGuidValue("EmailWidgetThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("EmailWidgetThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Email widget icon CSS class defining the web part thumbnail.
        /// </summary>
        [DatabaseField]
        public virtual string EmailWidgetIconCssClass
        {
            get
            {
                return GetStringValue("EmailWidgetIconCssClass", null);
            }
            set
            {
                SetValue("EmailWidgetIconCssClass", value, string.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailWidgetInfoProvider.DeleteEmailWidgetInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            // Do not store empty IconCssClass, use NULL value
            if (String.IsNullOrEmpty(EmailWidgetIconCssClass))
            {
                EmailWidgetIconCssClass = null;
            }

            EmailWidgetInfoProvider.SetEmailWidgetInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected EmailWidgetInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="EmailWidgetInfo"/> class.
        /// </summary>
        public EmailWidgetInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="EmailWidgetInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public EmailWidgetInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type.</param>
        /// <returns>ManageTemplates permission name for managing permission type, or base permission name otherwise.</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                    return "ManageTemplates";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}