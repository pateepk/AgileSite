using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SocialMarketing;

[assembly: RegisterObjectType(typeof(FacebookAccountInfo), FacebookAccountInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a Facebook account (page).
    /// </summary>
    public class FacebookAccountInfo : AbstractInfo<FacebookAccountInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.facebookaccount";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FacebookAccountInfoProvider), OBJECT_TYPE, "SM.FacebookAccount", "FacebookAccountID", "FacebookAccountLastModified", "FacebookAccountGUID", "FacebookAccountName", "FacebookAccountDisplayName", null, "FacebookAccountSiteID", "FacebookAccountFacebookApplicationID", FacebookApplicationInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            Feature = FeatureEnum.SocialMarketing,
            SensitiveColumns = new List<string> { "FacebookAccountPageAccessToken" },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Current information about a Facebook page access token.
        /// </summary>
        private FacebookPageAccessTokenData? mFacebookPageAccessToken;
        
        
        /// <summary>
        /// Current information about a Facebook page.
        /// </summary>
        private FacebookPageIdentityData? mFacebookPageIdentity;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets information about a Facebook page.
        /// </summary>
        [Obsolete("Property will be removed in the next version. Use property CMS.SocialMarketing.FacebookAccountInfo.FacebookAccountPageID directly.")]
        public virtual FacebookPageIdentityData FacebookPageIdentity
        {
            get
            {
                if (!mFacebookPageIdentity.HasValue)
                {
                    mFacebookPageIdentity = new FacebookPageIdentityData(FacebookAccountPageUrl, FacebookAccountPageID);
                }

                return mFacebookPageIdentity.Value;
            }
            set
            {
                FacebookAccountPageUrl = value.PageUrl;
                FacebookAccountPageID = value.PageId;
                mFacebookPageIdentity = value;
            }
        }


        /// <summary>
        /// Gets or sets information about a Facebook page access token.
        /// </summary>
        public virtual FacebookPageAccessTokenData FacebookPageAccessToken
        {
            get
            {
                if (!mFacebookPageAccessToken.HasValue)
                {
                    mFacebookPageAccessToken = new FacebookPageAccessTokenData(FacebookAccountPageAccessToken, FacebookAccountPageAccessTokenExpiration);
                }

                return mFacebookPageAccessToken.Value;
            }
            set
            {
                FacebookAccountPageAccessToken = value.AccessToken;
                FacebookAccountPageAccessTokenExpiration = value.Expiration;
                mFacebookPageAccessToken = value;
            }
        }


        /// <summary>
        /// Gets or sets the Facebook account page URL.
        /// </summary>
        [DatabaseField]
        [Obsolete("Property will be removed in the next version. Use property CMS.SocialMarketing.FacebookAccountInfo.FacebookAccountPageID directly.")]
        public virtual string FacebookAccountPageUrl
        {
            get
            {
                return GetStringValue("FacebookAccountPageUrl", String.Empty);
            }
            set
            {
                SetValue("FacebookAccountPageUrl", value);
            }
        }


        /// <summary>
        /// Gets or sets the Facebook account page ID
        /// </summary>
        [DatabaseField]
        public virtual string FacebookAccountPageID
        {
            get
            {
                return GetStringValue("FacebookAccountPageID", String.Empty);
            }
            set
            {
                SetValue("FacebookAccountPageID", value);
            }
        }


        /// <summary>
        /// Gets or sets the Facebook account access token expiration.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? FacebookAccountPageAccessTokenExpiration
        {
            get
            {
                DateTime? expiration = GetDateTimeValue("FacebookAccountPageAccessTokenExpiration", DateTimeHelper.ZERO_TIME);
                return (expiration == DateTimeHelper.ZERO_TIME) ? null : expiration;
            }
            set
            {
                SetValue("FacebookAccountPageAccessTokenExpiration", value);
            }
        }


        /// <summary>
        /// Gets or sets the Facebook account access token.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookAccountPageAccessToken
        {
            get
            {
                return GetStringValue("FacebookAccountPageAccessToken", String.Empty);
            }
            set
            {
                SetValue("FacebookAccountPageAccessToken", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Facebook application object.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookAccountFacebookApplicationID
        {
            get
            {
                return GetIntegerValue("FacebookAccountFacebookApplicationID", 0);
            }
            set
            {
                SetValue("FacebookAccountFacebookApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets indicator whether Account is default for its site
        /// </summary>
        [DatabaseField]
        public virtual bool FacebookAccountIsDefault
        {
            get
            {
                return GetBooleanValue("FacebookAccountIsDefault", false);
            }
            set
            {
                SetValue("FacebookAccountIsDefault", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the globally unique identifier of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual Guid FacebookAccountGUID
        {
            get
            {
                return GetGuidValue("FacebookAccountGUID", Guid.Empty);
            }
            set
            {         
                SetValue("FacebookAccountGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookAccountSiteID
        {
            get
            {
                return GetIntegerValue("FacebookAccountSiteID", 0);
            }
            set
            {         
                SetValue("FacebookAccountSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FacebookAccountLastModified
        {
            get
            {
                return GetDateTimeValue("FacebookAccountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("FacebookAccountLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookAccountName
        {
            get
            {
                return GetStringValue("FacebookAccountName", "");
            }
            set
            {         
                SetValue("FacebookAccountName", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookAccountID
        {
            get
            {
                return GetIntegerValue("FacebookAccountID", 0);
            }
            set
            {         
                SetValue("FacebookAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the Facebook account.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookAccountDisplayName
        {
            get
            {
                return GetStringValue("FacebookAccountDisplayName", "");
            }
            set
            {         
                SetValue("FacebookAccountDisplayName", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            FacebookAccountInfoProvider.DeleteFacebookAccountInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            FacebookAccountInfoProvider.SetFacebookAccountInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("FacebookAccountPageAccessToken", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.EncryptData((string)value);
            }
            return base.SetValue(columnName, value);
        }


        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetValue(string columnName)
        {
            var value = base.GetValue(columnName);
            if (columnName.EqualsCSafe("FacebookAccountPageAccessToken", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string)value);
            }
            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FacebookAccountInfo class.
        /// </summary>
        public FacebookAccountInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the FacebookAccountInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FacebookAccountInfo(DataRow dr) : base(TYPEINFO, dr)
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
            var user = (UserInfo)userInfo;

            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Read", siteName, user, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Modify", siteName, user, false) ||
                        UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "ModifyAccounts", siteName, user, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }

}