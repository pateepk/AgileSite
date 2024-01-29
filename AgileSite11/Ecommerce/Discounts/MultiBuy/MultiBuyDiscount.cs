using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class implementing multibuy discount from calculation point of view.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    [Serializable]
    public sealed class MultiBuyDiscount : IMultiBuyDiscount
    {
        /// <summary>
        /// Inner discount object.
        /// </summary>
        private readonly MultiBuyDiscountInfo discount;
        private const decimal AUTOADDPERCENTAGELIMIT = 100m;
        [NonSerialized]
        private readonly Func<decimal, decimal> mValueCalculator;


        /// <summary>
        /// Unique Guid identifier of the discount.
        /// </summary>
        public Guid DiscountGuid => discount.MultiBuyDiscountGUID;


        /// <summary>
        /// Multibuy discount display name.
        /// </summary>
        public string DiscountName => discount.MultiBuyDiscountDisplayName;


        /// <summary>
        /// Applied coupon code.
        /// </summary>
        public string AppliedCouponCode
        {
            get;
            set;
        }


        /// <summary>
        /// Discount value. Value is percentage of fix, based on value of <see cref="IsFlat"/>.
        /// </summary>
        public decimal Value => discount.MultiBuyDiscountValue;


        /// <summary>
        /// True - discount value is fix, False - discount value is relative.
        /// </summary>
        public bool IsFlat => discount.MultiBuyDiscountIsFlat;


        /// <summary>
        /// The number of products needed to enable this discount.
        /// </summary>
        public int BasedOnUnitsCount => discount.MultiBuyDiscountMinimumBuyCount;


        /// <summary>
        /// The number of discounted units.
        /// </summary>
        public int ApplyOnUnitsCount => 1;


        /// <summary>
        /// Indicates if further discounts are to be applied if this discount applies.
        /// </summary>
        public bool ApplyFurtherDiscounts => discount.MultiBuyDiscountApplyFurtherDiscounts;


        /// <summary>
        /// Indicates if product is added to cart automatically, the system adds product to shopping cart only when the discount is percentage and set 100 % off.
        /// </summary>
        public bool AutoAddEnabled => discount.MultiBuyDiscountAutoAddEnabled
                                      && !discount.MultiBuyDiscountIsFlat
                                      && discount.MultiBuyDiscountValue.Equals(AUTOADDPERCENTAGELIMIT);


        /// <summary>
        /// Maximum number of possible usages of the discount. Zero or negative value is representing an unlimited application count.
        /// </summary>
        public int MaxApplication => discount.MultiBuyDiscountLimitPerOrder;


        /// <summary>
        /// Creates a new instance of relative <see cref="MultiBuyDiscount"/> based on the specified <see cref="MultiBuyDiscountInfo"/>.
        /// </summary>
        /// <param name="multiBuyDiscountInfo">MultiBuyDiscountInfo object to create instance for.</param>
        /// <param name="valueCalculator">Service used to round the result.</param>
        /// <param name="appliedCouponCode">Applied coupon code which.</param>
        public MultiBuyDiscount(MultiBuyDiscountInfo multiBuyDiscountInfo, Func<decimal, decimal> valueCalculator, string appliedCouponCode = null)
        {
            discount = multiBuyDiscountInfo;
            mValueCalculator = valueCalculator;
            AppliedCouponCode = appliedCouponCode;
        }


        /// <summary>
        /// Creates new instance of absolute <see cref="MultiBuyDiscount"/> based on the specified <see cref="MultiBuyDiscountInfo"/>.
        /// </summary>
        /// <param name="multiBuyDiscountInfo">MultiBuyDiscountInfo object to create instance for.</param>
        /// <param name="value">Absolute value of the discount.</param>
        /// <param name="appliedCouponCode">Applied coupon code.</param>
        public MultiBuyDiscount(MultiBuyDiscountInfo multiBuyDiscountInfo, decimal value, string appliedCouponCode = null)
        {
            discount = multiBuyDiscountInfo;
            mValueCalculator = price => (value > price) ? price : value;
            AppliedCouponCode = appliedCouponCode;
        }


        /// <summary>
        /// Moves Y items to higher priorities in case of BXGY discount.
        /// </summary>
        /// <param name="items">Cart items to prioritize.</param>
        public void PrioritizeItems(List<MultiBuyItem> items)
        {
            var discountedSKUID = discount.MultiBuyDiscountApplyToSKUID;
            if (discountedSKUID > 0)
            {
                var Ys = items.FindAll(item => item.SKU.SKUID == discountedSKUID);
                items.RemoveAll(item => item.SKU.SKUID == discountedSKUID);
                items.InsertRange(0, Ys);
            }
        }


        /// <summary>
        /// Indicates if this discount is based on given cart item, i.e. this method returns true for items needed 
        /// to be in the cart to be eligible to get this discount.
        /// </summary>
        /// <param name="item">Item to check.</param>
        public bool IsBasedOn(MultiBuyItem item)
        {
            // Skip query (optimization) for MultiBuyDiscountSKU binding check in case of valid department, brand, collection or product section check
            return IsBasedOnSKU(item) || IsBasedOnDepartment(item) || IsBasedOnBrand(item) || IsBasedOnCollection(item) || IsBasedOnSection(item);
        }


        private bool IsBasedOnDepartment(MultiBuyItem item)
        {
            return item.SKU.SKUDepartmentID > 0
                && discount.MultiBuyDiscountDepartments.Any(d => d.DepartmentID == item.SKU.SKUDepartmentID)
                && discount.MultiBuyDiscountExcludedBrands.All(b => b.BrandID != item.SKU.SKUBrandID)
                && discount.MultiBuyDiscountExcludedCollections.All(c => c.CollectionID != item.SKU.SKUCollectionID)
                && discount.MultiBuyDiscountExcludedSections.All(s => !item.SKU.SectionIDs.Contains(s.NodeID));
        }


        private bool IsBasedOnBrand(MultiBuyItem item)
        {
            return item.SKU.SKUBrandID > 0
                && discount.MultiBuyDiscountIncludedBrands.Any(b => b.BrandID == item.SKU.SKUBrandID)
                && discount.MultiBuyDiscountExcludedCollections.All(c => c.CollectionID != item.SKU.SKUCollectionID)
                && discount.MultiBuyDiscountExcludedSections.All(s => !item.SKU.SectionIDs.Contains(s.NodeID));
        }


        private bool IsBasedOnCollection(MultiBuyItem item)
        {
            return item.SKU.SKUCollectionID > 0
                && discount.MultiBuyDiscountIncludedCollections.Any(c => c.CollectionID == item.SKU.SKUCollectionID)
                && discount.MultiBuyDiscountExcludedBrands.All(b => b.BrandID != item.SKU.SKUBrandID)
                && discount.MultiBuyDiscountExcludedSections.All(s => !item.SKU.SectionIDs.Contains(s.NodeID));
        }


        private bool IsBasedOnSection(MultiBuyItem item)
        {
            var discountIncludedSections = discount.MultiBuyDiscountIncludedSections.Select(i => i.NodeID);
            var discountExcludedSections = discount.MultiBuyDiscountExcludedSections.Select(i => i.NodeID);
            var productSections = item.SKU.SectionIDs.ToList();

            return productSections.Any()
                   && discountIncludedSections.Intersect(productSections).Any()
                   && !discountExcludedSections.Intersect(productSections).Any()
                   && discount.MultiBuyDiscountExcludedCollections.All(c => c.CollectionID != item.SKU.SKUCollectionID)
                   && discount.MultiBuyDiscountExcludedBrands.All(b => b.BrandID != item.SKU.SKUBrandID);
        }


        private bool IsBasedOnSKU(MultiBuyItem item)
        {
            return discount.MultiBuyDiscountProducts.Any(p => p.SKUID == item.SKU.SKUID);
        }


        /// <summary>
        /// Indicates if this discount is affecting the price of given cart item, i.e. this method returns true for items 
        /// discounted by this discount.
        /// </summary>
        /// <param name="item">Item to check.</param>
        public bool IsApplicableOn(MultiBuyItem item)
        {
            // Check if Y product is not selected to determine kind of discount (BOGO/BXGY).
            if (discount.MultiBuyDiscountApplyToSKUID == 0)
            {
                // In case of BOGO discount ApplicableOn rule is equal to BasedOn one.
                return IsBasedOn(item);
            }

            // Check if item's SKU is selected as Y product in discount definition (BXGY discount)
            return (discount.MultiBuyDiscountApplyToSKUID == item.SKU.SKUID);
        }


        /// <summary>
        /// Returns IDs of SKUs which could be discounted if present in cart. Most important products go first.
        /// </summary>
        public IEnumerable<int> GetMissingProducts()
        {
            if (discount.MultiBuyDiscountApplyToSKUID > 0)
            {
                yield return discount.MultiBuyDiscountApplyToSKUID;
            }
        }
        

        /// <summary>
        /// Calculates the discount value for given <paramref name="basePrice"/>.
        /// </summary>
        /// <param name="basePrice">The price to calculate discount from.</param>
        /// <returns>Rounded discount value.</returns>
        public decimal CalculateDiscount(decimal basePrice)
        {
            return mValueCalculator(basePrice);
        }


        /// <summary>
        /// Indicates if discount is applicable only with discount coupon.
        /// </summary>
        public bool DiscountUsesCoupons
        {
            get
            {
                return discount.DiscountUsesCoupons;
            }
            set
            {
                discount.DiscountUsesCoupons = value;
            }
        }


        /// <summary>
        /// Indicates if given coupon code is suitable for this discount. Returns false if this discount has no codes assigned.
        /// </summary>
        /// <param name="couponCode">Code to be checked</param>
        /// <param name="ignoreUseLimit">Indicates if use limitation is to be ignored.</param>
        public bool AcceptsCoupon(string couponCode, bool ignoreUseLimit)
        {
            return discount.AcceptsCoupon(couponCode, ignoreUseLimit);
        }


        /// <summary>
        /// Logs discount usage.
        /// </summary>
        public void Apply()
        {
            if ((!discount.MultiBuyDiscountUsesCoupons) || string.IsNullOrEmpty(AppliedCouponCode))
            {
                return;
            }

            // Log coupon code use only if not exceeded
            var coupon = MultiBuyCouponCodeInfoProvider.GetDiscountCouponCode(discount.MultiBuyDiscountID, AppliedCouponCode);
            if ((coupon != null) && !coupon.UseLimitExceeded)
            {
                coupon.MultiBuyCouponCodeUseCount++;
                coupon.Update();
            }
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{DiscountName} ({Value}{(IsFlat ? String.Empty : "%")})";
    }
}
