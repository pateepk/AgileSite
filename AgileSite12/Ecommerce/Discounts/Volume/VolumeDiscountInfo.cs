using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(VolumeDiscountInfo), VolumeDiscountInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// VolumeDiscountInfo data container class.
    /// </summary>
    [Serializable]
    public class VolumeDiscountInfo : AbstractInfo<VolumeDiscountInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.volumediscount";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VolumeDiscountInfoProvider), OBJECT_TYPE, "ECommerce.VolumeDiscount", "VolumeDiscountID", "VolumeDiscountLastModified", "VolumeDiscountGUID", null, null, null, null, "VolumeDiscountSKUID", SKUInfo.OBJECT_TYPE_SKU)
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
            RegisterAsChildToObjectTypes = new List<string> { SKUInfo.OBJECT_TYPE_SKU, SKUInfo.OBJECT_TYPE_OPTIONSKU, SKUInfo.OBJECT_TYPE_VARIANT },
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the discount.
        /// </summary>
        public virtual int VolumeDiscountID
        {
            get
            {
                return GetIntegerValue("VolumeDiscountID", 0);
            }
            set
            {
                SetValue("VolumeDiscountID", value);
            }
        }


        /// <summary>
        /// Value of the discount.
        /// </summary>
        public virtual decimal VolumeDiscountValue
        {
            get
            {
                return GetDecimalValue("VolumeDiscountValue", 0m);
            }
            set
            {
                SetValue("VolumeDiscountValue", value);
            }
        }


        /// <summary>
        /// Minimal number of objects for the discount to be applied.
        /// </summary>
        public virtual int VolumeDiscountMinCount
        {
            get
            {
                return GetIntegerValue("VolumeDiscountMinCount", 0);
            }
            set
            {
                SetValue("VolumeDiscountMinCount", value);
            }
        }


        /// <summary>
        /// ID of the SKU for which the discount is applied.
        /// </summary>
        public virtual int VolumeDiscountSKUID
        {
            get
            {
                return GetIntegerValue("VolumeDiscountSKUID", 0);
            }
            set
            {
                SetValue("VolumeDiscountSKUID", value);
            }
        }


        /// <summary>
        /// Indicates whether the discount is flat or not.
        /// </summary>
        public virtual bool VolumeDiscountIsFlatValue
        {
            get
            {
                return GetBooleanValue("VolumeDiscountIsFlatValue", false);
            }
            set
            {
                SetValue("VolumeDiscountIsFlatValue", value);
            }
        }


        /// <summary>
        /// GUID of the discount.
        /// </summary>
        public virtual Guid VolumeDiscountGUID
        {
            get
            {
                return GetGuidValue("VolumeDiscountGUID", Guid.Empty);
            }
            set
            {
                SetValue("VolumeDiscountGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime VolumeDiscountLastModified
        {
            get
            {
                return GetDateTimeValue("VolumeDiscountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VolumeDiscountLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Discount display name. Used for discount calculation in the shopping cart.
        /// </summary>
        public string ItemDiscountDisplayName => String.Format(ResHelper.GetString("ProductPriceDetail.VolumeDiscount"), VolumeDiscountMinCount);

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VolumeDiscountInfoProvider.DeleteVolumeDiscountInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VolumeDiscountInfoProvider.SetVolumeDiscountInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty VolumeDiscountInfo object.
        /// </summary>
        public VolumeDiscountInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new VolumeDiscountInfo object from the given DataRow.
        /// </summary>
        public VolumeDiscountInfo(DataRow dr)
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