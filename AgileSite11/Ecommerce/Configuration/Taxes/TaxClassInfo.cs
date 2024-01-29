using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(TaxClassInfo), TaxClassInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// TaxClassInfo data container class.
    /// </summary>
    public class TaxClassInfo : AbstractInfo<TaxClassInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.taxclass";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaxClassInfoProvider), OBJECT_TYPE, "ECommerce.TaxClass", "TaxClassID", "TaxClassLastModified", "TaxClassGUID", "TaxClassName", "TaxClassDisplayName", null, "TaxClassSiteID", null, null)
        {
            // Child object types
            // Object dependencies
            // - None

            // Binding object types
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
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Display name of the tax class.
        /// </summary>
        [DatabaseField]
        public virtual string TaxClassDisplayName
        {
            get
            {
                return GetStringValue("TaxClassDisplayName", "");
            }
            set
            {
                SetValue("TaxClassDisplayName", value);
            }
        }


        /// <summary>
        /// ID of the tax class.
        /// </summary>
        [DatabaseField]
        public virtual int TaxClassID
        {
            get
            {
                return GetIntegerValue("TaxClassID", 0);
            }
            set
            {
                SetValue("TaxClassID", value);
            }
        }


        /// <summary>
        /// Code name of the tax class.
        /// </summary>
        [DatabaseField]
        public virtual string TaxClassName
        {
            get
            {
                return GetStringValue("TaxClassName", "");
            }
            set
            {
                SetValue("TaxClassName", value);
            }
        }


        /// <summary>
        /// Zero tax if tax ID is supplied.
        /// </summary>
        [DatabaseField]
        public virtual bool TaxClassZeroIfIDSupplied
        {
            get
            {
                return GetBooleanValue("TaxClassZeroIfIDSupplied", false);
            }
            set
            {
                SetValue("TaxClassZeroIfIDSupplied", value);
            }
        }


        /// <summary>
        /// TaxClass GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid TaxClassGUID
        {
            get
            {
                return GetGuidValue("TaxClassGUID", Guid.Empty);
            }
            set
            {
                SetValue("TaxClassGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaxClassLastModified
        {
            get
            {
                return GetDateTimeValue("TaxClassLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TaxClassLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Tax class site ID. Set to 0 for global tax class.
        /// </summary>
        [DatabaseField]
        public virtual int TaxClassSiteID
        {
            get
            {
                return GetIntegerValue("TaxClassSiteID", 0);
            }
            set
            {
                SetValue("TaxClassSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject() => TaxClassInfoProvider.DeleteTaxClassInfo(this);


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject() => TaxClassInfoProvider.SetTaxClassInfo(this);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TaxClassInfo object.
        /// </summary>
        public TaxClassInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaxClassInfo object from the given DataRow.
        /// </summary>
        public TaxClassInfo(DataRow dr)
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