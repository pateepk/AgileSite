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

[assembly: RegisterObjectType(typeof(ContactStatusInfo), ContactStatusInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactStatusInfo data container class.
    /// </summary>
    public class ContactStatusInfo : AbstractInfo<ContactStatusInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.contactstatus";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactStatusInfoProvider), OBJECT_TYPE, "OM.ContactStatus", "ContactStatusID", null, null, 
            "ContactStatusName", "ContactStatusDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING)
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
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING)
                },
            },
            Feature = FeatureEnum.FullContactManagement
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or set the contact status description.
        /// </summary>
        public virtual string ContactStatusDescription
        {
            get
            {
                return GetStringValue("ContactStatusDescription", "");
            }
            set
            {
                SetValue("ContactStatusDescription", value);
            }
        }


        /// <summary>
        /// Gets or set the contact status name.
        /// </summary>
        public virtual string ContactStatusName
        {
            get
            {
                return GetStringValue("ContactStatusName", "");
            }
            set
            {
                SetValue("ContactStatusName", value);
            }
        }


        /// <summary>
        /// Gets or set the contact status display name.
        /// </summary>
        public virtual string ContactStatusDisplayName
        {
            get
            {
                return GetStringValue("ContactStatusDisplayName", "");
            }
            set
            {
                SetValue("ContactStatusDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or set the contact status ID.
        /// </summary>
        public virtual int ContactStatusID
        {
            get
            {
                return GetIntegerValue("ContactStatusID", 0);
            }
            set
            {
                SetValue("ContactStatusID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactStatusInfoProvider.DeleteContactStatusInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactStatusInfoProvider.SetContactStatusInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactStatusInfo object.
        /// </summary>
        public ContactStatusInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactStatusInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactStatusInfo(DataRow dr)
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