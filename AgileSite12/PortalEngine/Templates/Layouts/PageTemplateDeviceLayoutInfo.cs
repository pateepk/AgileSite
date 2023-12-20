using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(PageTemplateDeviceLayoutInfo), PageTemplateDeviceLayoutInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page template device layout info data container class.
    /// </summary>
    public class PageTemplateDeviceLayoutInfo : AbstractInfo<PageTemplateDeviceLayoutInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.templatedevicelayout";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateDeviceLayoutInfoProvider), OBJECT_TYPE, "CMS.TemplateDeviceLayout", "TemplateDeviceLayoutID", "LayoutLastModified", "LayoutGUID", null, null, null, null, "PageTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DESIGN,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ProfileID", DeviceProfileInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("LayoutID", LayoutInfo.OBJECT_TYPE)
            },
            IsBinding = true,
            SupportsLocking = true,
            SupportsVersioning = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "LayoutVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CSSColumn = EXTERNAL_COLUMN_CSS,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables

        /// <summary>
        ///  External column name for Layout Code
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "LayoutCode";

        /// <summary>
        ///  External column name for Layout CSS
        /// </summary>
        public const string EXTERNAL_COLUMN_CSS = "LayoutCSS";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the last modified value
        /// </summary>
        [DatabaseField]
        public DateTime LayoutLastModified
        {
            get
            {
                return GetDateTimeValue("LayoutLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LayoutLastModified", value);
            }
        }


        /// <summary>
        /// Gets the full name in format templateid.deviceid
        /// </summary>
        public string LayoutFullName
        {
            get
            {
                return ObjectHelper.BuildFullName(PageTemplateID.ToString(), ProfileID.ToString());
            }
        }


        /// <summary>
        /// Gets the full name in format sitename.devicename.pagetemplatename
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return LayoutFullName;
            }
        }


        /// <summary>
        /// Layout version GUID.
        /// </summary>
        [DatabaseField]
        public string LayoutVersionGUID
        {
            get
            {
                return GetStringValue("LayoutVersionGUID", null);
            }
            set
            {
                SetValue("LayoutVersionGUID", value);
            }
        }


        /// <summary>
        /// Template device layout id.
        /// </summary>
        [DatabaseField]
        public virtual int TemplateDeviceLayoutID
        {
            get
            {
                return GetIntegerValue("TemplateDeviceLayoutID", 0);
            }
            set
            {
                SetValue("TemplateDeviceLayoutID", value);
            }
        }


        /// <summary>
        /// Page template id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateID
        {
            get
            {
                return GetIntegerValue("PageTemplateID", 0);
            }
            set
            {
                SetValue("PageTemplateID", value, 0);
            }
        }


        /// <summary>
        /// Device profile id.
        /// </summary>
        [DatabaseField]
        public virtual int ProfileID
        {
            get
            {
                return GetIntegerValue("ProfileID", 0);
            }
            set
            {
                SetValue("ProfileID", value, 0);
            }
        }


        /// <summary>
        /// Layout id.
        /// </summary>
        [DatabaseField]
        public virtual int LayoutID
        {
            get
            {
                return GetIntegerValue("LayoutID", 0);
            }
            set
            {
                SetValue("LayoutID", value, 0);
            }
        }


        /// <summary>
        /// Layout code.
        /// </summary>
        [DatabaseField]
        public virtual string LayoutCode
        {
            get
            {
                return GetStringValue("LayoutCode", string.Empty);
            }
            set
            {
                SetValue("LayoutCode", value, string.Empty);
            }
        }


        /// <summary>
        /// Layout type.
        /// </summary>
        [DatabaseField]
        public virtual LayoutTypeEnum LayoutType
        {
            get
            {
                return LayoutInfoProvider.GetLayoutTypeEnum(GetStringValue("LayoutType", string.Empty));
            }
            set
            {
                SetValue("LayoutType", LayoutInfoProvider.GetLayoutTypeCode(value));
            }
        }


        /// <summary>
        /// Layout CSS.
        /// </summary>
        [DatabaseField]
        public virtual string LayoutCSS
        {
            get
            {
                return GetStringValue("LayoutCSS", string.Empty);
            }
            set
            {
                SetValue("LayoutCSS", value, string.Empty);
            }
        }


        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public bool UsesExternalStorage
        {
            get
            {
                return StorageHelper.IsExternalStorage(GetThemePath());
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageTemplateDeviceLayoutInfoProvider.DeleteTemplateDeviceLayoutInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageTemplateDeviceLayoutInfoProvider.SetTemplateDeviceLayoutInfo(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            string extension = ".ascx";
            if (EXTERNAL_COLUMN_CSS.EqualsCSafe(externalColumnName, true))
            {
                extension = ".css";
            }


            // Keep original version GUID
            string originalVersionGuid = versionGuid;

            // Do not use version GUID for files stored externally
            if (PageTemplateInfoProvider.StorePageTemplatesInExternalStorage || SettingsKeyInfoProvider.DeploymentMode)
            {
                versionGuid = null;
            }

            string path = String.Empty;

            // Get bindings
            PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(PageTemplateID);
            if (pti != null)
            {
                DeviceProfileInfo dpi = DeviceProfileInfoProvider.GetDeviceProfileInfo(ProfileID);
                if (dpi != null)
                {
                    // Get specific directory and site name for ad-hoc templates
                    string directory = PageTemplateDeviceLayoutInfoProvider.DeviceLayoutsDirectory;
                    string prefix = dpi.ProfileName;
                    if (!pti.IsReusable)
                    {
                        directory = PageTemplateDeviceLayoutInfoProvider.AdHocDeviceLayoutsDirectory;
                        prefix = SiteInfoProvider.GetSiteName(pti.PageTemplateSiteID) + "/" + prefix + "/" + pti.CodeName.Substring(0, 2);
                    }

                    // If file should be in FS but wasn't found, use DB version
                    path = VirtualPathHelper.GetVirtualFileRelativePath(pti.CodeName, extension, directory, prefix, versionGuid);
                    if (!SettingsKeyInfoProvider.DeploymentMode && WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage && !FileHelper.FileExists(path))
                    {
                        path = VirtualPathHelper.GetVirtualFileRelativePath(pti.CodeName, extension, directory, prefix, originalVersionGuid);
                    }
                }
            }
            return path;
        }


        /// <summary>
        /// Returns path to externally stored layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            ExternalColumnSettings<PageTemplateDeviceLayoutInfo> settings = new ExternalColumnSettings<PageTemplateDeviceLayoutInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStorePageTemplatesInFS",
                SetDataTransformation = (m, data, readOnly) => TextHelper.EnsureLineEndings(LayoutInfoProvider.AddLayoutDirectives(ValidationHelper.GetString(data, ""), m.LayoutType), "\r\n"),
                GetDataTransformation = (m, data) => VirtualPathHelper.RemoveDirectives(ValidationHelper.GetString(data, ""), LayoutInfoProvider.DefaultDirectives)
            };

            // CSS Component
            ExternalColumnSettings<PageTemplateDeviceLayoutInfo> cssSettings = new ExternalColumnSettings<PageTemplateDeviceLayoutInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStorePageTemplatesInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
            RegisterExternalColumn(EXTERNAL_COLUMN_CSS, cssSettings);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportSubscriptionInfo object.
        /// </summary>
        public PageTemplateDeviceLayoutInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportSubscriptionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public PageTemplateDeviceLayoutInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called.
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Modify:
                case PermissionsEnum.Read:
                    return "design";
            }
            return base.GetPermissionName(permission);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("cms.globalpermissions", "DestroyObjects", siteName, (UserInfo)userInfo) ||
                           // Check the page template destroy permission
                           UserInfoProvider.IsAuthorizedPerResource(ModuleName.DESIGN, "Destroy" + PageTemplateInfo.TYPEINFO.ObjectType.Replace(".", ""), siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Gets the theme path for the object.
        /// </summary>
        public string GetThemePath()
        {
            var profile = DeviceProfileInfoProvider.GetDeviceProfileInfo(ProfileID);

            if (profile != null)
            {
                return "~/App_Themes/Components/PageTemplates/DeviceLayouts/" + ValidationHelper.GetSafeFileName(profile.ProfileName);
            }

            return null;
        }

        #endregion
    }
}
