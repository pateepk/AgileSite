using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(RolePermissionInfo), RolePermissionInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// RolePermissionInfo data container class.
    /// </summary>
    public class RolePermissionInfo : AbstractInfo<RolePermissionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.rolepermission";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RolePermissionInfoProvider), OBJECT_TYPE, "CMS.RolePermission", null, null, null, null, null, null, null, "RoleID", PredefinedObjectType.ROLE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PermissionID", PermissionNameInfo.OBJECT_TYPE_CLASS, ObjectDependencyEnum.Binding)
            },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsGlobalObjects = true,
            ModuleName = ModuleName.PERMISSIONS,
            RegisterAsBindingToObjectTypes = new List<string>()
            {
                PredefinedObjectType.ROLE,
                PredefinedObjectType.GROUPROLE
            },
            RegisterAsOtherBindingToObjectTypes = new List<string>()
            {
                PermissionNameInfo.OBJECT_TYPE_CLASS,
                PermissionNameInfo.OBJECT_TYPE_RESOURCE
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RolePermissionInfoProvider.DeleteRolePermissionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RolePermissionInfoProvider.SetRolePermissionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RolePermissionInfo object.
        /// </summary>
        public RolePermissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RolePermissionInfo object from the given DataRow.
        /// </summary>
        public RolePermissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called. 
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return "Manage";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}