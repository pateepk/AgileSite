using System;
using System.Collections;
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

[assembly: RegisterObjectType(typeof(LayoutInfo), LayoutInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Layout info data container class.
    /// </summary>
    public class LayoutInfo : AbstractInfo<LayoutInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.layout";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LayoutInfoProvider), OBJECT_TYPE, "CMS.Layout", "LayoutID", "LayoutLastModified", "LayoutGUID", "LayoutCodeName", "LayoutDisplayName", null, null, null, null)
        {
            ModuleName = ModuleName.DESIGN,
            ThumbnailGUIDColumn = "LayoutThumbnailGUID",
            HasMetaFiles = true,
            SupportsLocking = true,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.All,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "LayoutVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CSSColumn = EXTERNAL_COLUMN_CSS,
            DefaultData = new DefaultDataSettings()
        };

        #endregion


        #region "Variables"

        int? mLayoutZoneCountAutomatic;

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
        /// The layout ID.
        /// </summary>
        [DatabaseField]
        public int LayoutId
        {
            get
            {
                return GetIntegerValue("LayoutID", 0);
            }
            set
            {
                SetValue("LayoutID", value);
            }
        }


        /// <summary>
        /// The layout display name.
        /// </summary>
        [DatabaseField]
        public string LayoutDisplayName
        {
            get
            {
                return GetStringValue("LayoutDisplayName", "");
            }
            set
            {
                SetValue("LayoutDisplayName", value);
            }
        }


        /// <summary>
        /// The layout description.
        /// </summary>
        [DatabaseField]
        public string LayoutDescription
        {
            get
            {
                return GetStringValue("LayoutDescription", "");
            }
            set
            {
                SetValue("LayoutDescription", value);
            }
        }


        /// <summary>
        /// Layout CSS.
        /// </summary>
        [DatabaseField]
        public string LayoutCSS
        {
            get
            {
                return GetStringValue("LayoutCSS", "");
            }
            set
            {
                SetValue("LayoutCSS", value);
            }
        }


        /// <summary>
        /// Layout is convertible.
        /// </summary>
        [DatabaseField]
        public bool LayoutIsConvertible
        {
            get
            {
                return GetBooleanValue("LayoutIsConvertible", false);
            }
            set
            {
                SetValue("LayoutIsConvertible", value);
            }
        }


        /// <summary>
        /// Number of zones.
        /// </summary>
        [DatabaseField]
        public int LayoutZoneCount
        {
            get
            {
                return GetIntegerValue("LayoutZoneCount", -1);
            }
            set
            {
                SetValue("LayoutZoneCount", value);
            }
        }


        /// <summary>
        /// Number of zones (counted automatically).
        /// </summary>
        public int LayoutZoneCountAutomatic
        {
            get
            {
                if (!mLayoutZoneCountAutomatic.HasValue)
                {
                    mLayoutZoneCountAutomatic = LayoutInfoProvider.CountWebpartZones(LayoutCode, LayoutType);
                }
                return mLayoutZoneCountAutomatic.Value;
            }
        }


        /// <summary>
        /// Layout type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public LayoutTypeEnum LayoutType
        {
            get
            {
                return LayoutInfoProvider.GetLayoutTypeEnum(GetStringValue("LayoutType", ""));
            }
            set
            {
                SetValue("LayoutType", LayoutInfoProvider.GetLayoutTypeCode(value));
            }
        }


        /// <summary>
        /// The layout code.
        /// </summary>
        [DatabaseField]
        public string LayoutCode
        {
            get
            {
                return GetStringValue("LayoutCode", "");
            }
            set
            {
                SetValue("LayoutCode", value);
            }
        }


        /// <summary>
        /// The layout code name.
        /// </summary>
        [DatabaseField]
        public string LayoutCodeName
        {
            get
            {
                return GetStringValue("LayoutCodeName", "");
            }
            set
            {
                SetValue("LayoutCodeName", value);
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
                return GetStringValue("LayoutVersionGUID", "");
            }
            set
            {
                SetValue("LayoutVersionGUID", value);
            }
        }


        /// <summary>
        /// Layout GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid LayoutGUID
        {
            get
            {
                return GetGuidValue("LayoutGUID", Guid.Empty);
            }
            set
            {
                SetValue("LayoutGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LayoutLastModified
        {
            get
            {
                return GetDateTimeValue("LayoutLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LayoutLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Layout thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid LayoutThumbnailGUID
        {
            get
            {
                return GetGuidValue("LayoutThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("LayoutThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Layout icon class defining the page layout thumbnail.
        /// </summary>
        [DatabaseField]
        public virtual string LayoutIconClass
        {
            get
            {
                return GetStringValue("LayoutIconClass", null);
            }
            set
            {
                SetValue("LayoutIconClass", value, string.Empty);
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
            LayoutInfoProvider.DeleteLayoutInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            // Do not store empty IconClass, use NULL value
            if (string.IsNullOrEmpty(LayoutIconClass))
            {
                LayoutIconClass = null;
            }

            LayoutInfoProvider.SetLayoutInfo(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            // Ensure extension
            string extension = (LayoutType == LayoutTypeEnum.Html) ? ".html" : ".ascx";
            if (EXTERNAL_COLUMN_CSS.EqualsCSafe(externalColumnName, true))
            {
                extension = ".css";
            }

            // Keep original version GUID
            string originalVersionGuid = versionGuid;

            // Do not use version GUID for files stored externally
            if (LayoutInfoProvider.StoreLayoutsInExternalStorage || SettingsKeyInfoProvider.DeploymentMode)
            {
                versionGuid = null;
            }

            string directory = LayoutInfoProvider.LayoutsDirectory;

            string path = VirtualPathHelper.GetVirtualFileRelativePath(LayoutCodeName, extension, directory, null, versionGuid);
            if (!SettingsKeyInfoProvider.DeploymentMode && LayoutInfoProvider.StoreLayoutsInExternalStorage && !FileHelper.FileExists(path))
            {
                path = VirtualPathHelper.GetVirtualFileRelativePath(LayoutCodeName, extension, directory, null, originalVersionGuid);
            }

            return path;
        }


        /// <summary>
        /// Returns path to externally stored layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            // Code
            ExternalColumnSettings<LayoutInfo> settings = new ExternalColumnSettings<LayoutInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreLayoutsInFS",
                SetDataTransformation = (m, data, readOnly) => LayoutInfoProvider.AddLayoutDirectives(ValidationHelper.GetString(data, ""), m.LayoutType),
                GetDataTransformation = (m, data) => VirtualPathHelper.RemoveDirectives(ValidationHelper.GetString(data, ""), LayoutInfoProvider.DefaultDirectives)
            };

            // CSS Component
            ExternalColumnSettings<LayoutInfo> cssSettings = new ExternalColumnSettings<LayoutInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStoreLayoutsInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
            RegisterExternalColumn(EXTERNAL_COLUMN_CSS, cssSettings);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty LayoutInfo structure.
        /// </summary>
        public LayoutInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates LayoutInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public LayoutInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            return "~/App_Themes/Components/Layouts/" + ValidationHelper.GetSafeFileName(LayoutCodeName);
        }


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
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure) ||
                           UserInfoProvider.IsAuthorizedPerResource(ModuleName.DESIGN, "Destroy" + TypeInfo.ObjectType.Replace(".", ""), siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool copyFiles = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                copyFiles = ValidationHelper.GetBoolean(p["cms.layout" + ".appthemes"], false);
            }

            if (copyFiles)
            {
                // Copy files from App_Themes
                string sourcePath = "~/App_Themes/Components/Layouts/" + originalObject.Generalized.ObjectCodeName;
                string targetPath = "~/App_Themes/Components/Layouts/" + ObjectCodeName;

                FileHelper.CopyDirectory(sourcePath, targetPath);
            }

            Insert();
        }

        #endregion
    }
}