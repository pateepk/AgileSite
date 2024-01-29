using System;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(DiscountInfo), DiscountInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// DiscountInfo data container class.
    /// </summary>
    [Serializable]
    public class DiscountInfo : AbstractInfo<DiscountInfo>, IConditionalDiscount, IPrioritizable, IDiscountInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.discount";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DiscountInfoProvider), OBJECT_TYPE, "ECommerce.Discount", "DiscountID", "DiscountLastModified", "DiscountGUID", "DiscountName", "DiscountDisplayName", null, "DiscountSiteID", null, null)
                                              {
                                                  // Binding object types
                                                  // Synchronization
                                                  SynchronizationSettings =
                                                  {
                                                      LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                                                      ObjectTreeLocations = new List<ObjectTreeLocation>
                                                      {
                                                           new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),                                                      
                                                      }
                                                  },
                                                  // Others
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
                                                  EnabledColumn = "DiscountEnabled",
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Variables"

        private bool? mHasCoupons;
        private bool? mCouponsUseLimitExceeded;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Discount ID.
        /// </summary>
        [DatabaseField]
        public virtual int DiscountID
        {
            get
            {
                return GetIntegerValue("DiscountID", 0);
            }
            set
            {
                SetValue("DiscountID", value);
            }
        }


        /// <summary>
        /// Discount display name.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountDisplayName
        {
            get
            {
                return GetStringValue("DiscountDisplayName", "");
            }
            set
            {
                SetValue("DiscountDisplayName", value);
            }
        }


        /// <summary>
        /// Discount name.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountName
        {
            get
            {
                return GetStringValue("DiscountName", "");
            }
            set
            {
                SetValue("DiscountName", value);
            }
        }


        /// <summary>
        /// Discount description.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountDescription
        {
            get
            {
                return GetStringValue("DiscountDescription", "");
            }
            set
            {
                SetValue("DiscountDescription", value);
            }
        }


        /// <summary>
        /// Discount enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool DiscountEnabled
        {
            get
            {
                return GetBooleanValue("DiscountEnabled", false);
            }
            set
            {
                SetValue("DiscountEnabled", value);
            }
        }


        /// <summary>
        /// True - discount value is flat, False - discount value is relative. Used for discount calculation in the shopping cart.
        /// </summary>
        [DatabaseField]
        public virtual bool DiscountIsFlat
        {
            get
            {
                return GetBooleanValue("DiscountIsFlat", true);
            }
            set
            {
                SetValue("DiscountIsFlat", value);
            }
        }


        /// <summary>
        /// Discount value.
        /// </summary>
        [DatabaseField]
        public virtual decimal DiscountValue
        {
            get
            {
                return GetDecimalValue("DiscountValue", 0m);
            }
            set
            {
                SetValue("DiscountValue", value);
            }
        }


        /// <summary>
        /// Indicates if this discount is applicable only with discount coupon.
        /// </summary>
        [DatabaseField]
        public virtual bool DiscountUsesCoupons
        {
            get
            {
                return GetBooleanValue("DiscountUsesCoupons", false);
            }
            set
            {
                SetValue("DiscountUsesCoupons", value);
            }
        }


        /// <summary>
        /// Discount valid from.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DiscountValidFrom
        {
            get
            {
                return GetDateTimeValue("DiscountValidFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("DiscountValidFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Discount valid to.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DiscountValidTo
        {
            get
            {
                return GetDateTimeValue("DiscountValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("DiscountValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Macro condition to restrict this discount to particular products only.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountProductCondition
        {
            get
            {
                return GetStringValue("DiscountProductCondition", null);
            }
            set
            {
                SetValue("DiscountProductCondition", value);
            }
        }


        /// <summary>
        /// Macro condition to restrict this discount based on shopping cart.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountCartCondition
        {
            get
            {
                return GetStringValue("DiscountCartCondition", null);
            }
            set
            {
                SetValue("DiscountCartCondition", value);
            }
        }


        /// <summary>
        /// Indicates that further discounts (in order of DiscountOrder) are to be processed.
        /// </summary>
        [DatabaseField]
        public virtual bool DiscountApplyFurtherDiscounts
        {
            get
            {
                return GetBooleanValue("DiscountApplyFurtherDiscounts", true);
            }
            set
            {
                SetValue("DiscountApplyFurtherDiscounts", value);
            }
        }


        /// <summary>
        /// Order of this discount among other discounts.
        /// </summary>
        [DatabaseField]
        public virtual double DiscountOrder
        {
            get
            {
                return GetDoubleValue("DiscountOrder", 1);
            }
            set
            {
                SetValue("DiscountOrder", value);
            }
        }


        /// <summary>
        /// Order price to limit application of discount.
        /// </summary>
        [DatabaseField]
        public virtual decimal DiscountOrderAmount
        {
            get
            {
                return GetDecimalValue("DiscountOrderAmount", 0m);
            }
            set
            {
                SetValue("DiscountOrderAmount", value);
            }
        }


        /// <summary>
        /// Type of discount customer restriction.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual DiscountCustomerEnum DiscountCustomerRestriction
        {
            get
            {
                return GetStringValue("DiscountCustomerRestriction", "").ToEnum<DiscountCustomerEnum>();
            }
            set
            {
                SetValue("DiscountCustomerRestriction", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Indicates what kind of objects are the discounts applicable to. Discount can be applied to orders or products.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual DiscountApplicationEnum DiscountApplyTo
        {
            get
            {
                return GetStringValue("DiscountApplyTo", "").ToEnum<DiscountApplicationEnum>();
            }
            set
            {
                SetValue("DiscountApplyTo", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Discount roles to apply discount for. Is dependable on DiscountCustomerRestriction configuration.
        /// </summary>
        [DatabaseField]
        public virtual string DiscountRoles
        {
            get
            {
                return GetStringValue("DiscountRoles", "");
            }
            set
            {
                SetValue("DiscountRoles", value);
            }
        }


        /// <summary>
        /// Discount GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid DiscountGUID
        {
            get
            {
                return GetGuidValue("DiscountGUID", Guid.Empty);
            }
            set
            {
                SetValue("DiscountGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Discount last modified date and time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DiscountLastModified
        {
            get
            {
                return GetDateTimeValue("DiscountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("DiscountLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Discount site ID.
        /// </summary>
        [DatabaseField]
        public virtual int DiscountSiteID
        {
            get
            {
                return GetIntegerValue("DiscountSiteID", 0);
            }
            set
            {
                SetValue("DiscountSiteID", value, 0);
            }
        }


        /// <summary>
        /// Determines whether discount is currently running. ValidFrom and ValidTo properties are compared to current date/time.
        /// </summary>
        public virtual bool IsRunning => IsRunningDueDate(DateTime.Now);


        /// <summary>
        /// Indicates that all coupon codes has exceeded its use limitation. Returns false for discounts without coupon codes.
        /// </summary>
        public virtual bool CouponsUseLimitExceeded
        {
            get
            {
                if (!mCouponsUseLimitExceeded.HasValue)
                {
                    // Find at least one not exceeded coupon code
                    var nonExceededCoupon = CouponCodeInfoProvider.GetDiscountCouponCodes(DiscountID)
                                                                     .Where(new WhereCondition()
                                                                         .WhereNull("CouponCodeUseLimit")
                                                                         .Or()
                                                                         .WhereLessThan("CouponCodeUseCount", "CouponCodeUseLimit".AsColumn()))
                                                                     .TopN(1);

                    mCouponsUseLimitExceeded = HasCoupons && !nonExceededCoupon.HasResults();
                }

                return mCouponsUseLimitExceeded.Value;
            }
        }


        /// <summary>
        /// Indicates whether this discount can be applied to whole orders.
        /// </summary>
        public virtual bool IsApplicableToOrders
        {
            get
            {
                return DiscountApplyTo == DiscountApplicationEnum.Order;
            }
        }


        /// <summary>
        /// Indicates whether this discount can be applied to shipping price.
        /// </summary>
        public virtual bool IsApplicableToShipping
        {
            get
            {
                return DiscountApplyTo == DiscountApplicationEnum.Shipping;
            }
        }


        /// <summary>
        /// Indicates that this discount is catalog kind of discount. 
        /// This means that discount does not need shopping cart data to be applied.
        /// </summary>
        public virtual bool IsCatalogDiscount
        {
            get
            {
                return (DiscountApplyTo == DiscountApplicationEnum.Products) && string.IsNullOrEmpty(DiscountCartCondition);
            }
        }


        /// <summary>
        /// Indicates if discount has some coupons defined.
        /// </summary>
        [RegisterProperty]
        public virtual bool HasCoupons
        {
            get
            {
                if (!mHasCoupons.HasValue)
                {
                    // Find at least one coupon for this discount
                    mHasCoupons = CouponCodeInfoProvider.GetDiscountCouponCodes(DiscountID).TopN(1).HasResults();
                }

                return mHasCoupons.Value;
            }
        }


        /// <summary>
        /// Indicates if discount uses coupon or has some coupons defined.
        /// </summary>
        [RegisterProperty]
        public virtual bool HasOrUsesCoupon
        {
            get
            {
                return DiscountUsesCoupons || HasCoupons;
            }
        }


        /// <summary>
        /// Indicates if given coupon code is suitable for this discount. Returns false if this discount has no codes assigned.
        /// </summary>
        /// <param name="couponCode">Code to be checked</param>
        /// <param name="ignoreUseLimit">Indicates if use limitation is to be ignored.</param>
        public virtual bool AcceptsCoupon(string couponCode, bool ignoreUseLimit)
        {
            if (!DiscountUsesCoupons)
            {
                return false;
            }

            var code = CouponCodeInfoProvider.GetDiscountCouponCode(DiscountID, couponCode);
            return (code != null) && (ignoreUseLimit || !code.UseLimitExceeded);
        }


        /// <summary>
        /// Discount status.
        /// </summary>
        public virtual DiscountStatusEnum DiscountStatus
        {
            get
            {
                if (!DiscountEnabled)
                {
                    return DiscountStatusEnum.Disabled;
                }

                if (DiscountUsesCoupons && !HasCoupons)
                {
                    return DiscountStatusEnum.Incomplete;
                }

                if (IsRunning)
                {
                    if (DiscountUsesCoupons && CouponsUseLimitExceeded)
                    {
                        return DiscountStatusEnum.Finished;
                    }

                    return DiscountStatusEnum.Active;
                }

                if (DiscountValidFrom > DateTime.Now)
                {
                    return DiscountStatusEnum.NotStarted;
                }

                if (DiscountValidTo < DateTime.Now)
                {
                    return DiscountStatusEnum.Finished;
                }

                return EnumHelper.GetDefaultValue<DiscountStatusEnum>();
            }
        }


        /// <summary>
        /// Gets the type of the discount.
        /// </summary>
        DiscountTypeEnum IDiscountInfo.DiscountType => ItemDiscountType;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DiscountInfo object.
        /// </summary>
        public DiscountInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DiscountInfo object from the given DataRow.
        /// </summary>
        public DiscountInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public DiscountInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the discount using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DiscountInfoProvider.DeleteDiscountInfo(this);
        }


        /// <summary>
        /// Updates the discount using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DiscountInfoProvider.SetDiscountInfo(this);
        }


        /// <summary>
        /// Invalidates the current discount.
        /// Stored query results are cleared because discount codes could been modified.
        /// </summary>
        /// <param name="keepThisInstanceValid">Indicates if the current instance remains valid.</param>
        protected override void Invalidate(bool keepThisInstanceValid)
        {
            base.Invalidate(keepThisInstanceValid);

            // Clear store query results
            mCouponsUseLimitExceeded = null;
            mHasCoupons = null;
        }

        #endregion


        #region "IConditionalDiscount interface"

        /// <summary>
        /// Discount type. Used for discount calculation in the shopping cart.
        /// </summary>
        public DiscountTypeEnum ItemDiscountType
        {
            get
            {
                switch (DiscountApplyTo)
                {
                    case DiscountApplicationEnum.Products:
                        return DiscountTypeEnum.CatalogDiscount;
                    case DiscountApplicationEnum.Order:
                        return DiscountTypeEnum.OrderDiscount;
                    case DiscountApplicationEnum.Shipping:
                        return DiscountTypeEnum.ShippingDiscount;
                    default:
                        return DiscountTypeEnum.Discount;
                }
            }
        }


        /// <summary>
        /// Discount display name. Used for discount calculation in the shopping cart.
        /// </summary>
        public string ItemDiscountDisplayName => DiscountDisplayName;


        /// <summary>
        /// Discount value. Used for discount calculation in the shopping cart.
        /// </summary>
        public decimal ItemDiscountValue => DiscountValue;


        /// <summary>
        /// True - discount value is flat, False - discount value is relative. Used for discount calculation in the shopping cart.
        /// </summary>
        public bool ItemDiscountIsFlat => DiscountIsFlat;


        /// <summary>
        /// True - says that discounts value is set in global main currency, False - says that discounts value is set in site main currency. 
        /// Takes effect only if discount value is flat. Used for discount calculation in the shopping cart.
        /// </summary>
        public bool ItemDiscountIsGlobal => IsGlobal;


        /// <summary>
        /// Indicates that discounts following this one in order of increasing DiscountOrder value are to be applied.
        /// </summary>
        public bool ApplyFurtherDiscounts
        {
            get
            {
                return DiscountApplyFurtherDiscounts;
            }
            set
            {
                DiscountApplyFurtherDiscounts = value;
            }
        }


        /// <summary>
        /// Discount order used to define its priority (1 is the highest priority). Corresponds to DiscountOrder property.
        /// </summary>
        public double DiscountItemOrder
        {
            get
            {
                return DiscountOrder;
            }
            set
            {
                DiscountOrder = value;
            }
        }


        /// <summary>
        /// Order price to limit application of discount.
        /// </summary>
        public decimal DiscountItemMinOrderAmount
        {
            get
            {
                return DiscountOrderAmount;
            }
            set
            {
                DiscountOrderAmount = value;
            }
        }


        /// <summary>
        /// Returns true if this discount is to be applied on the same base as the given discount.
        /// False indicates that discounts are to be applied one after another. False is returned 
        /// also when discount parameter is null.
        /// </summary>
        /// <param name="discount">Examined discount.</param>
        public bool ApplyTogetherWith(IConditionalDiscount discount)
        {
            return discount?.DiscountItemOrder == DiscountItemOrder;
        }


        /// <summary>
        /// Informs this discount that it was applied.
        /// </summary>
        /// <param name="couponCode">The coupon code that was used for discount application.</param>       
        public void LogUseOnce(string couponCode)
        {
            DiscountInfoProvider.LogDiscountUseOnce(this, couponCode);
        }

        #endregion


        #region "IPrioritizable interface"

        /// <summary>
        /// Discount priority.
        /// </summary>
        public double Priority
        {
            get
            {
                return DiscountOrder;
            }
        }


        /// <summary>
        /// Updates discount priority.
        /// </summary>
        /// <param name="priority">New priority</param>
        /// <returns>Error message (null if succeeded)</returns>
        public string TryUpdatePriority(double priority)
        {
            // Check if the priority higher than 1
            if (priority < 1)
            {
                return ResHelper.GetString("com.discountedit.weightinvalid");
            }

            // Make object complete and update
            DiscountOrder = priority;
            MakeComplete(true);
            Update();
            return null;
        }


        /// <summary>
        /// Determines whether discount is running due the specified date. ValidFrom and ValidTo properties are compared to the specified date/time.
        /// </summary>
        internal bool IsRunningDueDate(DateTime date)
        {
            return (DiscountValidFrom == DateTimeHelper.ZERO_TIME || DiscountValidFrom <= date) &&
                   (DiscountValidTo == DateTimeHelper.ZERO_TIME || DiscountValidTo >= date);
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


        /// <summary>
        /// Creates and saves new coupon for this discount.
        /// NOTE: The uniqueness is not checked.
        /// </summary>        
        /// <param name="config">Configuration for coupon code creation</param>
        BaseInfo IDiscountInfo.CreateCoupon(CouponGeneratorConfig config)
        {
            return CouponCodeInfoProvider.CreateCoupon(this, config.CouponCode, config.NumberOfUses);
        }

        #endregion
    }
}