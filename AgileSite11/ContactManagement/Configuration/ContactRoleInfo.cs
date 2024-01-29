using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(ContactRoleInfo), ContactRoleInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactRoleInfo data container class.
    /// </summary>
    public class ContactRoleInfo : AbstractInfo<ContactRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.contactrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactRoleInfoProvider), OBJECT_TYPE, "OM.ContactRole", "ContactRoleID", null, null, "ContactRoleName", "ContactRoleDisplayName", null, null, null, null)
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
        /// Gets or sets the contact role description.
        /// </summary>
        public virtual string ContactRoleDescription
        {
            get
            {
                return GetStringValue("ContactRoleDescription", "");
            }
            set
            {
                SetValue("ContactRoleDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact role display name.
        /// </summary>
        public virtual string ContactRoleDisplayName
        {
            get
            {
                return GetStringValue("ContactRoleDisplayName", "");
            }
            set
            {
                SetValue("ContactRoleDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact role name.
        /// </summary>
        public virtual string ContactRoleName
        {
            get
            {
                return GetStringValue("ContactRoleName", "");
            }
            set
            {
                SetValue("ContactRoleName", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact role ID.
        /// </summary>
        public virtual int ContactRoleID
        {
            get
            {
                return GetIntegerValue("ContactRoleID", 0);
            }
            set
            {
                SetValue("ContactRoleID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactRoleInfoProvider.DeleteContactRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactRoleInfoProvider.SetContactRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactRoleInfo object.
        /// </summary>
        public ContactRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactRoleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactRoleInfo(DataRow dr)
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