using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(AccountStatusInfo), AccountStatusInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// AccountStatusInfo data container class.
    /// </summary>
    public class AccountStatusInfo : AbstractInfo<AccountStatusInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.accountstatus";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AccountStatusInfoProvider), OBJECT_TYPE, "OM.AccountStatus", "AccountStatusID", null, null, "AccountStatusName", "AccountStatusDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.CONTACTMANAGEMENT,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING),
                },
            },
            Feature = FeatureEnum.FullContactManagement
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the account status display name.
        /// </summary>
        public virtual string AccountStatusDisplayName
        {
            get
            {
                return GetStringValue("AccountStatusDisplayName", "");
            }
            set
            {
                SetValue("AccountStatusDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the account status ID.
        /// </summary>
        public virtual int AccountStatusID
        {
            get
            {
                return GetIntegerValue("AccountStatusID", 0);
            }
            set
            {
                SetValue("AccountStatusID", value);
            }
        }


        /// <summary>
        /// Gets or sets the account status name.
        /// </summary>
        public virtual string AccountStatusName
        {
            get
            {
                return GetStringValue("AccountStatusName", "");
            }
            set
            {
                SetValue("AccountStatusName", value);
            }
        }


        /// <summary>
        /// Gets or sets the account status description.
        /// </summary>
        public virtual string AccountStatusDescription
        {
            get
            {
                return GetStringValue("AccountStatusDescription", "");
            }
            set
            {
                SetValue("AccountStatusDescription", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AccountStatusInfoProvider.DeleteAccountStatusInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AccountStatusInfoProvider.SetAccountStatusInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AccountStatusInfo object.
        /// </summary>
        public AccountStatusInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AccountStatusInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public AccountStatusInfo(DataRow dr)
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
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, "ReadConfiguration", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, "ModifyConfiguration", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}