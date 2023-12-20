using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(PageTemplateConfigurationInfo), PageTemplateConfigurationInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Data container class for <see cref="PageTemplateConfigurationInfo"/>.
    /// Represents custom page template configuration for MVC sites.
    /// </summary>
    [Serializable]
    public class PageTemplateConfigurationInfo : AbstractInfo<PageTemplateConfigurationInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.pagetemplateconfiguration";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateConfigurationInfoProvider), OBJECT_TYPE, "CMS.PageTemplateConfiguration", "PageTemplateConfigurationID", "PageTemplateConfigurationLastModified", "PageTemplateConfigurationGUID", null, "PageTemplateConfigurationName", null, "PageTemplateConfigurationSiteID", null, null)
        {
            TouchCacheDependencies = true,
            LogEvents = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.CONTENT,
            ThumbnailGUIDColumn = "PageTemplateConfigurationThumbnailGUID",
            HasMetaFiles = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                }
            }
        };


        /// <summary>
        /// Page template configuration ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateConfigurationID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageTemplateConfigurationID"), 0);
            }
            set
            {
                SetValue("PageTemplateConfigurationID", value);
            }
        }


        /// <summary>
        /// Page template configuration GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageTemplateConfigurationGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("PageTemplateConfigurationGUID"), Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationGUID", value);
            }
        }


        /// <summary>
        /// Page template configuration site ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateConfigurationSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageTemplateConfigurationSiteID"), 0);
            }
            set
            {
                SetValue("PageTemplateConfigurationSiteID", value);
            }
        }


        /// <summary>
        /// Page template configuration last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageTemplateConfigurationLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("PageTemplateConfigurationLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageTemplateConfigurationLastModified", value);
            }
        }


        /// <summary>
        /// Page template configuration name.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateConfigurationName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageTemplateConfigurationName"), String.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationName", value);
            }
        }


        /// <summary>
        /// Page template configuration description.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateConfigurationDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageTemplateConfigurationDescription"), String.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// Page template configuration thumbnail GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageTemplateConfigurationThumbnailGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("PageTemplateConfigurationThumbnailGUID"), Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Page template configuration template configuration.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateConfigurationTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageTemplateConfigurationTemplate"), String.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationTemplate", value);
            }
        }


        /// <summary>
        /// Page template configuration widgets configuration.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateConfigurationWidgets
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageTemplateConfigurationWidgets"), String.Empty);
            }
            set
            {
                SetValue("PageTemplateConfigurationWidgets", value, String.Empty);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageTemplateConfigurationInfoProvider.DeletePageTemplateConfigurationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageTemplateConfigurationInfoProvider.SetPageTemplateConfigurationInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected PageTemplateConfigurationInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="PageTemplateConfigurationInfo"/> class.
        /// </summary>
        public PageTemplateConfigurationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="PageTemplateConfigurationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public PageTemplateConfigurationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                case PermissionsEnum.Modify:
                    return userInfo.IsAuthorizedPerResource(ModuleName.CONTENT, "ManagePageTemplates", siteName, exceptionOnFailure);
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }
    }
}