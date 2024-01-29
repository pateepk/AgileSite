using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(PollSiteInfo), PollSiteInfo.OBJECT_TYPE)]

namespace CMS.Polls
{
    /// <summary>
    /// PollSiteInfo data container class.
    /// </summary>
    public class PollSiteInfo : AbstractInfo<PollSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "polls.pollsite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PollSiteInfoProvider), OBJECT_TYPE, "Polls.PollSite", null, null, null, null, null, null, "SiteID", "PollID", PollInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            ModuleName = ModuleName.POLLS,
            RegisterAsBindingToObjectTypes = new List<string>() { PollInfo.OBJECT_TYPE, PollInfo.OBJECT_TYPE_GROUP },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the site.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
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
            PollSiteInfoProvider.DeletePollSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PollSiteInfoProvider.SetPollSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PollSiteInfo object.
        /// </summary>
        public PollSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PollSiteInfo object from the given DataRow.
        /// </summary>
        public PollSiteInfo(DataRow dr)
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