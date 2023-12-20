using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(InternalStatusInfo), InternalStatusInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// InternalStatusInfo data container class.
    /// </summary>
    public class InternalStatusInfo : AbstractInfo<InternalStatusInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.internalstatus";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(InternalStatusInfoProvider), OBJECT_TYPE, "ECommerce.InternalStatus", "InternalStatusID", "InternalStatusLastModified", "InternalStatusGUID", "InternalStatusName", "InternalStatusDisplayName", null, "InternalStatusSiteID", null, null)
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
            EnabledColumn = "InternalStatusEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether the status is enabled or not.
        /// </summary>
        public virtual bool InternalStatusEnabled
        {
            get
            {
                return GetBooleanValue("InternalStatusEnabled", false);
            }
            set
            {
                SetValue("InternalStatusEnabled", value);
            }
        }


        /// <summary>
        /// ID of the status.
        /// </summary>
        public virtual int InternalStatusID
        {
            get
            {
                return GetIntegerValue("InternalStatusID", 0);
            }
            set
            {
                SetValue("InternalStatusID", value);
            }
        }


        /// <summary>
        /// Code name of the status.
        /// </summary>
        public virtual string InternalStatusName
        {
            get
            {
                return GetStringValue("InternalStatusName", "");
            }
            set
            {
                SetValue("InternalStatusName", value);
            }
        }


        /// <summary>
        /// Display name of the status.
        /// </summary>
        public virtual string InternalStatusDisplayName
        {
            get
            {
                return GetStringValue("InternalStatusDisplayName", "");
            }
            set
            {
                SetValue("InternalStatusDisplayName", value);
            }
        }


        /// <summary>
        /// InternalStatus GUID.
        /// </summary>
        public virtual Guid InternalStatusGUID
        {
            get
            {
                return GetGuidValue("InternalStatusGUID", Guid.Empty);
            }
            set
            {
                SetValue("InternalStatusGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime InternalStatusLastModified
        {
            get
            {
                return GetDateTimeValue("InternalStatusLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("InternalStatusLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Internal status site ID. Set to 0 for global internal status.
        /// </summary>
        public virtual int InternalStatusSiteID
        {
            get
            {
                return GetIntegerValue("InternalStatusSiteID", 0);
            }
            set
            {
                SetValue("InternalStatusSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            InternalStatusInfoProvider.DeleteInternalStatusInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            InternalStatusInfoProvider.SetInternalStatusInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty InternalStatusInfo object.
        /// </summary>
        public InternalStatusInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new InternalStatusInfo object from the given DataRow.
        /// </summary>
        public InternalStatusInfo(DataRow dr)
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