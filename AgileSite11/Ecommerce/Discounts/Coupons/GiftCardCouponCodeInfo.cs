using System;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(GiftCardCouponCodeInfo), GiftCardCouponCodeInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// GiftCardCouponCodeInfo data container class.
    /// </summary>
    [Serializable]
    public class GiftCardCouponCodeInfo : AbstractInfo<GiftCardCouponCodeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.giftcardcouponcode";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GiftCardCouponCodeInfoProvider), OBJECT_TYPE, "Ecommerce.GiftCardCouponCode", "GiftCardCouponCodeID", "GiftCardCouponCodeLastModified", "GiftCardCouponCodeGUID", "GiftCardCouponCodeCode", null, null, null, "GiftCardCouponCodeGiftCardID", GiftCardInfo.OBJECT_TYPE)
        {
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
                ExcludedFieldNames = { "GiftCardCouponCodeRemainingValue" }
            },
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gift card coupon code ID.
        /// </summary>
        [DatabaseField]
        public virtual int GiftCardCouponCodeID
        {
            get
            {
                return GetIntegerValue("GiftCardCouponCodeID", 0);
            }
            set
            {
                SetValue("GiftCardCouponCodeID", value);
            }
        }


        /// <summary>
        /// Unique gift card coupon code.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardCouponCodeCode
        {
            get
            {
                return GetStringValue("GiftCardCouponCodeCode", "");
            }
            set
            {
                SetValue("GiftCardCouponCodeCode", value);
            }
        }


        /// <summary>
        /// Coupon code's remaining value
        /// </summary>
        [DatabaseField]
        public virtual decimal GiftCardCouponCodeRemainingValue
        {
            get
            {
                return GetDecimalValue("GiftCardCouponCodeRemainingValue", 0m);
            }
            set
            {
                SetValue("GiftCardCouponCodeRemainingValue", value);
            }
        }


        /// <summary>
        /// ID of the coupon code parent gift card.
        /// </summary>
        [DatabaseField]
        public virtual int GiftCardCouponCodeGiftCardID
        {
            get
            {
                return GetIntegerValue("GiftCardCouponCodeGiftCardID", 0);
            }
            set
            {
                SetValue("GiftCardCouponCodeGiftCardID", value);
            }
        }


        /// <summary>
        /// Date and time when gift card coupon code was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GiftCardCouponCodeLastModified
        {
            get
            {
                return GetDateTimeValue("GiftCardCouponCodeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GiftCardCouponCodeLastModified", value);
            }
        }


        /// <summary>
        /// Gift card coupon code unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid GiftCardCouponCodeGuid
        {
            get
            {
                return GetGuidValue("GiftCardCouponCodeGuid", Guid.Empty);
            }
            set
            {
                SetValue("GiftCardCouponCodeGuid", value);
            }
        }


        /// <summary>
        /// The value of a parent gift card. Used also in UniGrid transformation.
        /// </summary>
        [RegisterProperty]
        public virtual decimal GiftCardCouponCodeGiftCardValue => (Parent as GiftCardInfo)?.GiftCardValue ?? 0m;

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            GiftCardCouponCodeInfoProvider.DeleteGiftCardCouponCodeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            GiftCardCouponCodeInfoProvider.SetGiftCardCouponCodeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty GiftCardCouponCodeInfo object.
        /// </summary>
        public GiftCardCouponCodeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new GiftCardCouponCodeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GiftCardCouponCodeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
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

            var query = ECommerceHelper.GetAllCouponCodesQuery(site).WhereEquals("CouponCodeCode", GiftCardCouponCodeCode);

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
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return DiscountInfoProvider.IsUserAuthorizedToModifyDiscount(siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}