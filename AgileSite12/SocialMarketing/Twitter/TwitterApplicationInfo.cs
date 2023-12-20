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

[assembly: RegisterObjectType(typeof(TwitterApplicationInfo), TwitterApplicationInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Represents a Twitter application.
    /// </summary>
    public class TwitterApplicationInfo : AbstractInfo<TwitterApplicationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.twitterapplication";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TwitterApplicationInfoProvider), OBJECT_TYPE, "SM.TwitterApplication", "TwitterApplicationID", "TwitterApplicationLastModified", "TwitterApplicationGUID", "TwitterApplicationName", "TwitterApplicationDisplayName", null, "TwitterApplicationSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY)
                }
            },
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            Feature = FeatureEnum.SocialMarketing,
            SensitiveColumns = new List<string> { "TwitterApplicationConsumerKey", "TwitterApplicationConsumerSecret" },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the consumer key of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterApplicationConsumerKey
        {
            get
            {
                return GetStringValue("TwitterApplicationConsumerKey", String.Empty);
            }
            set
            {
                SetValue("TwitterApplicationConsumerKey", value);
            }
        }


        /// <summary>
        /// Gets or sets the consumer secret of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterApplicationConsumerSecret
        {
            get
            {
                return GetStringValue("TwitterApplicationConsumerSecret", String.Empty);
            }
            set
            {         
                SetValue("TwitterApplicationConsumerSecret", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterApplicationID
        {
            get
            {
                return GetIntegerValue("TwitterApplicationID", 0);
            }
            set
            {
                SetValue("TwitterApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual int TwitterApplicationSiteID
        {
            get
            {
                return GetIntegerValue("TwitterApplicationSiteID", 0);
            }
            set
            {         
                SetValue("TwitterApplicationSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterApplicationName
        {
            get
            {
                return GetStringValue("TwitterApplicationName", String.Empty);
            }
            set
            {         
                SetValue("TwitterApplicationName", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual string TwitterApplicationDisplayName
        {
            get
            {
                return GetStringValue("TwitterApplicationDisplayName", String.Empty);
            }
            set
            {         
                SetValue("TwitterApplicationDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TwitterApplicationLastModified
        {
            get
            {
                return GetDateTimeValue("TwitterApplicationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("TwitterApplicationLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the globally unique identifier of the Twitter application.
        /// </summary>
        [DatabaseField]
        public virtual Guid TwitterApplicationGUID
        {
            get
            {
                return GetGuidValue("TwitterApplicationGUID", Guid.Empty);
            }
            set
            {         
                SetValue("TwitterApplicationGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            TwitterApplicationInfoProvider.DeleteTwitterApplicationInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            TwitterApplicationInfoProvider.SetTwitterApplicationInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("TwitterApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
            {
#pragma warning disable 618
                value = EncryptionHelper.EncryptData((string)value);
#pragma warning restore 618
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
            if (columnName.EqualsCSafe("TwitterApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string)value);
            }
            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterApplicationInfo class.
        /// </summary>
        public TwitterApplicationInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the TwitterApplicationInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TwitterApplicationInfo(DataRow dr) : base(TYPEINFO, dr)
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
                        UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "ModifyApplications", siteName, user, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion

    }

}