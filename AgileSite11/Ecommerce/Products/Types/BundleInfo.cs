using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(BundleInfo), BundleInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// BundleInfo data container class.
    /// </summary>
    public class BundleInfo : AbstractInfo<BundleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.bundle";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BundleInfoProvider), OBJECT_TYPE, "ECommerce.Bundle", null, null, null, null, null, null, null, "BundleID", SKUInfo.OBJECT_TYPE_SKU)
        {
            // Child object types
            // - None

            // Object dependencies
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("SKUID", SKUInfo.OBJECT_TYPE_SKU, ObjectDependencyEnum.Binding) },
            // Binding object types
            // - None

            // Others
            LogEvents = false,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            RegisterAsBindingToObjectTypes = new List<string> { SKUInfo.OBJECT_TYPE_SKU, SKUInfo.OBJECT_TYPE_VARIANT },
            ImportExportSettings = { LogExport = false },
            ContinuousIntegrationSettings = { Enabled = true }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Bundle ID.
        /// </summary>
        public virtual int BundleID
        {
            get
            {
                return GetIntegerValue("BundleID", 0);
            }
            set
            {
                SetValue("BundleID", value);
            }
        }


        /// <summary>
        /// SKU ID.
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
            BundleInfoProvider.DeleteBundleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BundleInfoProvider.SetBundleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BundleInfo object.
        /// </summary>
        public BundleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BundleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public BundleInfo(DataRow dr)
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