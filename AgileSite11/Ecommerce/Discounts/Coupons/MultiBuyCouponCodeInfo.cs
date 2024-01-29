using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(MultiBuyCouponCodeInfo), MultiBuyCouponCodeInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// MultiBuyCouponCodeInfo data container class.
    /// </summary>
    [Serializable]
    public class MultiBuyCouponCodeInfo : AbstractInfo<MultiBuyCouponCodeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuycouponcode";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyCouponCodeInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyCouponCode", "MultiBuyCouponCodeID", "MultiBuyCouponCodeLastModified", "MultiBuyCouponCodeGUID", "MultiBuyCouponCodeCode", null, null, null, "MultiBuyCouponCodeMultiBuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

            // Binding object types
            // - None

            // Synchronization

            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            SupportsCloneToOtherSite = false,
            SupportsCloning = false,
            AllowRestore = false,
            ImportExportSettings = { LogExport = false },
            RegisterAsChildToObjectTypes = new List<string> { MultiBuyDiscountInfo.OBJECT_TYPE, MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "MultiBuyCouponCodeUseCount" }
            },
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Multi buy coupon code ID
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyCouponCodeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyCouponCodeID"), 0);
            }
            set
            {
                SetValue("MultiBuyCouponCodeID", value);
            }
        }


        /// <summary>
        /// Unique product coupon code.
        /// </summary>
        [DatabaseField]
        public virtual string MultiBuyCouponCodeCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MultiBuyCouponCodeCode"), String.Empty);
            }
            set
            {
                SetValue("MultiBuyCouponCodeCode", value);
            }
        }


        /// <summary>
        /// Coupon usage is limited by inserted value.
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyCouponCodeUseLimit
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyCouponCodeUseLimit"), 0);
            }
            set
            {
                SetValue("MultiBuyCouponCodeUseLimit", value, value > 0);
            }
        }


        /// <summary>
        /// Multi buy coupon code use count
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyCouponCodeUseCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyCouponCodeUseCount"), 0);
            }
            set
            {
                SetValue("MultiBuyCouponCodeUseCount", value);
            }
        }


        /// <summary>
        /// Multi buy coupon code multi buy discount ID
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyCouponCodeMultiBuyDiscountID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyCouponCodeMultiBuyDiscountID"), 0);
            }
            set
            {
                SetValue("MultiBuyCouponCodeMultiBuyDiscountID", value);
            }
        }


        /// <summary>
        /// Multi buy coupon code last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime MultiBuyCouponCodeLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("MultiBuyCouponCodeLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MultiBuyCouponCodeLastModified", value);
            }
        }


        /// <summary>
        /// Multi buy coupon code GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid MultiBuyCouponCodeGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("MultiBuyCouponCodeGUID"), Guid.Empty);
            }
            set
            {
                SetValue("MultiBuyCouponCodeGUID", value);
            }
        }


        /// <summary>
        /// Indicates that this code has exceeded its use limitation. Returns false for codes with no use limit.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseLimitExceeded
        {
            get
            {
                return (MultiBuyCouponCodeUseLimit > 0) && (MultiBuyCouponCodeUseCount >= MultiBuyCouponCodeUseLimit);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyCouponCodeInfoProvider.DeleteMultiBuyCouponCodeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyCouponCodeInfoProvider.SetMultiBuyCouponCodeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public MultiBuyCouponCodeInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty MultiBuyCouponCodeInfo object.
        /// </summary>
        public MultiBuyCouponCodeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MultiBuyCouponCodeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MultiBuyCouponCodeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            MultiBuyCouponCodeUseCount = 0;
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        public override bool CheckUniqueCodeName()
        {
            if (!base.CheckUniqueCodeName())
            {
                return false;
            }

            var site = ObjectParent?.Generalized.ObjectSiteID ?? 0;

            var query = ECommerceHelper.GetAllCouponCodesQuery(site).WhereEquals("CouponCodeCode", MultiBuyCouponCodeCode);

            return !query.HasResults();
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            bool result;
            if (!MultiBuyCouponCodeInfoProvider.TryCheckDiscountPermissions(permission, siteName, userInfo, exceptionOnFailure, out result))
            {
                return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }

            return result;
        }

        #endregion
    }
}