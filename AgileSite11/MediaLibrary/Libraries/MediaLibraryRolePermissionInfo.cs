using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Modules;
using CMS.MediaLibrary;

[assembly: RegisterObjectType(typeof(MediaLibraryRolePermissionInfo), MediaLibraryRolePermissionInfo.OBJECT_TYPE)]

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Media library role permission data container class.
    /// </summary>
    public class MediaLibraryRolePermissionInfo : AbstractInfo<MediaLibraryRolePermissionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "media.libraryrolepermission";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MediaLibraryRolePermissionInfoProvider), OBJECT_TYPE, "Media.LibraryRolePermission", null, null, null, null, null, null, null, "LibraryID", MediaLibraryInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("PermissionID", PermissionNameInfo.OBJECT_TYPE_RESOURCE, ObjectDependencyEnum.Binding)
            },

            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.TouchParent,
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            RegisterAsOtherBindingToObjectTypes = new List<string> { RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP, PermissionNameInfo.OBJECT_TYPE_RESOURCE },
            RegisterAsBindingToObjectTypes = new List<string> { MediaLibraryInfo.OBJECT_TYPE, MediaLibraryInfo.OBJECT_TYPE_GROUP },
            ImportExportSettings = { LogExport = false },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Library ID.
        /// </summary>
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
        /// Role ID.
        /// </summary>
        public virtual int RoleID
        {
            get
            {
                return GetIntegerValue("RoleID", 0);
            }
            set
            {
                SetValue("RoleID", value);
            }
        }


        /// <summary>
        /// Permission ID.
        /// </summary>
        public virtual int PermissionID
        {
            get
            {
                return GetIntegerValue("PermissionID", 0);
            }
            set
            {
                SetValue("PermissionID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MediaLibraryRolePermissionInfoProvider.DeleteMediaLibraryRolePermissionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MediaLibraryRolePermissionInfoProvider.SetMediaLibraryRolePermissionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MediaLibraryRolePermission object.
        /// </summary>
        public MediaLibraryRolePermissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MediaLibraryRolePermission object from the given DataRow.
        /// </summary>
        public MediaLibraryRolePermissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}