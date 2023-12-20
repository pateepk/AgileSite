using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Membership;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(ForumRoleInfo), ForumRoleInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumRoleInfo data container class.
    /// </summary>
    public class ForumRoleInfo : AbstractInfo<ForumRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumRoleInfoProvider), OBJECT_TYPE, "Forums.ForumRole", null, null, null, null, null, null, null, "ForumID", ForumInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.FORUMS,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("PermissionID", PermissionNameInfo.OBJECT_TYPE_RESOURCE, ObjectDependencyEnum.Binding)
            },
            TouchCacheDependencies = true,
            RegisterAsOtherBindingToObjectTypes = new List<string>
            {
                RoleInfo.OBJECT_TYPE,
                RoleInfo.OBJECT_TYPE_GROUP,
                PermissionNameInfo.OBJECT_TYPE_RESOURCE
            },
            RegisterAsBindingToObjectTypes = new List<string>
            {
                ForumInfo.OBJECT_TYPE,
                ForumInfo.OBJECT_TYPE_GROUP
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

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
        /// Forum ID.
        /// </summary>
        public virtual int ForumID
        {
            get
            {
                return GetIntegerValue("ForumID", 0);
            }
            set
            {
                SetValue("ForumID", value);
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
            ForumRoleInfoProvider.DeleteForumRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumRoleInfoProvider.SetForumRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumRoleInfo object.
        /// </summary>
        public ForumRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumRoleInfo object from the given DataRow.
        /// </summary>
        public ForumRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}