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

[assembly: RegisterObjectType(typeof(FacebookApplicationInfo), FacebookApplicationInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a Facebook application.
    /// </summary>
    public class FacebookApplicationInfo : AbstractInfo<FacebookApplicationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.facebookapplication";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FacebookApplicationInfoProvider), OBJECT_TYPE, "SM.FacebookApplication", "FacebookApplicationID", "FacebookApplicationLastModified", "FacebookApplicationGUID", "FacebookApplicationName", "FacebookApplicationDisplayName", null, "FacebookApplicationSiteID", null, null)
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
            SensitiveColumns = new List<string> { "FacebookApplicationConsumerKey", "FacebookApplicationConsumerSecret" },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the consumer key of the Facebook application.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookApplicationConsumerKey
        {
            get
            {
                return GetStringValue("FacebookApplicationConsumerKey", String.Empty);
            }
            set
            {
                SetValue("FacebookApplicationConsumerKey", value);
            }
        }


        /// <summary>
        /// Gets or sets the consumer secret of the Facebook application.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookApplicationConsumerSecret
        {
            get
            {
                return GetStringValue("FacebookApplicationConsumerSecret", String.Empty);
            }
            set
            {         
                SetValue("FacebookApplicationConsumerSecret", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookApplicationID
        {
            get
            {
                return GetIntegerValue("FacebookApplicationID", 0);
            }
            set
            {
                SetValue("FacebookApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual int FacebookApplicationSiteID
        {
            get
            {
                return GetIntegerValue("FacebookApplicationSiteID", 0);
            }
            set
            {         
                SetValue("FacebookApplicationSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookApplicationName
        {
            get
            {
                return GetStringValue("FacebookApplicationName", String.Empty);
            }
            set
            {         
                SetValue("FacebookApplicationName", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual string FacebookApplicationDisplayName
        {
            get
            {
                return GetStringValue("FacebookApplicationDisplayName", String.Empty);
            }
            set
            {         
                SetValue("FacebookApplicationDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FacebookApplicationLastModified
        {
            get
            {
                return GetDateTimeValue("FacebookApplicationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("FacebookApplicationLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the globally unique identifier of the FacebookApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual Guid FacebookApplicationGUID
        {
            get
            {
                return GetGuidValue("FacebookApplicationGUID", Guid.Empty);
            }
            set
            {         
                SetValue("FacebookApplicationGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes this object.
        /// </summary>
        protected override void DeleteObject()
        {
            FacebookApplicationInfoProvider.DeleteFacebookApplicationInfo(this);
        }


        /// <summary>
        /// Updates this object.
        /// </summary>
        protected override void SetObject()
        {
            FacebookApplicationInfoProvider.SetFacebookApplicationInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("FacebookApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
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
            if (columnName.EqualsCSafe("FacebookApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string)value);
            }
            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FacebookApplicationInfo class.
        /// </summary>
        public FacebookApplicationInfo() : base(TYPEINFO)
        {

        }


        /// <summary>
        /// Initializes a new instance of the FacebookApplicationInfo class with the specified data.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FacebookApplicationInfo(DataRow dr) : base(TYPEINFO, dr)
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
                        UserInfoProvider.IsAuthorizedPerResource(ModuleName.SOCIALMARKETING, "ModifyApplications", siteName, user, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }

}