using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(PollRoleInfo), PollRoleInfo.OBJECT_TYPE)]

namespace CMS.Polls
{
    /// <summary>
    /// PollRoleInfo data container class.
    /// </summary>
    public class PollRoleInfo : AbstractInfo<PollRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "polls.pollrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PollRoleInfoProvider), OBJECT_TYPE, "Polls.PollRole", null, null, null, null, null, null, null, "PollID", PollInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            ModuleName = ModuleName.POLLS,
            RegisterAsOtherBindingToObjectTypes = new List<string>() { RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP },
            RegisterAsBindingToObjectTypes = new List<string>() { PollInfo.OBJECT_TYPE, PollInfo.OBJECT_TYPE_GROUP },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the role.
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
        /// ID of the poll.
        /// </summary>
        public virtual int PollID
        {
            get
            {
                return GetIntegerValue("PollID", 0);
            }
            set
            {
                SetValue("PollID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PollRoleInfoProvider.DeletePollRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PollRoleInfoProvider.SetPollRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PollRoleInfo object.
        /// </summary>
        public PollRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PollRoleInfo object from the given DataRow.
        /// </summary>
        public PollRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            PollInfo pi = null;
            switch (permission)
            {
                case PermissionsEnum.Read:
                    pi = PollInfoProvider.GetPollInfo(PollID);
                    if (pi != null)
                    {
                        if ((pi.PollGroupID > 0) && (pi.PollSiteID > 0))
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "read", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                        if (pi.PollSiteID <= 0)
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalread", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    pi = PollInfoProvider.GetPollInfo(PollID);
                    if (pi != null)
                    {

                        if ((pi.PollGroupID > 0) && (pi.PollSiteID > 0))
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "manage", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                        if (pi.PollSiteID <= 0)
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalmodify", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}