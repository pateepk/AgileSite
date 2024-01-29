using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of shipping discounts.
    /// </summary>
    internal class ShippingDiscountSource : IShippingDiscountSource
    {
        private readonly ICurrencyConverterFactory mConverterFactory;
        private readonly ISiteMainCurrencySource mSiteMainCurrencySource;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;


        /// <summary>
        /// Creates a new instance of <see cref="ShippingDiscountSource"/>.
        /// </summary>
        /// <param name="converterFactory">Factory for fetching currency converters by site.</param>
        /// <param name="siteMainCurrencySource">Main currency source.</param>
        /// <param name="roundingServiceFactory">Rounding service.</param>
        public ShippingDiscountSource(ICurrencyConverterFactory converterFactory, ISiteMainCurrencySource siteMainCurrencySource, IRoundingServiceFactory roundingServiceFactory)
        {
            mConverterFactory = converterFactory;
            mSiteMainCurrencySource = siteMainCurrencySource;
            mRoundingServiceFactory = roundingServiceFactory;
        }


        /// <summary>
        /// Returns the shipping discounts for the specified <paramref name="data"/>.
        /// Applied shipping discounts must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the discount conditions.
        /// Only shipping discounts satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Order amount which is used to filter applicable shipping discounts. (specified in the calculation currency)</param>
        public IEnumerable<IDiscount> GetDiscounts(CalculatorData data, decimal orderAmount)
        {
            var request = data.Request;

            // Calculate order price threshold to the main currency
            var mainCurrency = mSiteMainCurrencySource.GetSiteMainCurrencyCode(request.Site);
            var totalPriceInMainCurrency = mConverterFactory.GetCurrencyConverter(request.Site).Convert(orderAmount, request.Currency.CurrencyCode, mainCurrency);

            var shippingDiscounts = GetFilteredShippingDiscounts(data, totalPriceInMainCurrency).ToList();
            return CombineWithCouponCodes(shippingDiscounts, request);
        }


        /// <summary>
        /// Returns remaining amount for free shipping. 
        /// Method checks applicable free shipping offers which <see cref="DiscountInfo.DiscountOrderAmount"/> 
        /// is larger than <paramref name="orderAmount"/> and returns additional amount to reach free shipping offer.
        /// Method returns 0 if there is no valid discount or if free shipping is already applied.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Current order amount. (specified in the calculation currency)</param>
        public decimal GetRemainingAmountForFreeShipping(CalculatorData data, decimal orderAmount)
        {
            var request = data.Request;

            var mainCurrency = mSiteMainCurrencySource.GetSiteMainCurrencyCode(request.Site);
            var discounts = GetFilteredShippingDiscounts(data);
            var codes = GetCoupons(request.CouponCodes.Codes.Select(x => x.Code).ToList(), discounts);

            // Filter codes
            discounts = discounts.Where(d => !d.DiscountUsesCoupons || (GetAcceptedCode(d, codes) != null)).ToList();

            // Get currency converter
            var converter = mConverterFactory.GetCurrencyConverter(request.Site);

            // Find the lowest Minimum order amount
            decimal minOrderAmount = -1m;
            foreach (var discount in discounts)
            {
                var discountItemMinOrderAmount = converter.Convert(discount.DiscountOrderAmount, mainCurrency, request.Currency.CurrencyCode);

                if ((minOrderAmount > discountItemMinOrderAmount) || (minOrderAmount < 0m))
                {
                    minOrderAmount = discountItemMinOrderAmount;
                }
            }

            var roundingService = mRoundingServiceFactory.GetRoundingService(request.Site);

            minOrderAmount = roundingService.Round(minOrderAmount, request.Currency);

            // Check if the Free shipping hasn't already applied
            if (minOrderAmount > orderAmount)
            {
                // Get remaining amount in shopping cart currency
                return minOrderAmount - orderAmount;
            }

            return 0m;
        }


        /// <summary>
        /// Combines the specified <paramref name="shippingDiscounts"/> with <see cref="CalculationRequest.CouponCodes"/> 
        /// and returns Shipping discount applications.
        /// </summary>
        /// <param name="shippingDiscounts">Shipping discounts.</param>
        /// <param name="request">Calculation request data.</param>
        private IEnumerable<IDiscount> CombineWithCouponCodes(ICollection<DiscountInfo> shippingDiscounts, CalculationRequest request)
        {
            var couponCodes = GetCoupons(request.CouponCodes.Codes.Select(x => x.Code).Distinct(ECommerceHelper.CouponCodeComparer).ToList(), shippingDiscounts).ToList();

            foreach (var shippingDiscount in shippingDiscounts)
            {
                if (!shippingDiscount.DiscountUsesCoupons)
                {
                    yield return GetShippingDiscount(shippingDiscount, null, request);
                }

                var code = GetAcceptedCode(shippingDiscount, couponCodes, request.CouponCodes);
                if (code == null)
                {
                    continue;
                }
               
                yield return GetShippingDiscount(shippingDiscount, code, request);
            }
        }


        private IEnumerable<CouponCodeInfo> GetCoupons(IEnumerable<string> codes, ICollection<DiscountInfo> discounts)
        {
            var couponCodes = Enumerable.Empty<CouponCodeInfo>();

            if (codes.Any())
            {
                couponCodes = CouponCodeInfoProvider.GetCouponCodes()
                                                        .WhereIn("CouponCodeDiscountID", discounts.Select(g => g.DiscountID).ToList())
                                                        .WhereIn("CouponCodeCode", codes.ToList())
                                                        .ToList();
            }
            return couponCodes;
        }


        private IDiscount GetShippingDiscount(DiscountInfo discount, string code, CalculationRequest request)
        {
            var roundingService = mRoundingServiceFactory.GetRoundingService(request.Site);

            if (discount.DiscountIsFlat)
            {
                var discountCurrency = mSiteMainCurrencySource.GetSiteMainCurrencyCode(request.Site);
                var converter = mConverterFactory.GetCurrencyConverter(request.Site);

                var valueInCalculationCurrency = converter.Convert(discount.DiscountValue, discountCurrency, request.Currency.CurrencyCode);

                valueInCalculationCurrency = roundingService.Round(valueInCalculationCurrency, request.Currency);

                return new FixedDiscountApplication(discount.DiscountDisplayName, valueInCalculationCurrency, code, discount.LogUseOnce);
            }

            var rate = discount.DiscountValue / 100m;
            return new PercentageDiscountApplication(discount.DiscountDisplayName, rate, request.Currency, code, roundingService, discount.LogUseOnce);
        }


        private CartDiscountsFilter GetCartDiscountsFilter(CalculatorData data)
        {
            return new CartDiscountsFilter(data);
        }


        private ICollection<DiscountInfo> GetFilteredShippingDiscounts(CalculatorData data, decimal minOrderAmount = -1)
        {
            var applicableShippingDiscounts = GetApplicableShippingDiscounts(data.Request);
            var discountFilter = GetCartDiscountsFilter(data);

            return discountFilter.Filter(applicableShippingDiscounts, minOrderAmount).OfType<DiscountInfo>().ToList();
        }


        private IEnumerable<DiscountInfo> GetApplicableShippingDiscounts(CalculationRequest request)
        {
            int siteID = request.Site;

            var discounts = CacheHelper.Cache(() => GetShippingDiscounts(siteID),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetShippingDiscounts", siteID)
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        DiscountInfo.OBJECT_TYPE + "|all"
                    })
                });

            return discounts.Where(d => d.IsRunningDueDate(request.CalculationDate) && MeetsRestriction(d, request));
        }


        private static bool MeetsRestriction(DiscountInfo discount, CalculationRequest request)
        {
            var restriction = discount.DiscountCustomerRestriction;
            var discountRoles = discount.DiscountRoles;
            var user = request.User;
            var site = request.Site;

            return DiscountRestrictionHelper.CheckCustomerRestriction(restriction, discountRoles, user, site);
        }


        private IEnumerable<DiscountInfo> GetShippingDiscounts(int siteID)
        {
            return DiscountInfoProvider.GetDiscounts(siteID, true)
                                       .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Shipping.ToStringRepresentation())
                                       .OrderBy("DiscountOrder")
                                       .ToList();
        }


        private string GetAcceptedCode(DiscountInfo discount, IEnumerable<CouponCodeInfo> coupons, IReadOnlyCouponCodeCollection requestCouponCodes)
        {
            var codes = coupons.Where(c => 
                                    c.CouponCodeDiscountID == discount.DiscountID 
                                    && (requestCouponCodes.OrderAppliedCodes.Select(x => x.Code).Contains(c.CouponCodeCode, ECommerceHelper.CouponCodeComparer) 
                                    || !c.UseLimitExceeded))
                                .ToList();

            var appliedCouponCode = codes.FirstOrDefault(x => requestCouponCodes.AllAppliedCodes.Select(i => i.Code).Contains(x.CouponCodeCode, ECommerceHelper.CouponCodeComparer));

            if (appliedCouponCode != null)
            {
                return appliedCouponCode.CouponCodeCode;
            }

            return codes.FirstOrDefault()?.CouponCodeCode;
        }


        private string GetAcceptedCode(DiscountInfo discount, IEnumerable<CouponCodeInfo> coupons)
        {
            var code = coupons.FirstOrDefault(c => (c.CouponCodeDiscountID == discount.DiscountID) && !c.UseLimitExceeded);

            return code?.CouponCodeCode;
        }
    }
}