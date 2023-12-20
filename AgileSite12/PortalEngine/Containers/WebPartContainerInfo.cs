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

[assembly: RegisterObjectType(typeof(WebPartContainerInfo), WebPartContainerInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WebPartContainerInfo data container class.
    /// </summary>
    public class WebPartContainerInfo : AbstractInfo<WebPartContainerInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webpartcontainer";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPartContainerInfoProvider), OBJECT_TYPE, "CMS.WebPartContainer", "ContainerID", "ContainerLastModified", "ContainerGUID", "ContainerName", "ContainerDisplayName", null, null, null, null)
        {
            ModuleName = ModuleName.DESIGN,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            SupportsLocking = true,
            HasExternalColumns = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Container text before.
        /// </summary>
        [DatabaseField]
        public virtual string ContainerTextBefore
        {
            get
            {
                return GetStringValue("ContainerTextBefore", "");
            }
            set
            {
                SetValue("ContainerTextBefore", value);
            }
        }


        /// <summary>
        /// Container text after.
        /// </summary>
        [DatabaseField]
        public virtual string ContainerTextAfter
        {
            get
            {
                return GetStringValue("ContainerTextAfter", "");
            }
            set
            {
                SetValue("ContainerTextAfter", value);
            }
        }


        /// <summary>
        /// Container CSS styles.
        /// </summary>
        [DatabaseField]
        public virtual string ContainerCSS
        {
            get
            {
                return GetStringValue("ContainerCSS", "");
            }
            set
            {
                SetValue("ContainerCSS", value);
            }
        }


        /// <summary>
        /// Container name.
        /// </summary>
        [DatabaseField]
        public virtual string ContainerName
        {
            get
            {
                return GetStringValue("ContainerName", "");
            }
            set
            {
                SetValue("ContainerName", value);
            }
        }


        /// <summary>
        /// Container id.
        /// </summary>
        [DatabaseField]
        public virtual int ContainerID
        {
            get
            {
                return GetIntegerValue("ContainerID", 0);
            }
            set
            {
                SetValue("ContainerID", value);
            }
        }


        /// <summary>
        /// Container display name.
        /// </summary>
        [DatabaseField]
        public virtual string ContainerDisplayName
        {
            get
            {
                return GetStringValue("ContainerDisplayName", "");
            }
            set
            {
                SetValue("ContainerDisplayName", value);
            }
        }


        /// <summary>
        /// Container GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ContainerGUID
        {
            get
            {
                return GetGuidValue("ContainerGUID", Guid.Empty);
            }
            set
            {
                SetValue("ContainerGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContainerLastModified
        {
            get
            {
                return GetDateTimeValue("ContainerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ContainerLastModified", value, DateTimeHelper.ZERO_TIME);
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
            WebPartContainerInfoProvider.DeleteWebPartContainerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebPartContainerInfoProvider.SetWebPartContainerInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebPartContainerInfo object.
        /// </summary>
        public WebPartContainerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebPartContainerInfo object from the given DataRow.
        /// </summary>
        public WebPartContainerInfo(DataRow dr)
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
            return "~/App_Themes/Components/Containers/" + ValidationHelper.GetSafeFileName(ContainerName);
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
                copyFiles = ValidationHelper.GetBoolean(p["cms.webpartcontainer" + ".appthemes"], false);
            }

            if (copyFiles)
            {
                // Copy files from App_Themes
                string sourcePath = "~/App_Themes/Components/Containers/" + originalObject.Generalized.ObjectCodeName;
                string targetPath = "~/App_Themes/Components/Containers/" + ObjectCodeName;

                FileHelper.CopyDirectory(sourcePath, targetPath);
            }

            Insert();
        }


        /// <summary>
        /// Returns virtual relative path for specific column.
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            string extension = ".html";
            string directory = WebPartContainerInfoProvider.WebPartContainersDirectory;
            string suffix = String.Empty;

            switch (externalColumnName.ToLowerCSafe())
            {
                case "containertextafter":
                    suffix = "_after";
                    break;

                case "containertextbefore":
                    suffix = "_before";
                    break;

                case "containercss":
                    extension = ".css";
                    break;
            }

            return VirtualPathHelper.GetVirtualFileRelativePath(ContainerName + suffix, extension, directory, null, null);
        }


        /// <summary>
        /// Returns path to externally stored before/after codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            ExternalColumnSettings<WebPartContainerInfo> settingsBefore = new ExternalColumnSettings<WebPartContainerInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath("ContainerTextBefore", null),
                StoreInExternalStorageSettingsKey = "CMSStoreWebpartContainersInFS"
            };
            ExternalColumnSettings<WebPartContainerInfo> settingsAfter = new ExternalColumnSettings<WebPartContainerInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath("ContainerTextAfter", null),
                StoreInExternalStorageSettingsKey = "CMSStoreWebpartContainersInFS"
            };
            ExternalColumnSettings<WebPartContainerInfo> settingsContainerCSS = new ExternalColumnSettings<WebPartContainerInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath("ContainerCSS", null),
                StoreInExternalStorageSettingsKey = "CMSStoreWebpartContainersInFS"
            };

            RegisterExternalColumn("ContainerTextBefore", settingsBefore);
            RegisterExternalColumn("ContainerTextAfter", settingsAfter);
            RegisterExternalColumn("ContainerCSS", settingsContainerCSS);
        }

        #endregion
    }
}