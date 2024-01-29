using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SupplierInfo), SupplierInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SupplierInfo data container class.
    /// </summary>
    public class SupplierInfo : AbstractInfo<SupplierInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.supplier";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SupplierInfoProvider), OBJECT_TYPE, "ECommerce.Supplier", "SupplierID", "SupplierLastModified", "SupplierGUID", "SupplierName", "SupplierDisplayName", null, "SupplierSiteID", null, null)
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
            NameGloballyUnique = true,
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
            EnabledColumn = "SupplierEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Fax of the supplier.
        /// </summary>
        public virtual string SupplierFax
        {
            get
            {
                return GetStringValue("SupplierFax", "");
            }
            set
            {
                SetValue("SupplierFax", value);
            }
        }


        /// <summary>
        /// E-mail of the supplier.
        /// </summary>
        public virtual string SupplierEmail
        {
            get
            {
                return GetStringValue("SupplierEmail", "");
            }
            set
            {
                SetValue("SupplierEmail", value);
            }
        }


        /// <summary>
        /// ID of the supplier.
        /// </summary>
        public virtual int SupplierID
        {
            get
            {
                return GetIntegerValue("SupplierID", 0);
            }
            set
            {
                SetValue("SupplierID", value);
            }
        }


        /// <summary>
        /// Phone of the supplier.
        /// </summary>
        public virtual string SupplierPhone
        {
            get
            {
                return GetStringValue("SupplierPhone", "");
            }
            set
            {
                SetValue("SupplierPhone", value);
            }
        }


        /// <summary>
        /// Supplier display name.
        /// </summary>
        public virtual string SupplierDisplayName
        {
            get
            {
                return GetStringValue("SupplierDisplayName", "");
            }
            set
            {
                SetValue("SupplierDisplayName", value);
            }
        }


        /// <summary>
        /// Supplier code name.
        /// </summary>
        public virtual string SupplierName
        {
            get
            {
                return GetStringValue("SupplierName", "");
            }
            set
            {
                SetValue("SupplierName", value);
            }
        }


        /// <summary>
        /// Supplier enabled.
        /// </summary>
        public virtual bool SupplierEnabled
        {
            get
            {
                return GetBooleanValue("SupplierEnabled", false);
            }
            set
            {
                SetValue("SupplierEnabled", value);
            }
        }


        /// <summary>
        /// Supplier GUID.
        /// </summary>
        public virtual Guid SupplierGUID
        {
            get
            {
                return GetGuidValue("SupplierGUID", Guid.Empty);
            }
            set
            {
                SetValue("SupplierGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime SupplierLastModified
        {
            get
            {
                return GetDateTimeValue("SupplierLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SupplierLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Supplier site ID. Set to 0 for global supplier.
        /// </summary>
        public virtual int SupplierSiteID
        {
            get
            {
                return GetIntegerValue("SupplierSiteID", 0);
            }
            set
            {
                SetValue("SupplierSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SupplierInfoProvider.DeleteSupplierInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SupplierInfoProvider.SetSupplierInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SupplierInfo object.
        /// </summary>
        public SupplierInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SupplierInfo object from the given DataRow.
        /// </summary>
        public SupplierInfo(DataRow dr)
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
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.SUPPLIERS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return SupplierInfoProvider.IsUserAuthorizedToModifySupplier(IsGlobal, siteName, userInfo, exceptionOnFailure);
                    
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}