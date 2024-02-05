using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(PublicStatusInfo), PublicStatusInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// PublicStatusInfo data container class.
    /// </summary>
    public class PublicStatusInfo : AbstractInfo<PublicStatusInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.publicstatus";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PublicStatusInfoProvider), OBJECT_TYPE, "ECommerce.PublicStatus", "PublicStatusID", "PublicStatusLastModified", "PublicStatusGUID", "PublicStatusName", "PublicStatusDisplayName", null, "PublicStatusSiteID", null, null)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

            // Binding object types
            // - None

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                }
            },

            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsGlobalObjects = true,
            SupportsCloneToOtherSite = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                },
            },
            EnabledColumn = "PublicStatusEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of the status.
        /// </summary>
        public virtual string PublicStatusName
        {
            get
            {
                return GetStringValue("PublicStatusName", "");
            }
            set
            {
                SetValue("PublicStatusName", value);
            }
        }


        /// <summary>
        /// ID of the status.
        /// </summary>
        public virtual int PublicStatusID
        {
            get
            {
                return GetIntegerValue("PublicStatusID", 0);
            }
            set
            {
                SetValue("PublicStatusID", value);
            }
        }


        /// <summary>
        /// Indicates whether the status is enabled or not.
        /// </summary>
        public virtual bool PublicStatusEnabled
        {
            get
            {
                return GetBooleanValue("PublicStatusEnabled", false);
            }
            set
            {
                SetValue("PublicStatusEnabled", value);
            }
        }


        /// <summary>
        /// Display name of the status.
        /// </summary>
        public virtual string PublicStatusDisplayName
        {
            get
            {
                return GetStringValue("PublicStatusDisplayName", "");
            }
            set
            {
                SetValue("PublicStatusDisplayName", value);
            }
        }


        /// <summary>
        /// PublicStatus GUID.
        /// </summary>
        public virtual Guid PublicStatusGUID
        {
            get
            {
                return GetGuidValue("PublicStatusGUID", Guid.Empty);
            }
            set
            {
                SetValue("PublicStatusGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime PublicStatusLastModified
        {
            get
            {
                return GetDateTimeValue("PublicStatusLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PublicStatusLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Public status site ID. Set to 0 for global public status.
        /// </summary>
        public virtual int PublicStatusSiteID
        {
            get
            {
                return GetIntegerValue("PublicStatusSiteID", 0);
            }
            set
            {
                SetValue("PublicStatusSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PublicStatusInfoProvider.DeletePublicStatusInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PublicStatusInfoProvider.SetPublicStatusInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PublicStatusInfo object.
        /// </summary>
        public PublicStatusInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PublicStatusInfo object from the given DataRow.
        /// </summary>
        public PublicStatusInfo(DataRow dr)
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
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return EcommercePermissions.CheckConfigurationPermissions(permission, siteName, userInfo, exceptionOnFailure, IsGlobal, base.CheckPermissionsInternal);
        }

        #endregion
    }
}