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

[assembly: RegisterObjectType(typeof(TwitterAccountInfo), TwitterAccountInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Represents a Twitter account (channel).
    /// </summary>
    public class TwitterAccountInfo : AbstractInfo<TwitterAccountInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.twitteraccount";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TwitterAccountInfoProvider), OBJECT_TYPE, "SM.TwitterAccount", "TwitterAccountID", "TwitterAccountLastModified", "TwitterAccountGUID", "TwitterAccountName", "TwitterAccountDisplayName", null, "TwitterAccountSiteID", "TwitterAccountTwitterApplicationID", TwitterApplicationInfo.OBJECT_TYPE)
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
            SensitiveColumns = new List<string> { "TwitterAccountAccessToken", "TwitterAccountAccessTokenSecret" },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "TwitterAccountFollowers",
                    "TwitterAccountMentions",
                    "TwitterAccountMentionsRange"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the access token of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterAccountAccessToken
        {
            get
            {
                return GetStringValue("TwitterAccountAccessToken", String.Empty);
            }
            set
            {
                SetValue("TwitterAccountAccessToken", value);
            }
        }


        /// <summary>
        /// Gets or sets the access token secret of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterAccountAccessTokenSecret
        {
            get
            {
                return GetStringValue("TwitterAccountAccessTokenSecret", String.Empty);
            }
            set
            {
                SetValue("TwitterAccountAccessTokenSecret", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Twitter application object.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterAccountTwitterApplicationID
        {
            get
            {
                return GetIntegerValue("TwitterAccountTwitterApplicationID", 0);
            }
            set
            {
                SetValue("TwitterAccountTwitterApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets the Twitter user identifier.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterAccountUserID
        {
            get
            {
                return GetStringValue("TwitterAccountUserID", String.Empty);
            }
            set
            {
                SetValue("TwitterAccountUserID", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of followers on Twitter.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterAccountFollowers
        {
            get
            {
                return GetIntegerValue("TwitterAccountFollowers", 0);
            }
            set
            {
                SetValue("TwitterAccountFollowers", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of mentions on Twitter.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterAccountMentions
        {
            get
            {
                return GetIntegerValue("TwitterAccountMentions", 0);
            }
            set
            {
                SetValue("TwitterAccountMentions", value);
            }
        }


        /// <summary>
        /// Gets or sets indicator whether Account is default for its site
        /// </summary>
        [DatabaseField]
        public virtual bool TwitterAccountIsDefault
        {
            get
            {
                return GetBooleanValue("TwitterAccountIsDefault", false);
            }
            set
            {
                SetValue("TwitterAccountIsDefault", value);
            }
        }


        /// <summary>
        /// Gets or sets the range of identifiers on Twitter mentions timeline that the number of mentions was computed from.
        /// </summary>
        [DatabaseField]
        internal virtual TwitterIdentifierRange? TwitterAccountMentionsRange
        {
            get
            {
                string text = GetStringValue("TwitterAccountMentionsRange", String.Empty);
                if (String.IsNullOrEmpty(text))
                {
                    return null;
                }
                return TwitterIdentifierRange.Parse(text);
            }
            set
            {
                string text = null;
                if (value != null)
                {
                    text = value.ToString();
                }
                SetValue("TwitterAccountMentionsRange", text);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the globally unique identifier of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual Guid TwitterAccountGUID
        {
            get
            {
                return GetGuidValue("TwitterAccountGUID", Guid.Empty);
            }
            set
            {         
                SetValue("TwitterAccountGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterAccountSiteID
        {
            get
            {
                return GetIntegerValue("TwitterAccountSiteID", 0);
            }
            set
            {         
                SetValue("TwitterAccountSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TwitterAccountLastModified
        {
            get
            {
                return GetDateTimeValue("TwitterAccountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("TwitterAccountLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterAccountName
        {
            get
            {
                return GetStringValue("TwitterAccountName", String.Empty);
            }
            set
            {         
                SetValue("TwitterAccountName", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterAccountID
        {
            get
            {
                return GetIntegerValue("TwitterAccountID", 0);
            }
            set
            {         
                SetValue("TwitterAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the Twitter account.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterAccountDisplayName
        {
            get
            {
                return GetStringValue("TwitterAccountDisplayName", String.Empty);
            }
            set
            {         
                SetValue("TwitterAccountDisplayName", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            TwitterAccountInfoProvider.DeleteTwitterAccountInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            TwitterAccountInfoProvider.SetTwitterAccountInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("TwitterAccountAccessTokenSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.EncryptData((string) value);
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
            if (columnName.EqualsCSafe("TwitterAccountAccessTokenSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string) value);
            }
            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterAccountInfo class.
        /// </summary>
        public TwitterAccountInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the TwitterAccountInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TwitterAccountInfo(DataRow dr) : base(TYPEINFO, dr)
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
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "Read", siteName, user, false);

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