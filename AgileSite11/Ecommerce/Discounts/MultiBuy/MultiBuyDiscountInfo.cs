using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Ecommerce;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountInfo), MultiBuyDiscountInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(MultiBuyDiscountInfo), MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// MultiBuyDiscountInfo data container class.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountInfo : AbstractInfo<MultiBuyDiscountInfo>, IDiscountInfo
    {
        #region "Type information"

        /// <summary>
        /// Multibuy discount object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscount";


        /// <summary>
        /// Product coupon discount object type
        /// </summary>
        public const string OBJECT_TYPE_PRODUCT_COUPON = "ecommerce.productcoupondiscount";
        

        /// <summary>
        /// Multibuy discount type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscount", "MultiBuyDiscountID", "MultiBuyDiscountLastModified", "MultiBuyDiscountGUID", "MultiBuyDiscountName", "MultiBuyDiscountDisplayName", null, "MultiBuyDiscountSiteID", null, null)
        {
            // Object dependencies
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MultiBuyDiscountApplyToSKUID", SKUInfo.OBJECT_TYPE_SKU)
            },

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)                                                    
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
            SupportsCloning = false,
            OrderColumn = "MultiBuyDiscountPriority",
            TypeCondition = new TypeCondition().WhereEquals("MultiBuyDiscountIsProductCoupon", false),
            MacroCollectionName = OBJECT_TYPE,

            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,

                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },

            EnabledColumn = "MultiBuyDiscountEnabled",

            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Product coupon discount type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO_PRODUCT_COUPON = new ObjectTypeInfo(typeof(MultiBuyDiscountInfoProvider), OBJECT_TYPE_PRODUCT_COUPON, "Ecommerce.MultiBuyDiscount", "MultiBuyDiscountID", "MultiBuyDiscountLastModified", "MultiBuyDiscountGUID", "MultiBuyDiscountName", "MultiBuyDiscountDisplayName", null, "MultiBuyDiscountSiteID", null, null)
        {
            OriginalTypeInfo = TYPEINFO,

            // Object dependencies
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MultiBuyDiscountApplyToSKUID", SKUInfo.OBJECT_TYPE_SKU)
            },

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)
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
            SupportsCloning = false,
            OrderColumn = "MultiBuyDiscountPriority",
            TypeCondition = new TypeCondition().WhereEquals("MultiBuyDiscountIsProductCoupon", true),
            MacroCollectionName = OBJECT_TYPE_PRODUCT_COUPON,

            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,

                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },

            EnabledColumn = "MultiBuyDiscountEnabled",

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


        #region "Properties"

        /// <summary>
        /// Multi buy discount ID
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyDiscountID"), 0);
            }
            set
            {
                SetValue("MultiBuyDiscountID", value);
            }
        }


        /// <summary>
        /// Multi buy discount display name
        /// </summary>
        [DatabaseField]
        public virtual string MultiBuyDiscountDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MultiBuyDiscountDisplayName"), "");
            }
            set
            {
                SetValue("MultiBuyDiscountDisplayName", value);
            }
        }


        /// <summary>
        /// Multi buy discount name
        /// </summary>
        [DatabaseField]
        public virtual string MultiBuyDiscountName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MultiBuyDiscountName"), "");
            }
            set
            {
                SetValue("MultiBuyDiscountName", value);
            }
        }


        /// <summary>
        /// Multi buy discount description
        /// </summary>
        [DatabaseField]
        public virtual string MultiBuyDiscountDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MultiBuyDiscountDescription"), "");
            }
            set
            {
                SetValue("MultiBuyDiscountDescription", value);
            }
        }


        /// <summary>
        /// Multi buy discount enabled
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("MultiBuyDiscountEnabled"), true);
            }
            set
            {
                SetValue("MultiBuyDiscountEnabled", value);
            }
        }


        /// <summary>
        /// True - multi buy discount value is flat, False - multi buy value is relative. Used for discount calculation in the shopping cart.
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountIsFlat
        {
            get
            {
                return GetBooleanValue("MultiBuyDiscountIsFlat", false);
            }
            set
            {
                SetValue("MultiBuyDiscountIsFlat", value);
            }
        }


        /// <summary>
        /// Multi buy discount value.
        /// </summary>
        [DatabaseField]
        public virtual decimal MultiBuyDiscountValue
        {
            get
            {
                return GetDecimalValue("MultiBuyDiscountValue", 100m);
            }
            set
            {
                SetValue("MultiBuyDiscountValue", value);
            }
        }
        

        /// <summary>
        /// Multi buy discount GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid MultiBuyDiscountGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("MultiBuyDiscountGUID"), Guid.Empty);
            }
            set
            {
                SetValue("MultiBuyDiscountGUID", value);
            }
        }


        /// <summary>
        /// Multi buy discount last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime MultiBuyDiscountLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("MultiBuyDiscountLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MultiBuyDiscountLastModified", value);
            }
        }


        /// <summary>
        /// Multi buy discount get product sku ID, i.e ID of product which user can get for free.
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountApplyToSKUID
        {
            get
            {
                return GetIntegerValue("MultiBuyDiscountApplyToSKUID", 0);
            }
            set
            {
                SetValue("MultiBuyDiscountApplyToSKUID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates if this discount is applicable only with discount coupon.
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountUsesCoupons
        {
            get
            {
                return GetBooleanValue("MultiBuyDiscountUsesCoupons", false);
            }
            set
            {
                SetValue("MultiBuyDiscountUsesCoupons", value);
            }
        }


        /// <summary>
        /// Multi buy discount valid from
        /// </summary>
        [DatabaseField]
        public virtual DateTime MultiBuyDiscountValidFrom
        {
            get
            {
                return GetDateTimeValue("MultiBuyDiscountValidFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MultiBuyDiscountValidFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Multi buy discount valid to
        /// </summary>
        [DatabaseField]
        public virtual DateTime MultiBuyDiscountValidTo
        {
            get
            {
                return GetDateTimeValue("MultiBuyDiscountValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MultiBuyDiscountValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Multi buy discount site ID
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyDiscountSiteID"), 0);
            }
            set
            {
                SetValue("MultiBuyDiscountSiteID", value, 0);
            }
        }


        /// <summary>
        /// Multi buy discount priority.
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountPriority
        {
            get
            {
                return GetIntegerValue("MultiBuyDiscountPriority", 0);
            }
            set
            {
                SetValue("MultiBuyDiscountPriority", value);
            }
        }


        /// <summary>
        /// Indicates that further discounts (in order of MultiBuyDiscountPriority) are to be processed.
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountApplyFurtherDiscounts
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("MultiBuyDiscountApplyFurtherDiscounts"), false);
            }
            set
            {
                SetValue("MultiBuyDiscountApplyFurtherDiscounts", value);
            }
        }


        /// <summary>
        /// Specifies how many times customers can apply the discount in one order.
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountLimitPerOrder
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyDiscountLimitPerOrder"), 0);
            }
            set
            {
                SetValue("MultiBuyDiscountLimitPerOrder", value, 0);
            }
        }


        /// <summary>
        /// Minimum unit quantity condition. Default value is 1.
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountMinimumBuyCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyDiscountMinimumBuyCount"), 1);
            }
            set
            {
                SetValue("MultiBuyDiscountMinimumBuyCount", value);
            }
        }


        /// <summary>
        /// Multibuy discount customer restriction.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual DiscountCustomerEnum MultiBuyDiscountCustomerRestriction
        {
            get
            {
                return GetStringValue("MultiBuyDiscountCustomerRestriction", "").ToEnum<DiscountCustomerEnum>();
            }
            set
            {
                SetValue("MultiBuyDiscountCustomerRestriction", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Discount roles to apply discount for. Is dependable on MultiBuyDiscountCustomerRestriction configuration.
        /// </summary>
        [DatabaseField]
        public virtual string MultiBuyDiscountRoles
        {
            get
            {
                return GetStringValue("MultiBuyDiscountRoles", "");
            }
            set
            {
                SetValue("MultiBuyDiscountRoles", value);
            }
        }


        /// <summary>
        /// Indicates if product is added to cart automatically, the system adds product to shopping cart only when the discount is percentage and set 100 % off.
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountAutoAddEnabled
        {
            get
            {
                return GetBooleanValue("MultiBuyDiscountAutoAddEnabled", false);
            }
            set
            {
                SetValue("MultiBuyDiscountAutoAddEnabled", value);
            }
        }


        /// <summary>
        /// Indicates whether discount is product coupon or not.
        /// </summary>
        [DatabaseField]
        public virtual bool MultiBuyDiscountIsProductCoupon
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("MultiBuyDiscountIsProductCoupon"), false);
            }
            set
            {
                SetValue("MultiBuyDiscountIsProductCoupon", value);
            }
        }

        #endregion


        #region "IDiscountInfo members"

        int IDiscountInfo.DiscountID => MultiBuyDiscountID;


        string IDiscountInfo.DiscountDisplayName => MultiBuyDiscountDisplayName;


        bool IDiscountInfo.DiscountEnabled => MultiBuyDiscountEnabled;


        int IDiscountInfo.DiscountSiteID => MultiBuyDiscountSiteID;


        DiscountStatusEnum IDiscountInfo.DiscountStatus => Status;


        DiscountTypeEnum IDiscountInfo.DiscountType => MultiBuyDiscountIsProductCoupon ? DiscountTypeEnum.ProductCoupon : DiscountTypeEnum.MultibuyDiscount;


        BaseInfo IDiscountInfo.CreateCoupon(CouponGeneratorConfig config)
        {
            return MultiBuyCouponCodeInfoProvider.CreateCoupon(this, config.CouponCode, config.NumberOfUses);
        }

        #endregion


        #region "Custom properties"

        /// <summary>
        /// Object type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return MultiBuyDiscountIsProductCoupon 
                    ? TYPEINFO_PRODUCT_COUPON 
                    : TYPEINFO;
            }
        }


        /// <summary>
        /// Determines whether multi buy discount is currently running. ValidFrom and ValidTo properties are compared to current date/time.
        /// </summary>
        public virtual bool IsRunning
        {
            get
            {
                return IsValidForDate(DateTime.Now);
            }
        }


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
                    var nonExceededCoupon = MultiBuyCouponCodeInfoProvider.GetDiscountCouponCodes(MultiBuyDiscountID)
                                                                     .Where(new WhereCondition()
                                                                         .WhereNull("MultiBuyCouponCodeUseLimit")
                                                                         .Or()
                                                                         .WhereLessThan("MultiBuyCouponCodeUseCount", "MultiBuyCouponCodeUseLimit".AsColumn()))
                                                                     .TopN(1);

                    mCouponsUseLimitExceeded = HasCoupons && !nonExceededCoupon.HasResults();
                }

                return mCouponsUseLimitExceeded.Value;
            }
        }


        /// <summary>
        /// Indicates status of multi buy discount. It can be Disabled, Active, NotStarted of Finished.
        /// </summary>
        public virtual DiscountStatusEnum Status
        {
            get
            {
                if (!MultiBuyDiscountEnabled)
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

                if (MultiBuyDiscountValidFrom > DateTime.Now)
                {
                    return DiscountStatusEnum.NotStarted;
                }

                if (MultiBuyDiscountValidTo < DateTime.Now)
                {
                    return DiscountStatusEnum.Finished;
                }

                return EnumHelper.GetDefaultValue<DiscountStatusEnum>();
            }
        }


        /// <summary>
        /// Returns cached collection of <see cref="MultiBuyDiscountBrandInfo"/> which are connected with this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountBrandInfo> MultiBuyDiscountBrands
        {
            get
            {
                var key = $"{TypeInfo.ObjectType}|multibuydiscountbrands|{MultiBuyDiscountID}".ToLowerInvariant();

                InfoDataSet<MultiBuyDiscountBrandInfo> discountBrands = null;

                using (var cs = new CachedSection<InfoDataSet<MultiBuyDiscountBrandInfo>>(ref discountBrands, ECommerceSettings.ProvidersCacheMinutes, true, null, key))
                {
                    if (cs.LoadData)
                    {
                        discountBrands = MultiBuyDiscountBrandInfoProvider.GetMultiBuyDiscountBrands().WhereEquals("MultiBuyDiscountID", MultiBuyDiscountID).TypedResult;
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { $"{TypeInfo.ObjectType}|byid|{MultiBuyDiscountID}", $"{MultiBuyDiscountBrandInfo.TYPEINFO.ObjectType}|all" });
                        cs.Data = discountBrands;
                    }
                }

                return discountBrands;
            }
        }


        /// <summary>
        /// Gets included collection of <see cref="MultiBuyDiscountBrandInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountBrandInfo> MultiBuyDiscountIncludedBrands
        {
            get
            {
                return MultiBuyDiscountBrands.Where(i => i.BrandIncluded);
            }
        }


        /// <summary>
        /// Gets excluded collection of <see cref="MultiBuyDiscountBrandInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountBrandInfo> MultiBuyDiscountExcludedBrands
        {
            get
            {
                return MultiBuyDiscountBrands.Where(i => !i.BrandIncluded);
            }
        }


        /// <summary>
        /// Determines whether multi buy discount uses set of brands for application.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseBrands => MultiBuyDiscountBrands.Any();


        /// <summary>
        /// Returns cached collection of <see cref="MultiBuyDiscountCollectionInfo"/> which are connected with this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountCollectionInfo> MultiBuyDiscountCollections
        {
            get
            {
                var key = $"{TypeInfo.ObjectType}|multibuydiscountcollections|{MultiBuyDiscountID}".ToLowerInvariant();

                InfoDataSet<MultiBuyDiscountCollectionInfo> discountCollections = null;

                using (var cs = new CachedSection<InfoDataSet<MultiBuyDiscountCollectionInfo>>(ref discountCollections, ECommerceSettings.ProvidersCacheMinutes, true, null, key))
                {
                    if (cs.LoadData)
                    {
                        discountCollections = MultiBuyDiscountCollectionInfoProvider.GetMultiBuyDiscountCollections().WhereEquals("MultiBuyDiscountID", MultiBuyDiscountID).TypedResult;
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { $"{TypeInfo.ObjectType}|byid|{MultiBuyDiscountID}", $"{MultiBuyDiscountCollectionInfo.TYPEINFO.ObjectType}|all" });
                        cs.Data = discountCollections;
                    }
                }

                return discountCollections;
            }
        }


        /// <summary>
        /// Gets included collection of <see cref="MultiBuyDiscountCollectionInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountCollectionInfo> MultiBuyDiscountIncludedCollections
        {
            get
            {
                return MultiBuyDiscountCollections.Where(i => i.CollectionIncluded);
            }
        }


        /// <summary>
        /// Gets excluded collection of <see cref="MultiBuyDiscountCollectionInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountCollectionInfo> MultiBuyDiscountExcludedCollections
        {
            get
            {
                return MultiBuyDiscountCollections.Where(i => !i.CollectionIncluded);
            }
        }


        /// <summary>
        /// Determines whether multi buy discount uses set of collections for application.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseCollections => MultiBuyDiscountCollections.Any();


        /// <summary>
        /// Returns cached collection of <see cref="MultiBuyDiscountSKUInfo"/> which are connected with this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountSKUInfo> MultiBuyDiscountProducts
        {
            get
            {
                var key = $"{TypeInfo.ObjectType}|multibuydiscountproducts|{MultiBuyDiscountID}".ToLowerInvariant();

                InfoDataSet<MultiBuyDiscountSKUInfo> discountProducts = null;

                using (var cs = new CachedSection<InfoDataSet<MultiBuyDiscountSKUInfo>>(ref discountProducts, ECommerceSettings.ProvidersCacheMinutes, true, null, key))
                {
                    if (cs.LoadData)
                    {
                        discountProducts = MultiBuyDiscountSKUInfoProvider.GetMultiBuyDiscountSKUs().WhereEquals("MultiBuyDiscountID", MultiBuyDiscountID).TypedResult;
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { $"{TypeInfo.ObjectType}|byid|{MultiBuyDiscountID}", $"{MultiBuyDiscountSKUInfo.TYPEINFO.ObjectType}|all" });
                        cs.Data = discountProducts;
                    }
                }

                return discountProducts;
            }
        }


        /// <summary>
        /// Determines whether multi buy discount uses set of products for application.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseProducts => MultiBuyDiscountProducts.Any();


        /// <summary>
        /// Returns cached collection of <see cref="MultiBuyDiscountDepartmentInfo"/> which are connected with this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountDepartmentInfo> MultiBuyDiscountDepartments
        {
            get
            {
                var key = $"{TypeInfo.ObjectType}|multibuydiscountdepartments|{MultiBuyDiscountID}".ToLowerInvariant();

                InfoDataSet<MultiBuyDiscountDepartmentInfo> discountDepartments = null;

                using (var cs = new CachedSection<InfoDataSet<MultiBuyDiscountDepartmentInfo>>(ref discountDepartments, ECommerceSettings.ProvidersCacheMinutes, true, null, key))
                {
                    if (cs.LoadData)
                    {
                        discountDepartments = MultiBuyDiscountDepartmentInfoProvider.GetMultiBuyDiscountDepartments().WhereEquals("MultiBuyDiscountID", MultiBuyDiscountID).TypedResult;
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { $"{TypeInfo.ObjectType}|byid|{MultiBuyDiscountID}", $"{MultiBuyDiscountDepartmentInfo.TYPEINFO.ObjectType}|all" });
                        cs.Data = discountDepartments;
                    }
                }

                return discountDepartments;
            }
        }


        /// <summary>
        /// Determines whether multi buy discount uses set of departments for application.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseDepartments => MultiBuyDiscountDepartments.Any();


        /// <summary>
        /// Returns cached collection of <see cref="MultiBuyDiscountTreeInfo"/> which are connected with this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountTreeInfo> MultiBuyDiscountSections
        {
            get
            {
                var key = $"{TypeInfo.ObjectType}|multibuydiscounttrees|{MultiBuyDiscountID}".ToLowerInvariant();

                InfoDataSet<MultiBuyDiscountTreeInfo> discountSections = null;

                using (var cs = new CachedSection<InfoDataSet<MultiBuyDiscountTreeInfo>>(ref discountSections, ECommerceSettings.ProvidersCacheMinutes, true, null, key))
                {
                    if (cs.LoadData)
                    {
                        discountSections = MultiBuyDiscountTreeInfoProvider.GetMultiBuyDiscountTrees().WhereEquals("MultiBuyDiscountID", MultiBuyDiscountID).TypedResult;
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { $"{TypeInfo.ObjectType}|byid|{MultiBuyDiscountID}", $"{MultiBuyDiscountTreeInfo.TYPEINFO.ObjectType}|all" });
                        cs.Data = discountSections;
                    }
                }

                return discountSections;
            }
        }


        /// <summary>
        /// Gets included collection of <see cref="MultiBuyDiscountTreeInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountTreeInfo> MultiBuyDiscountIncludedSections
        {
            get
            {
                return MultiBuyDiscountSections.Where(i => i.NodeIncluded);
            }
        }


        /// <summary>
        /// Gets excluded collection of <see cref="MultiBuyDiscountTreeInfo"/> connected to this discount.
        /// </summary>
        public virtual IEnumerable<MultiBuyDiscountTreeInfo> MultiBuyDiscountExcludedSections
        {
            get
            {
                return MultiBuyDiscountSections.Where(i => !i.NodeIncluded);
            }
        }


        /// <summary>
        /// Determines whether multi buy discount uses set of sections for application.
        /// </summary>
        [RegisterProperty]
        public virtual bool UseSections => MultiBuyDiscountSections.Any();


        /// <summary>
        /// Indicates if multibuy discount uses coupon or has some coupons defined.
        /// </summary>
        [RegisterProperty]
        public virtual bool HasOrUsesCoupon
        {
            get
            {
                return MultiBuyDiscountUsesCoupons || HasCoupons;
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
                    mHasCoupons = MultiBuyCouponCodeInfoProvider.GetDiscountCouponCodes(MultiBuyDiscountID).TopN(1).HasResults();
                }
                return mHasCoupons.Value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountInfoProvider.DeleteMultiBuyDiscountInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountInfoProvider.SetMultiBuyDiscountInfo(this);
        }


        /// <summary>
        /// Invalidates the current discount.
        /// Stored query results are cleared because discount codes could been modified.
        /// </summary>
        /// <param name="keepThisInstanceValid">Indicates if the current instance remains valid.</param>
        protected override void Invalidate(bool keepThisInstanceValid)
        {
            base.Invalidate(keepThisInstanceValid);

            // Clear stored query results
            mCouponsUseLimitExceeded = null;
            mHasCoupons = null;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public MultiBuyDiscountInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty MultiBuyDiscountInfo object.
        /// </summary>
        public MultiBuyDiscountInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MultiBuyDiscountInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MultiBuyDiscountInfo(DataRow dr)
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
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_MODIFY, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "ICanUseDiscountCoupons"

        /// <summary>
        /// Indicates if discount is applicable only with discount coupon.
        /// </summary>
        public bool DiscountUsesCoupons
        {
            get
            {
                return MultiBuyDiscountUsesCoupons;
            }
            set
            {
                MultiBuyDiscountUsesCoupons = value;
            }
        }


        /// <summary>
        /// Indicates if given coupon code is suitable for this discount. Returns false if this discount has no codes assigned.
        /// </summary>
        /// <param name="couponCode">Code to be checked</param>
        /// <param name="ignoreUseLimit">Indicates if use limitation is to be ignored.</param>
        /// <returns></returns>
        public bool AcceptsCoupon(string couponCode, bool ignoreUseLimit)
        {
            return MultiBuyDiscountInfoProvider.AcceptsCoupon(this, couponCode, ignoreUseLimit);
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Returns if the multi buy discount is valid in specified date by checking <see cref="MultiBuyDiscountValidFrom"/> and <see cref="MultiBuyDiscountValidTo"/> properties.
        /// Returns <c>True</c> also if the properties are not specified (they return <see cref="DateTime.MinValue"/>).
        /// </summary>
        /// <param name="date">Date for validation</param>
        internal bool IsValidForDate(DateTime date)
        {
            return ((MultiBuyDiscountValidFrom == DateTimeHelper.ZERO_TIME) || (MultiBuyDiscountValidFrom <= date)) &&
                   ((MultiBuyDiscountValidTo == DateTimeHelper.ZERO_TIME) || (MultiBuyDiscountValidTo >= date));
        }

        #endregion
    }
}