using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Core;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(MembershipRoleInfo), MembershipRoleInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// MembershipRoleInfo data container class.
    /// </summary>
    public class MembershipRoleInfo : AbstractInfo<MembershipRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.membershiprole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MembershipRoleInfoProvider), OBJECT_TYPE, "CMS.MembershipRole", null, null, null, null, null, null, null, "MembershipID", MembershipInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            ModuleName = "cms.membership",
            ImportExportSettings =
            {
                LogExport = true
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            },
            TouchCacheDependencies = true,
            LogEvents = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of role object.
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
        /// ID of membership object.
        /// </summary>
        public virtual int MembershipID
        {
            get
            {
                return GetIntegerValue("MembershipID", 0);
            }
            set
            {
                SetValue("MembershipID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MembershipRoleInfoProvider.DeleteMembershipRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MembershipRoleInfoProvider.SetMembershipRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MembershipRoleInfo object.
        /// </summary>
        public MembershipRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MembershipRoleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MembershipRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

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
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource(ModuleName.MEMBERSHIP, "Modify", siteName, exceptionOnFailure)
                        && userInfo.IsAuthorizedPerResource("CMS.Roles", "Modify", siteName, exceptionOnFailure); 

                case PermissionsEnum.Read:                   
                    return userInfo.IsAuthorizedPerResource(ModuleName.MEMBERSHIP, "Read", siteName, exceptionOnFailure)
                        && userInfo.IsAuthorizedPerResource("CMS.Roles", "Read", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }           
        }

        #endregion
    }
}