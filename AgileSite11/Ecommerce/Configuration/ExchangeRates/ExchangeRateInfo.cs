using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ExchangeRateInfo), ExchangeRateInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ExchangeRateInfo data container class.
    /// </summary>
    public class ExchangeRateInfo : AbstractInfo<ExchangeRateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.exchangerate";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExchangeRateInfoProvider), OBJECT_TYPE, "ECommerce.ExchangeRate", "ExchagneRateID", "ExchangeRateLastModified", "ExchangeRateGUID", null, null, null, null, "ExchangeTableID", ExchangeTableInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("ExchangeRateToCurrencyID", CurrencyInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
                                                  // Binding object types
                                                  // - None
                                                  IsBinding = true,
                                                  // Others
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  SupportsCloning = false,
                                                  SupportsGlobalObjects = true,
                                                  SupportsCloneToOtherSite = false,
                                                  ImportExportSettings = { LogExport = false },
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

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
        /// ID of the exchange rate.
        /// </summary>
        [DatabaseField]
        public virtual int ExchagneRateID
        {
            get
            {
                return GetIntegerValue("ExchagneRateID", 0);
            }
            set
            {
                SetValue("ExchagneRateID", value);
            }
        }


        /// <summary>
        /// ID of the target currency.
        /// </summary>
        [DatabaseField]
        public virtual int ExchangeRateToCurrencyID
        {
            get
            {
                return GetIntegerValue("ExchangeRateToCurrencyID", 0);
            }
            set
            {
                SetValue("ExchangeRateToCurrencyID", value);
            }
        }


        /// <summary>
        /// Exchange rate value
        /// </summary>
		[DatabaseField]
        public virtual decimal ExchangeRateValue
        {
            get
            {
                return GetDecimalValue("ExchangeRateValue", 0m);
            }
            set
            {
                SetValue("ExchangeRateValue", value);
            }
        }


        /// <summary>
        /// Exchange rate GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ExchangeRateGUID
        {
            get
            {
                return GetGuidValue("ExchangeRateGUID", Guid.Empty);
            }
            set
            {
                SetValue("ExchangeRateGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ExchangeRateLastModified
        {
            get
            {
                return GetDateTimeValue("ExchangeRateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExchangeRateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExchangeRateInfoProvider.DeleteExchangeRateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExchangeRateInfoProvider.SetExchangeRateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ExchangeRateInfo object.
        /// </summary>
        public ExchangeRateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ExchangeRateInfo object from the given DataRow.
        /// </summary>
        public ExchangeRateInfo(DataRow dr)
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