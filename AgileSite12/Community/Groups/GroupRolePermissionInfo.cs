using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Community;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(GroupRolePermissionInfo), GroupRolePermissionInfo.OBJECT_TYPE)]

namespace CMS.Community
{
    /// <summary>
    /// Class providing Group/Role/Permission data.
    /// </summary>
    public class GroupRolePermissionInfo : AbstractInfo<GroupRolePermissionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "community.grouprolepermission";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GroupRolePermissionInfoProvider), OBJECT_TYPE, "Community.GroupRolePermission", null, null, null, null, null, null, null, "GroupID", GroupInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.GROUPS,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE_GROUP, ObjectDependencyEnum.Binding),
                new ObjectDependency("PermissionID", PermissionNameInfo.OBJECT_TYPE_RESOURCE, ObjectDependencyEnum.Binding)
            },
            RegisterAsOtherBindingToObjectTypes = new List<string>
            {
                RoleInfo.OBJECT_TYPE_GROUP,
                PermissionNameInfo.OBJECT_TYPE_RESOURCE
            },
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Group ID.
        /// </summary>
        public virtual int GroupID
        {
            get
            {
                return GetIntegerValue("GroupID", 0);
            }
            set
            {
                SetValue("GroupID", value);
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
            GroupRolePermissionInfoProvider.DeleteGroupRolePermissionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            GroupRolePermissionInfoProvider.SetGroupRolePermissionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumRoleInfo object.
        /// </summary>
        public GroupRolePermissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumRoleInfo object from the given DataRow.
        /// </summary>
        public GroupRolePermissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}