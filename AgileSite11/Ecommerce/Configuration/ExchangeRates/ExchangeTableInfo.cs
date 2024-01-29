using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ExchangeTableInfo), ExchangeTableInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ExchangeTableInfo data container class.
    /// </summary>
    public class ExchangeTableInfo : AbstractInfo<ExchangeTableInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.exchangetable";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExchangeTableInfoProvider), OBJECT_TYPE, "ECommerce.ExchangeTable", "ExchangeTableID", "ExchangeTableLastModified", "ExchangeTableGUID", null, "ExchangeTableDisplayName", null, "ExchangeTableSiteID", null, null)
        {
            // Child object types
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
            // Others
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
                Enabled = true,
                IdentificationField = "ExchangeTableGUID",
                ObjectFileNameFields = { "ExchangeTableDisplayName" }
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Valid from.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ExchangeTableValidFrom
        {
            get
            {
                return GetDateTimeValue("ExchangeTableValidFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExchangeTableValidFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// ID of the exchange table.
        /// </summary>
        [DatabaseField]
        public virtual int ExchangeTableID
        {
            get
            {
                return GetIntegerValue("ExchangeTableID", 0);
            }
            set
            {
                SetValue("ExchangeTableID", value);
            }
        }


        /// <summary>
        /// Table display name.
        /// </summary>
        [DatabaseField]
        public virtual string ExchangeTableDisplayName
        {
            get
            {
                return GetStringValue("ExchangeTableDisplayName", "");
            }
            set
            {
                SetValue("ExchangeTableDisplayName", value);
            }
        }


        /// <summary>
        /// Valid to.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ExchangeTableValidTo
        {
            get
            {
                return GetDateTimeValue("ExchangeTableValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExchangeTableValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Exchange table GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ExchangeTableGUID
        {
            get
            {
                return GetGuidValue("ExchangeTableGUID", Guid.Empty);
            }
            set
            {
                SetValue("ExchangeTableGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ExchangeTableLastModified
        {
            get
            {
                return GetDateTimeValue("ExchangeTableLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExchangeTableLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Exchange table site ID. Set to 0 for global exchange table.
        /// </summary>
        [DatabaseField]
        public virtual int ExchangeTableSiteID
        {
            get
            {
                return GetIntegerValue("ExchangeTableSiteID", 0);
            }
            set
            {
                SetValue("ExchangeTableSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Rate from global main currency to site main currency.
        /// </summary>
		[DatabaseField]
        public virtual decimal ExchangeTableRateFromGlobalCurrency
        {
            get
            {
                return GetDecimalValue("ExchangeTableRateFromGlobalCurrency", 0m);
            }
            set
            {
                SetValue("ExchangeTableRateFromGlobalCurrency", value, (value > 0m));
            }
        }
        
        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExchangeTableInfoProvider.DeleteExchangeTableInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExchangeTableInfoProvider.SetExchangeTableInfo(this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            DateTime validFrom = ExchangeTableValidFrom;
            DateTime validTo = ExchangeTableValidTo;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                validFrom = ValidationHelper.GetDateTime(p["ecommerce.exchangetable" + ".validfrom"], ExchangeTableValidFrom);
                validTo = ValidationHelper.GetDateTime(p["ecommerce.exchangetable" + ".validto"], ExchangeTableValidTo);
            }

            ExchangeTableValidFrom = validFrom;
            ExchangeTableValidTo = validTo;

            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ExchangeTableInfo object.
        /// </summary>
        public ExchangeTableInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ExchangeTableInfo object from the given DataRow.
        /// </summary>
        public ExchangeTableInfo(DataRow dr)
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