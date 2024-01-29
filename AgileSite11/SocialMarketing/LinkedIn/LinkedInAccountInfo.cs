using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SocialMarketing;

[assembly: RegisterObjectType(typeof(LinkedInAccountInfo), LinkedInAccountInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Represents a LinkedIn account (company profile).
    /// </summary>
    [Serializable]
    public class LinkedInAccountInfo : AbstractInfo<LinkedInAccountInfo>
    {

        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.linkedinaccount";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LinkedInAccountInfoProvider), OBJECT_TYPE, "SM.LinkedInAccount", "LinkedInAccountID", "LinkedInAccountLastModified", "LinkedInAccountGUID", "LinkedInAccountName", "LinkedInAccountDisplayName", null, "LinkedInAccountSiteID", "LinkedInAccountLinkedInApplicationID", LinkedInApplicationInfo.OBJECT_TYPE)
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
            ModuleName = ModuleName.SOCIALMARKETING,
            SensitiveColumns = new List<string> { "LinkedInAccountAccessToken", "LinkedInAccountAccessTokenSecret" },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the access token of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountAccessToken
        {
            get
            {
                return GetStringValue("LinkedInAccountAccessToken", String.Empty);
            }
            set
            {
                SetValue("LinkedInAccountAccessToken", value);
            }
        }


        /// <summary>
        /// Gets or sets the access token secret of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountAccessTokenSecret
        {
            get
            {
                return GetStringValue("LinkedInAccountAccessTokenSecret", String.Empty);
            }
            set
            {
                SetValue("LinkedInAccountAccessTokenSecret", value);
            }
        }


        /// <summary>
        /// Gets or sets the access token secret expiration.
        /// </summary>
        [DatabaseField]
        public virtual DateTime? LinkedInAccountAccessTokenExpiration
        {
            get
            {
                DateTime? expiration = GetDateTimeValue("LinkedInAccountAccessTokenExpiration", DateTimeHelper.ZERO_TIME);
                return (expiration == DateTimeHelper.ZERO_TIME) ? null : expiration;
            }
            set
            {
                SetValue("LinkedInAccountAccessTokenExpiration", value);
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the LinkedIn application object.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInAccountLinkedInApplicationID
        {
            get
            {
                return GetIntegerValue("LinkedInAccountLinkedInApplicationID", 0);
            }
            set
            {
                SetValue("LinkedInAccountLinkedInApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn profile identifier.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountProfileID
        {
            get
            {
                return GetStringValue("LinkedInAccountProfileID", String.Empty);
            }
            set
            {
                SetValue("LinkedInAccountProfileID", value);
            }
        }


        /// <summary>
        /// Gets or sets the LinkedIn profile name.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountProfileName
        {
            get
            {
                return GetStringValue("LinkedInAccountProfileName", String.Empty);
            }
            set
            {
                SetValue("LinkedInAccountProfileName", value);
            }
        }


        /// <summary>
        /// Gets or sets indicator whether Account is default for its site.
        /// </summary>
        [DatabaseField]
        public virtual bool LinkedInAccountIsDefault
        {
            get
            {
                return GetBooleanValue("LinkedInAccountIsDefault", false);
            }
            set
            {
                SetValue("LinkedInAccountIsDefault", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInAccountID
        {
            get
            {
                return GetIntegerValue("LinkedInAccountID", 0);
            }
            set
            {
                SetValue("LinkedInAccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the globally unique identifier of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual Guid LinkedInAccountGUID
        {
            get
            {
                return GetGuidValue("LinkedInAccountGUID", Guid.Empty);
            }
            set
            {         
                SetValue("LinkedInAccountGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInAccountSiteID
        {
            get
            {
                return GetIntegerValue("LinkedInAccountSiteID", 0);
            }
            set
            {         
                SetValue("LinkedInAccountSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LinkedInAccountLastModified
        {
            get
            {
                return GetDateTimeValue("LinkedInAccountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("LinkedInAccountLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountName
        {
            get
            {
                return GetStringValue("LinkedInAccountName", String.Empty);
            }
            set
            {         
                SetValue("LinkedInAccountName", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the LinkedIn account.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInAccountDisplayName
        {
            get
            {
                return GetStringValue("LinkedInAccountDisplayName", String.Empty);
            }
            set
            {         
                SetValue("LinkedInAccountDisplayName", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            LinkedInAccountInfoProvider.DeleteLinkedInAccountInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            LinkedInAccountInfoProvider.SetLinkedInAccountInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("LinkedInAccountAccessTokenSecret", StringComparison.InvariantCultureIgnoreCase))
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
            if (columnName.EqualsCSafe("LinkedInAccountAccessTokenSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string) value);
            }
            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public LinkedInAccountInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the LinkedInAccountInfo class.
        /// </summary>
        public LinkedInAccountInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the LinkedInAccountInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public LinkedInAccountInfo(DataRow dr) : base(TYPEINFO, dr)
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