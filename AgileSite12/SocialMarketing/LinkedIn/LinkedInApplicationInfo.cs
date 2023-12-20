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

[assembly: RegisterObjectType(typeof(LinkedInApplicationInfo), LinkedInApplicationInfo.OBJECT_TYPE)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a LinkedIn application.
    /// </summary>
    [Serializable]
    public class LinkedInApplicationInfo : AbstractInfo<LinkedInApplicationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sm.linkedinapplication";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LinkedInApplicationInfoProvider), OBJECT_TYPE, "SM.LinkedInApplication", "LinkedInApplicationID", "LinkedInApplicationLastModified", "LinkedInApplicationGUID", "LinkedInApplicationName", "LinkedInApplicationDisplayName", null, "LinkedInApplicationSiteID", null, null)
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
            ModuleName = ModuleName.SOCIALMARKETING,
            SensitiveColumns = new List<string> { "LinkedInApplicationConsumerKey", "LinkedInApplicationConsumerSecret" },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the consumer key of the LinkedIn application.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInApplicationConsumerKey
        {
            get
            {
                return GetStringValue("LinkedInApplicationConsumerKey", String.Empty);
            }
            set
            {
                SetValue("LinkedInApplicationConsumerKey", value);
            }
        }


        /// <summary>
        /// Gets or sets the consumer secret of the LinkedIn application.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInApplicationConsumerSecret
        {
            get
            {
                return GetStringValue("LinkedInApplicationConsumerSecret", String.Empty);
            }
            set
            {
                SetValue("LinkedInApplicationConsumerSecret", value);
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Gets or sets the identifier of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInApplicationID
        {
            get
            {
                return GetIntegerValue("LinkedInApplicationID", 0);
            }
            set
            {
                SetValue("LinkedInApplicationID", value);
            }
        }


        /// <summary>
        /// Gets or sets the display name of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInApplicationDisplayName
        {
            get
            {
                return GetStringValue("LinkedInApplicationDisplayName", String.Empty);
            }
            set
            {
                SetValue("LinkedInApplicationDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the code name of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual string LinkedInApplicationName
        {
            get
            {
                return GetStringValue("LinkedInApplicationName", String.Empty);
            }
            set
            {
                SetValue("LinkedInApplicationName", value);
            }
        }


        /// <summary>
        /// Gets or sets the timestamp of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LinkedInApplicationLastModified
        {
            get
            {
                return GetDateTimeValue("LinkedInApplicationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LinkedInApplicationLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the globally unique identifier of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual Guid LinkedInApplicationGUID
        {
            get
            {
                return GetGuidValue("LinkedInApplicationGUID", Guid.Empty);
            }
            set
            {
                SetValue("LinkedInApplicationGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site identifier of the LinkedInApplicationInfo object.
        /// </summary>
        [DatabaseField]
        public virtual int LinkedInApplicationSiteID
        {
            get
            {
                return GetIntegerValue("LinkedInApplicationSiteID", 0);
            }
            set
            {
                SetValue("LinkedInApplicationSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            LinkedInApplicationInfoProvider.DeleteLinkedInApplicationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            LinkedInApplicationInfoProvider.SetLinkedInApplicationInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.EqualsCSafe("LinkedInApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
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
            if (columnName.EqualsCSafe("LinkedInApplicationConsumerSecret", StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string)value);
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
        public LinkedInApplicationInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty LinkedInApplicationInfo object.
        /// </summary>
        public LinkedInApplicationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new LinkedInApplicationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public LinkedInApplicationInfo(DataRow dr)
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