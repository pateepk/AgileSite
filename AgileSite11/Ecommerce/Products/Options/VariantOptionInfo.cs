using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Base;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(VariantOptionInfo), VariantOptionInfo.OBJECT_TYPE)]


namespace CMS.Ecommerce
{
    /// <summary>
    /// VariantOptionInfo data container class. Represents binding between variant of product and product option which the respective variant contains.
    /// </summary>
    public class VariantOptionInfo : AbstractInfo<VariantOptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.variantoption";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VariantOptionInfoProvider), OBJECT_TYPE, "ECommerce.VariantOption", null, null, null, null, null, null, null, "VariantSKUID", SKUInfo.OBJECT_TYPE_VARIANT)
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
        /// SKUID of the variant.
        /// </summary>
        public virtual int VariantSKUID
        {
            get
            {
                return GetIntegerValue("VariantSKUID", 0);
            }
            set
            {
                SetValue("VariantSKUID", value);
            }
        }


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
        
        #endregion


        #region "Constants"

        /// <summary>
        /// Constant for newly added option in variant.
        /// </summary>
        public static readonly int NewOption = 0;
        

        /// <summary>
        /// Constant for selected and existing option in variant.
        /// </summary>
        public static readonly int ExistingSelectedOption = -1;
        
        
        /// <summary>
        /// Constant for unselected option in variant.
        /// </summary>
        public static readonly int ExistingUnselectedOption = -2;

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VariantOptionInfoProvider.DeleteVariantOptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VariantOptionInfoProvider.SetVariantOptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty VariantOptionInfo object.
        /// </summary>
        public VariantOptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new VariantOptionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public VariantOptionInfo(DataRow dr)
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
