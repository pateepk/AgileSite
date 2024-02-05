using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SKUOptionCategoryInfo), SKUOptionCategoryInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKUOptionCategoryInfo data container class.
    /// </summary>
    public class SKUOptionCategoryInfo : AbstractInfo<SKUOptionCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.skuoptioncategory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SKUOptionCategoryInfoProvider), OBJECT_TYPE, "ECommerce.SKUOptionCategory", "SKUCategoryID", null, null, null, null, null, null, "SKUID", SKUInfo.OBJECT_TYPE_SKU)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("CategoryID", OptionCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  AllowRestore = false,
                                                  TouchCacheDependencies = true,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  OrderColumn = "SKUCategoryOrder",
                                                  IsBinding = true,
                                                  RegisterAsBindingToObjectTypes = new List<string>() { SKUInfo.OBJECT_TYPE_SKU },
                                                  ImportExportSettings = { LogExport = false },
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Unique identifier of the relationship between SKU and product option category.
        /// </summary>
        public virtual int SKUCategoryID
        {
            get
            {
                return GetIntegerValue("SKUCategoryID", 0);
            }
            set
            {
                SetValue("SKUCategoryID", value);
            }
        }


        /// <summary>
        /// ID of the SKU.
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


        /// <summary>
        /// ID of the product option category.
        /// </summary>
        public virtual int CategoryID
        {
            get
            {
                return GetIntegerValue("CategoryID", 0);
            }
            set
            {
                SetValue("CategoryID", value);
            }
        }


        /// <summary>
        /// Indicates if all product options from the given product option category are allowed for the given SKU. By default it is True.
        /// </summary>
        public virtual bool AllowAllOptions
        {
            get
            {
                return GetBooleanValue("AllowAllOptions", true);
            }
            set
            {
                SetValue("AllowAllOptions", value);
            }
        }


        /// <summary>
        /// Option category order within the parent product.
        /// </summary>
        public virtual int SKUCategoryOrder
        {
            get
            {
                return GetIntegerValue("SKUCategoryOrder", 0);
            }
            set
            {
                SetValue("SKUCategoryOrder", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SKUOptionCategoryInfoProvider.DeleteSKUOptionCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SKUOptionCategoryInfoProvider.SetSKUOptionCategoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SKUOptionCategoryInfo object.
        /// </summary>
        public SKUOptionCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SKUOptionCategoryInfo object from the given DataRow.
        /// </summary>
        public SKUOptionCategoryInfo(DataRow dr)
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