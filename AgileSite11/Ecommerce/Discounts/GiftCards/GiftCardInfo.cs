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

[assembly: RegisterObjectType(typeof(GiftCardInfo), GiftCardInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="GiftCardInfo"/>.
    /// </summary>
	[Serializable]
    public class GiftCardInfo : AbstractInfo<GiftCardInfo>, IDiscountInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.giftcard";
        

        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GiftCardInfoProvider), OBJECT_TYPE, "Ecommerce.GiftCard", "GiftCardID", "GiftCardLastModified", "GiftCardGuid", "GiftCardName", "GiftCardDisplayName", null, "GiftCardSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation> { new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE) }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            SupportsGlobalObjects = false,
            SupportsCloneToOtherSite = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },
            EnabledColumn = "GiftCardEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private bool? mCouponsUseLimitExceeded;
        private bool? mHasCoupons;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gift card ID.
        /// </summary>
        [DatabaseField]
        public virtual int GiftCardID
        {
            get
            {
                return GetIntegerValue("GiftCardID", 0);
            }
            set
            {
                SetValue("GiftCardID", value);
            }
        }


        /// <summary>
        /// Gift card guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GiftCardGuid
        {
            get
            {
                return GetGuidValue("GiftCardGuid", Guid.Empty);
            }
            set
            {
                SetValue("GiftCardGuid", value);
            }
        }


        /// <summary>
        /// Gift card name displayed to customers.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardDisplayName
        {
            get
            {
                return GetStringValue("GiftCardDisplayName", String.Empty);
            }
            set
            {
                SetValue("GiftCardDisplayName", value);
            }
        }


        /// <summary>
        /// Gift card name used internally.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardName
        {
            get
            {
                return GetStringValue("GiftCardName", String.Empty);
            }
            set
            {
                SetValue("GiftCardName", value);
            }
        }


        /// <summary>
        /// Internal note invisible to customers.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardDescription
        {
            get
            {
                return GetStringValue("GiftCardDescription", String.Empty);
            }
            set
            {
                SetValue("GiftCardDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// If disabled, the gift card is never applied. If enabled, the gift card checks other conditions to be applied.
        /// </summary>
        [DatabaseField]
        public virtual bool GiftCardEnabled
        {
            get
            {
                return GetBooleanValue("GiftCardEnabled", true);
            }
            set
            {
                SetValue("GiftCardEnabled", value);
            }
        }


        /// <summary>
        /// Gift card last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GiftCardLastModified
        {
            get
            {
                return GetDateTimeValue("GiftCardLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GiftCardLastModified", value);
            }
        }


        /// <summary>
        /// Gift card site ID.
        /// </summary>
        [DatabaseField]
        public virtual int GiftCardSiteID
        {
            get
            {
                return GetIntegerValue("GiftCardSiteID", 0);
            }
            set
            {
                SetValue("GiftCardSiteID", value, 0);
            }
        }


        /// <summary>
        /// Value of gift card.
        /// </summary>
        [DatabaseField]
        public virtual decimal GiftCardValue
        {
            get
            {
                return GetDecimalValue("GiftCardValue", 0m);
            }
            set
            {
                SetValue("GiftCardValue", value);
            }
        }


        /// <summary>
        /// Gift card minimum order price.
        /// </summary>
        [DatabaseField]
        public virtual decimal GiftCardMinimumOrderPrice
        {
            get
            {
                return GetDecimalValue("GiftCardMinimumOrderPrice", 0m);
            }
            set
            {
                SetValue("GiftCardMinimumOrderPrice", value, 0m);
            }
        }


        /// <summary>
        /// Conditions which have to be met in order to apply the gift card.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardCartCondition
        {
            get
            {
                return GetStringValue("GiftCardCartCondition", String.Empty);
            }
            set
            {
                SetValue("GiftCardCartCondition", value, String.Empty);
            }
        }


        /// <summary>
        /// Gift card is valid from this date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GiftCardValidFrom
        {
            get
            {
                return GetDateTimeValue("GiftCardValidFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GiftCardValidFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gift card is valid to this date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GiftCardValidTo
        {
            get
            {
                return GetDateTimeValue("GiftCardValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("GiftCardValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Customers to limit gift card for.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual DiscountCustomerEnum GiftCardCustomerRestriction
        {
            get
            {
                return GetStringValue("GiftCardCustomerRestriction", "").ToEnum<DiscountCustomerEnum>();
            }
            set
            {
                SetValue("GiftCardCustomerRestriction", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// User roles to apply gift cards for.
        /// </summary>
        [DatabaseField]
        public virtual string GiftCardRoles
        {
            get
            {
                return GetStringValue("GiftCardRoles", String.Empty);
            }
            set
            {
                SetValue("GiftCardRoles", value, String.Empty);
            }
        }


        /// <summary>
        /// Indicates that all coupon codes has exceeded its use limitation.
        /// </summary>
        public virtual bool CouponsUseLimitExceeded
        {
            get
            {
                if (!mCouponsUseLimitExceeded.HasValue)
                {
                    // Find at least one not exceeded coupon code
                    var nonExceededCoupon = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodes(GiftCardID)
                                                                 .WhereGreaterThan("GiftCardCouponCodeRemainingValue", 0)                                                                                                                                    
                                                                 .TopN(1);

                    mCouponsUseLimitExceeded = !nonExceededCoupon.HasResults();
                }

                return mCouponsUseLimitExceeded.Value;
            }
        }


        /// <summary>
        /// Indicates if gift card has some coupons defined.
        /// </summary>
        [RegisterProperty]
        public virtual bool HasCoupons
        {
            get
            {
                if (!mHasCoupons.HasValue)
                {
                    // Find at least one coupon for this gift card
                    mHasCoupons = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodes(GiftCardID).TopN(1).HasResults();
                }

                return mHasCoupons.Value;
            }
        }

        #endregion


        #region IDiscountInfo interface

        int IDiscountInfo.DiscountID => GiftCardID;


        string IDiscountInfo.DiscountDisplayName => GiftCardDisplayName;


        bool IDiscountInfo.DiscountEnabled => GiftCardEnabled;


        int IDiscountInfo.DiscountSiteID => GiftCardSiteID;


        DiscountStatusEnum IDiscountInfo.DiscountStatus => Status;


        DiscountTypeEnum IDiscountInfo.DiscountType => DiscountTypeEnum.GiftCard;


        bool IDiscountInfo.DiscountUsesCoupons => true;


        /// <summary>
        /// Gets the status of gift card. 
        /// </summary>
        /// <remarks>
        /// Possible values are Disabled, Incomplete, Active, NotStarted of Finished.
        /// </remarks>
        public DiscountStatusEnum Status
        {
            get
            {
                if (!GiftCardEnabled)
                {
                    return DiscountStatusEnum.Disabled;
                }

                if (!HasCoupons)
                {
                    return DiscountStatusEnum.Incomplete;
                }

                if (IsRunning)
                {
                    if (CouponsUseLimitExceeded)
                    {
                        return DiscountStatusEnum.Finished;
                    }

                    return DiscountStatusEnum.Active;
                }

                if (GiftCardValidFrom > DateTime.Now)
                {
                    return DiscountStatusEnum.NotStarted;
                }

                if (GiftCardValidTo < DateTime.Now)
                {
                    return DiscountStatusEnum.Finished;
                }

                return EnumHelper.GetDefaultValue<DiscountStatusEnum>();
            }
        }


        /// <summary>
        /// Determines whether gift card is currently running. ValidFrom and ValidTo properties are compared to current date/time.
        /// </summary>
        public bool IsRunning => IsRunningDueDate(DateTime.Now);


        BaseInfo IDiscountInfo.CreateCoupon(CouponGeneratorConfig config)
        {
            return GiftCardCouponCodeInfoProvider.CreateGiftCardCoupon(this, config.CouponCode);
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Informs this gift card that it was applied.
        /// </summary>
        /// <param name="couponCode">The coupon code that was used for gift card application.</param>
        /// <param name="paymentValue">The amount paid by gift card coupon.</param>       
        public void LogUseOnce(string couponCode, decimal paymentValue)
        {
            GiftCardInfoProvider.LogGiftCardUseOnce(this, couponCode, paymentValue);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            GiftCardInfoProvider.DeleteGiftCardInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            GiftCardInfoProvider.SetGiftCardInfo(this);
        }


        /// <summary>
        /// Invalidates the current gift card.
        /// Stored query results are cleared because gift card codes could been modified.
        /// </summary>
        /// <param name="keepThisInstanceValid">Indicates if the current instance remains valid</param>
        protected override void Invalidate(bool keepThisInstanceValid)
        {
            base.Invalidate(keepThisInstanceValid);

            mCouponsUseLimitExceeded = null;
            mHasCoupons = null;
        }


        /// <summary>
        /// Determines whether gift card is running due the specified date. ValidFrom and ValidTo properties are compared to the specified date/time.
        /// </summary>
        internal bool IsRunningDueDate(DateTime date)
        {
            return (GiftCardValidFrom == DateTimeHelper.ZERO_TIME || GiftCardValidFrom <= date) &&
                   (GiftCardValidTo == DateTimeHelper.ZERO_TIME || GiftCardValidTo >= date);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected GiftCardInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="GiftCardInfo"/> class.
        /// </summary>
        public GiftCardInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GiftCardInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public GiftCardInfo(DataRow dr)
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