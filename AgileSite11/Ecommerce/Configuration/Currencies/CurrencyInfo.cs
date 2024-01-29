using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;
using System.Diagnostics;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(CurrencyInfo), CurrencyInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// CurrencyInfo data container class.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(CurrencyCode) + ",nq}")]
    public class CurrencyInfo : AbstractInfo<CurrencyInfo>
    {
        #region "Constants"

        /// <summary>
        /// Default number of decimals for rounding
        /// </summary>
        private const int DEFAULT_ROUNDING_DECIMALS = 2;

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.currency";


        /// <summary>
        /// Type information
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CurrencyInfoProvider), OBJECT_TYPE, "ECommerce.Currency", "CurrencyID", "CurrencyLastModified", "CurrencyGUID", "CurrencyName", "CurrencyDisplayName", null, "CurrencySiteID", null, null)
        {
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
            SupportsInvalidation = true,
            SupportsGlobalObjects = true,
            SupportsCloning = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                }
            },
            EnabledColumn = "CurrencyEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Currency display name.
        /// </summary>
        [DatabaseField]
        public virtual string CurrencyDisplayName
        {
            get
            {
                return GetStringValue("CurrencyDisplayName", "");
            }
            set
            {
                SetValue("CurrencyDisplayName", value);
            }
        }


        /// <summary>
        /// Currency code name.
        /// </summary>
        [DatabaseField]
        public virtual string CurrencyName
        {
            get
            {
                return GetStringValue("CurrencyName", "");
            }
            set
            {
                SetValue("CurrencyName", value);
            }
        }


        /// <summary>
        /// Currency code.
        /// </summary>
        [DatabaseField]
        public virtual string CurrencyCode
        {
            get
            {
                return GetStringValue("CurrencyCode", "");
            }
            set
            {
                SetValue("CurrencyCode", value);
            }
        }


        /// <summary>
        /// Currency ID.
        /// </summary>
        [DatabaseField]
        public virtual int CurrencyID
        {
            get
            {
                return GetIntegerValue("CurrencyID", 0);
            }
            set
            {
                SetValue("CurrencyID", value);
            }
        }


        /// <summary>
        /// Indicates if currency is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CurrencyEnabled
        {
            get
            {
                return GetBooleanValue("CurrencyEnabled", true);
            }
            set
            {
                SetValue("CurrencyEnabled", value);
            }
        }


        /// <summary>
        /// Indicates if currency is main currency.
        /// </summary>
        [DatabaseField]
        public virtual bool CurrencyIsMain
        {
            get
            {
                return GetBooleanValue("CurrencyIsMain", true);
            }
            set
            {
                SetValue("CurrencyIsMain", value);
            }
        }


        /// <summary>
        /// Currency formatting string.
        /// </summary>
        [DatabaseField]
        public virtual string CurrencyFormatString
        {
            get
            {
                return GetStringValue("CurrencyFormatString", "");
            }
            set
            {
                SetValue("CurrencyFormatString", value);
            }
        }


        /// <summary>
        /// Number of digits the price in this currency is rounded to.
        /// </summary>
        [DatabaseField]
        public virtual int CurrencyRoundTo
        {
            get
            {
                return GetIntegerValue("CurrencyRoundTo", DEFAULT_ROUNDING_DECIMALS);
            }
            set
            {
                SetValue("CurrencyRoundTo", value);
            }
        }


        /// <summary>
        /// Currency GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid CurrencyGUID
        {
            get
            {
                return GetGuidValue("CurrencyGUID", Guid.Empty);
            }
            set
            {
                SetValue("CurrencyGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Date and time when the currency was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CurrencyLastModified
        {
            get
            {
                return GetDateTimeValue("CurrencyLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CurrencyLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Currency site ID. Set to 0 for global currency.
        /// </summary>
        [DatabaseField]
        public virtual int CurrencySiteID
        {
            get
            {
                return GetIntegerValue("CurrencySiteID", 0);
            }
            set
            {
                SetValue("CurrencySiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CurrencyInfoProvider.DeleteCurrencyInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CurrencyInfoProvider.SetCurrencyInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns price formatted according to settings in this currency.
        /// </summary>
        /// <param name="price">Price to be formatted.</param>
        public string FormatPrice(decimal price)
        {
            return CurrencyInfoProvider.GetFormattedPrice(price, this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Cloned currency should be never main
            CurrencyIsMain = false;
            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public CurrencyInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CurrencyInfo object.
        /// </summary>
        public CurrencyInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CurrencyInfo object from the given DataRow.
        /// </summary>
        public CurrencyInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            CurrencyIsMain = false;
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