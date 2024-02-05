using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ShippingCostInfo), ShippingCostInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ShippingCostInfo data container class.
    /// </summary>
    public class ShippingCostInfo : AbstractInfo<ShippingCostInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.shippingcost";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ShippingCostInfoProvider), OBJECT_TYPE, "Ecommerce.ShippingCost", "ShippingCostID", "ShippingCostLastModified", "ShippingCostGUID", null, null, null, null, "ShippingCostShippingOptionID", ShippingOptionInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  // - None

                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = true,
                                                  TouchCacheDependencies = true,
                                                  AllowRestore = false,
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
        /// Shipping cost unique identifier.
        /// </summary>
        public virtual Guid ShippingCostGUID
        {
            get
            {
                return GetGuidValue("ShippingCostGUID", Guid.Empty);
            }
            set
            {
                SetValue("ShippingCostGUID", value);
            }
        }


        /// <summary>
        /// Date and time when the shipping cost was last modified.
        /// </summary>
        public virtual DateTime ShippingCostLastModified
        {
            get
            {
                return GetDateTimeValue("ShippingCostLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ShippingCostLastModified", value);
            }
        }


        /// <summary>
        /// Order minimal weight the shipping cost is applied for.
        /// </summary>
        public virtual double ShippingCostMinWeight
        {
            get
            {
                return GetDoubleValue("ShippingCostMinWeight", 0.0);
            }
            set
            {
                SetValue("ShippingCostMinWeight", value);
            }
        }


        /// <summary>
        /// ID of the shipping option the shipping cost belongs to.
        /// </summary>
        public virtual int ShippingCostShippingOptionID
        {
            get
            {
                return GetIntegerValue("ShippingCostShippingOptionID", 0);
            }
            set
            {
                SetValue("ShippingCostShippingOptionID", value);
            }
        }


        /// <summary>
        /// Shipping cost value.
        /// </summary>
        public virtual decimal ShippingCostValue
        {
            get
            {
                return GetDecimalValue("ShippingCostValue", 0m);
            }
            set
            {
                SetValue("ShippingCostValue", value);
            }
        }


        /// <summary>
        /// Shipping cost ID.
        /// </summary>
        public virtual int ShippingCostID
        {
            get
            {
                return GetIntegerValue("ShippingCostID", 0);
            }
            set
            {
                SetValue("ShippingCostID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ShippingCostInfoProvider.DeleteShippingCostInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ShippingCostInfoProvider.SetShippingCostInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ShippingCostInfo object.
        /// </summary>
        public ShippingCostInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ShippingCostInfo object from the given DataRow.
        /// </summary>
        public ShippingCostInfo(DataRow dr)
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