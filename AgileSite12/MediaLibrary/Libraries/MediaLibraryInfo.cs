using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.IO;
using CMS.DataEngine;
using CMS.MediaLibrary;

[assembly: RegisterObjectType(typeof(MediaLibraryInfo), MediaLibraryInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(MediaLibraryInfo), MediaLibraryInfo.OBJECT_TYPE_GROUP)]

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Media library info data container class.
    /// </summary>
    public class MediaLibraryInfo : AbstractInfo<MediaLibraryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.MEDIALIBRARY;

        /// <summary>
        /// Object type for group
        /// </summary>
        public const string OBJECT_TYPE_GROUP = "media.grouplibrary";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MediaLibraryInfoProvider), OBJECT_TYPE, "Media.Library", "LibraryID", "LibraryLastModified", "LibraryGUID", "LibraryName", "LibraryDisplayName", null, "LibrarySiteID", "LibraryGroupID", null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT) { ObjectType = MediaLibraryHelper.OBJECT_TYPE_FOLDER },
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            MacroCollectionName = "CMS.MediaLibrary",
            GroupIDColumn = "LibraryGroupID",
            ModuleName = ModuleName.MEDIALIBRARY,
            ThumbnailGUIDColumn = "LibraryTeaserGUID",
            HasMetaFiles = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                },
            },
            TypeCondition = new TypeCondition().WhereIsNull("LibraryGroupID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for group library.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = new ObjectTypeInfo(typeof(MediaLibraryInfoProvider), OBJECT_TYPE_GROUP, "Media.Library", "LibraryID", "LibraryLastModified", "LibraryGUID", "LibraryName", "LibraryDisplayName", null, "LibrarySiteID", "LibraryGroupID", PredefinedObjectType.GROUP)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFO,
            MacroCollectionName = "CMS.MediaLibrary",
            GroupIDColumn = "LibraryGroupID",
            ModuleName = ModuleName.MEDIALIBRARY,
            ThumbnailGUIDColumn = "LibraryTeaserGUID",
            HasMetaFiles = true,
            ImportExportSettings = { LogExport = true, AllowSingleExport = false },
            TypeCondition = new TypeCondition().WhereIsNotNull("LibraryGroupID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Media library full name in format [sitename].[libraryname].
        /// </summary>
        [DatabaseField]
        public virtual string LibraryFullName
        {
            get
            {
                if (LibrarySiteID > 0)
                {
                    SiteInfo site = SiteInfoProvider.GetSiteInfo(LibrarySiteID);
                    if (site != null)
                    {
                        return String.Format("{0}.{1}", site.SiteName, LibraryName);
                    }
                }

                return LibraryName;
            }
        }


        /// <summary>
        /// Library group ID.
        /// </summary>
        [DatabaseField]
        public virtual int LibraryGroupID
        {
            get
            {
                return GetIntegerValue("LibraryGroupID", 0);
            }
            set
            {
                SetValue("LibraryGroupID", value);
            }
        }


        /// <summary>
        /// Library description.
        /// </summary>
        [DatabaseField]
        public virtual string LibraryDescription
        {
            get
            {
                return GetStringValue("LibraryDescription", string.Empty);
            }
            set
            {
                SetValue("LibraryDescription", value);
            }
        }


        /// <summary>
        /// Library site ID.
        /// </summary>
        [DatabaseField]
        public virtual int LibrarySiteID
        {
            get
            {
                return GetIntegerValue("LibrarySiteID", 0);
            }
            set
            {
                SetValue("LibrarySiteID", value);
            }
        }


        /// <summary>
        /// Library folder.
        /// </summary>
        [DatabaseField]
        public virtual string LibraryFolder
        {
            get
            {
                return GetStringValue("LibraryFolder", string.Empty);
            }
            set
            {
                SetValue("LibraryFolder", value);
            }
        }


        /// <summary>
        /// Library name.
        /// </summary>
        [DatabaseField]
        public virtual string LibraryName
        {
            get
            {
                return GetStringValue("LibraryName", string.Empty);
            }
            set
            {
                SetValue("LibraryName", value);
            }
        }


        /// <summary>
        /// Library display name.
        /// </summary>
        [DatabaseField]
        public virtual string LibraryDisplayName
        {
            get
            {
                return GetStringValue("LibraryDisplayName", string.Empty);
            }
            set
            {
                SetValue("LibraryDisplayName", value);
            }
        }


        /// <summary>
        /// Library GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid LibraryGUID
        {
            get
            {
                return GetGuidValue("LibraryGUID", Guid.Empty);
            }
            set
            {
                SetValue("LibraryGUID", value);
            }
        }


        /// <summary>
        /// Library ID.
        /// </summary>
        [DatabaseField]
        public virtual int LibraryID
        {
            get
            {
                return GetIntegerValue("LibraryID", 0);
            }
            set
            {
                SetValue("LibraryID", value);
            }
        }


        /// <summary>
        /// Library access.
        /// </summary>
        private int LibraryAccess
        {
            get
            {
                return GetIntegerValue("LibraryAccess", 0);
            }
            set
            {
                SetValue("LibraryAccess", value);
            }
        }


        /// <summary>
        /// Library last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LibraryLastModified
        {
            get
            {
                return GetDateTimeValue("LibraryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LibraryLastModified", value);
            }
        }


        /// <summary>
        /// Indicates whether the file creating is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FileCreate
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 1);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 1);
            }
        }


        /// <summary>
        /// Indicates whether the file deleting is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FileDelete
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 2);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 2);
            }
        }


        /// <summary>
        /// Indicates whether the file modifying is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FileModify
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 3);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 3);
            }
        }


        /// <summary>
        /// Indicates whether the folder creating is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FolderCreate
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 4);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 4);
            }
        }


        /// <summary>
        /// Indicates whether the folder deleting is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FolderDelete
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 5);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 5);
            }
        }


        /// <summary>
        /// Indicates whether the folder modifying is allowed.
        /// </summary>
        public virtual SecurityAccessEnum FolderModify
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 6);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 6);
            }
        }


        /// <summary>
        /// Indicates whether the access to library is allowed.
        /// </summary>
        public virtual SecurityAccessEnum Access
        {
            get
            {
                return SecurityHelper.GetSecurityAccessEnum(LibraryAccess, 7);
            }
            set
            {
                LibraryAccess = SecurityHelper.SetSecurityAccessEnum(LibraryAccess, value, 7);
            }
        }


        /// <summary>
        /// Library teaser path.
        /// </summary>
        [DatabaseField]
        public virtual string LibraryTeaserPath
        {
            get
            {
                return GetStringValue("LibraryTeaserPath", string.Empty);
            }
            set
            {
                SetValue("LibraryTeaserPath", value);
            }
        }


        /// <summary>
        /// Library teaser guid.
        /// </summary>
        [DatabaseField("LibraryTeaserGUID")]
        public virtual Guid LibraryTeaserGuid
        {
            get
            {
                return GetGuidValue("LibraryTeaserGUID", Guid.Empty);
            }
            set
            {
                SetValue("LibraryTeaserGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return LibraryGroupID == 0 ? TYPEINFO : TYPEINFOGROUP;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MediaLibraryInfoProvider.DeleteMediaLibraryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MediaLibraryInfoProvider.SetMediaLibraryInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes library dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            if (MediaLibraryInfoProvider.DeletePhysicalFiles && DeleteFiles)
            {
                // Delete folder from the disk
                SiteInfo site = SiteInfoProvider.GetSiteInfo(LibrarySiteID);
                if (site != null)
                {
                    MediaLibraryInfoProvider.DeleteMediaLibraryFolder(site.SiteName, LibraryFolder, false);
                }
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Set special values
            string folderName = null;
            bool copyFiles = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                folderName = ValidationHelper.GetString(p[PredefinedObjectType.MEDIALIBRARY + ".foldername"], "");
                copyFiles = ValidationHelper.GetBoolean(p[PredefinedObjectType.MEDIALIBRARY + ".files"], false);
            }

            if (!string.IsNullOrEmpty(folderName))
            {
                LibraryFolder = folderName;
            }
            else
            {
                // Ensure unique folder name if it was not supplied in the settings
                string originalPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(originalObject.Generalized.ObjectID);
                LibraryFolder = DirectoryInfo.New(FileHelper.GetUniqueDirectoryName(originalPath)).Name;
            }

            Insert();

            // Try to copy files
            if (copyFiles)
            {
                MediaLibraryHelper.CloneLibraryFiles(originalObject.Generalized.ObjectID, LibraryID, settings, result);
            }
        }


        /// <summary>
        /// Clones the role bindings.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            if (settings.IncludeBindings)
            {
                // Clone project role permissions
                DataSet ds = MediaLibraryRolePermissionInfoProvider.GetLibraryRolePermissions("LibraryID = " + originalObject.Generalized.ObjectID);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int originalParentId = settings.ParentID;
                    settings.ParentID = LibraryID;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        MediaLibraryRolePermissionInfo binding = new MediaLibraryRolePermissionInfo(dr);
                        int newRoleId = settings.Translations.GetNewID(RoleInfo.OBJECT_TYPE_GROUP, binding.RoleID, null, 0, null, null, null);
                        if (newRoleId > 0)
                        {
                            binding.RoleID = newRoleId;
                        }
                        binding.Generalized.InsertAsClone(settings, result);
                    }

                    settings.ParentID = originalParentId;
                }
            }
        }


        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called (Modify => Manage).
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            string permissionName = base.GetPermissionName(permission);

            if (permissionName.ToLowerCSafe() == "modify")
            {
                permissionName = "Manage";
            }

            return permissionName;
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
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource("CMS.MediaLibrary", "Destroy", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MediaLibraryInfo object.
        /// </summary>
        public MediaLibraryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MediaLibraryInfo object from the given DataRow.
        /// </summary>
        public MediaLibraryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            const SecurityAccessEnum DEFAULT_LIBRARY_ACCESS = SecurityAccessEnum.Nobody;

            FileCreate = DEFAULT_LIBRARY_ACCESS;
            FolderCreate = DEFAULT_LIBRARY_ACCESS;
            FileDelete = DEFAULT_LIBRARY_ACCESS;
            FolderDelete = DEFAULT_LIBRARY_ACCESS;
            FileModify = DEFAULT_LIBRARY_ACCESS;
            FolderModify = DEFAULT_LIBRARY_ACCESS;
            Access = DEFAULT_LIBRARY_ACCESS;
        }

        #endregion
    }
}