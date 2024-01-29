using System;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(CouponCodeInfo), CouponCodeInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// CouponCodeInfo data container class.
    /// </summary>
    [Serializable]
    public class CouponCodeInfo : AbstractInfo<CouponCodeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.couponcode";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CouponCodeInfoProvider), OBJECT_TYPE, "Ecommerce.CouponCode", "CouponCodeID", "CouponCodeLastModified", "CouponCodeGUID", "CouponCodeCode", null, null, null, "CouponCodeDiscountID", DiscountInfo.OBJECT_TYPE)
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
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "CouponCodeUseCount"}
            },
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Coupon code ID.
        /// </summary>
        [DatabaseField]
        public virtual int CouponCodeID
        {
            get
            {
                return GetIntegerValue("CouponCodeID", 0);
            }
            set
            {
                SetValue("CouponCodeID", value);
            }
        }


        /// <summary>
        /// Unique discount coupon code.
        /// </summary>
        [DatabaseField]
        public virtual string CouponCodeCode
        {
            get
            {
                return GetStringValue("CouponCodeCode", "");
            }
            set
            {
                SetValue("CouponCodeCode", value);
            }
        }


        /// <summary>
        /// Indicates how many times the coupon code can be applied.
        /// </summary>
        [DatabaseField]
        public virtual int CouponCodeUseLimit
        {
            get
            {
                return GetIntegerValue("CouponCodeUseLimit", 0);
            }
            set
            {
                SetValue("CouponCodeUseLimit", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates how many times the code was applied.
        /// </summary>
        [DatabaseField]
        public virtual int CouponCodeUseCount
        {
            get
            {
                return GetIntegerValue("CouponCodeUseCount", 0);
            }
            set
            {
                SetValue("CouponCodeUseCount", value);
            }
        }


        /// <summary>
        /// ID of the coupon code parent discount.
        /// </summary>
        [DatabaseField]
        public virtual int CouponCodeDiscountID
        {
            get
            {
                return GetIntegerValue("CouponCodeDiscountID", 0);
            }
            set
            {
                SetValue("CouponCodeDiscountID", value);
            }
        }


        /// <summary>
        /// Date and time when coupon code was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CouponCodeLastModified
        {
            get
            {
                return GetDateTimeValue("CouponCodeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CouponCodeLastModified", value);
            }
        }


        /// <summary>
        /// Coupon code unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid CouponCodeGUID
        {
            get
            {
                return GetGuidValue("CouponCodeGUID", Guid.Empty);
            }
            set
            {
                SetValue("CouponCodeGUID", value);
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
                return (CouponCodeUseLimit > 0) && (CouponCodeUseCount >= CouponCodeUseLimit);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CouponCodeInfoProvider.DeleteCouponCodeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CouponCodeInfoProvider.SetCouponCodeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CouponCodeInfo object.
        /// </summary>
        public CouponCodeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CouponCodeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CouponCodeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            CouponCodeUseCount = 0;
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

            var query = ECommerceHelper.GetAllCouponCodesQuery(site).WhereEquals("CouponCodeCode", CouponCodeCode);
            
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
            if (!CouponCodeInfoProvider.TryCheckDiscountPermissions(permission, siteName, userInfo, exceptionOnFailure, out result))
            {
                return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }

            return result;
        }

        #endregion
    }
}