using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SKUAllowedOptionInfo), SKUAllowedOptionInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKUAllowedOptionInfo data container class. Represents binding between product and product option which is suitable for given product.
    /// </summary>
    public class SKUAllowedOptionInfo : AbstractInfo<SKUAllowedOptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.skuallowedoption";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SKUAllowedOptionInfoProvider), OBJECT_TYPE, "ECommerce.SKUAllowedOption", null, null, null, null, null, null, null, "SKUID", SKUInfo.OBJECT_TYPE_SKU)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("OptionSKUID", SKUInfo.OBJECT_TYPE_OPTIONSKU, ObjectDependencyEnum.Binding) },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  ImportExportSettings = { LogExport = false },
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// SKUID of the product option.
        /// </summary>
        public virtual int OptionSKUID
        {
            get
            {
                return GetIntegerValue("OptionSKUID", 0);
            }
            set
            {
                SetValue("OptionSKUID", value);
            }
        }


        /// <summary>
        /// SKUID of the product.
        /// </summary>
        public virtual int SKUID
        {
            get
            {
                return GetIntegerValue("SKUID", 0);
            }
            set
            {
                SetValue("SKUID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SKUAllowedOptionInfoProvider.DeleteSKUOptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SKUAllowedOptionInfoProvider.SetSKUOptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SKUAllowedOptionInfo object.
        /// </summary>
        public SKUAllowedOptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SKUAllowedOptionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SKUAllowedOptionInfo(DataRow dr)
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
            return EcommercePermissions.CheckProductsPermissions(permission, siteName, userInfo, exceptionOnFailure, IsGlobal, base.CheckPermissionsInternal);
        }

        #endregion
    }
}
