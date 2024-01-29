using System;
using System.Web.Security;

using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.MembershipProvider
{
    /// <summary>
    /// User membership.
    /// </summary>
    public class CMSMembershipUser : MembershipUser
    {
        #region "Variable"

        private UserInfo mUserInfoMembership;

        #endregion


        #region "Properties"

        /// <summary>
        /// User info.
        /// </summary>
        public UserInfo UserInfoMembership
        {
            get
            {
                return mUserInfoMembership;
            }
            set
            {
                mUserInfoMembership = value;
            }
        }


        /// <summary>
        /// User name (reflects the UserName field).
        /// </summary>
        public override string UserName
        {
            get
            {
                return mUserInfoMembership.UserName;
            }
        }


        /// <summary>
        /// Email (reflects the Email field).
        /// </summary>
        public override string Email
        {
            get
            {
                return mUserInfoMembership.Email;
            }
            set
            {
                mUserInfoMembership.Email = value;
            }
        }


        /// <summary>
        /// Is approved (reflects the Enabled field).
        /// </summary>
        public override bool IsApproved
        {
            get
            {
                return mUserInfoMembership.Enabled;
            }
            set
            {
                mUserInfoMembership.Enabled = value;
            }
        }


        /// <summary>
        /// Creation date (reflects the UserCreated field).
        /// </summary>
        public override DateTime CreationDate
        {
            get
            {
                return mUserInfoMembership.UserCreated;
            }
        }


        /// <summary>
        /// Is locked out (reflects the UserEnabled and UserAccountLockReason fields).
        /// </summary>
        public override bool IsLockedOut
        {
            get
            {
                return mUserInfoMembership.UserIsDisabledManually;
            }
        }


        /// <summary>
        /// Last activity date (reflects the LastLogon field).
        /// </summary>
        public override DateTime LastActivityDate
        {
            get
            {
                return mUserInfoMembership.LastLogon;
            }
            set
            {
                mUserInfoMembership.LastLogon = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userInfoObj">UserInfo object</param>
        public CMSMembershipUser(UserInfo userInfoObj)
        {
            UserInfoMembership = userInfoObj;
        }

        #endregion
    }
}