using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    internal class OrderDiscountSource : IOrderDiscountSource
    {
        private readonly ICurrencyConverterFactory mConverterFactory;
        private readonly ISiteMainCurrencySource mSiteMainCurrencySource;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;


        /// <summary>
        /// Creates a new instance of <see cref="OrderDiscountSource"/>.
        /// </summary>
        /// <param name="converterFactory">Factory for fetching currency converters by site.</param>
        /// <param name="siteMainCurrencySource">Main currency source.</param>
        /// <param name="roundingServiceFactory">Rounding service.</param>
        public OrderDiscountSource(ICurrencyConverterFactory converterFactory, ISiteMainCurrencySource siteMainCurrencySource, IRoundingServiceFactory roundingServiceFactory)
        {
            mConverterFactory = converterFactory;
            mSiteMainCurrencySource = siteMainCurrencySource;
            mRoundingServiceFactory = roundingServiceFactory;
        }


        /// <summary>
        /// Returns the order discounts for the specified <paramref name="data"/> grouped by their priority.
        /// Applied order discounts must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the discount conditions.
        /// Only order discounts satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderPrice">Order price which is used to filter applicable order discounts. (specified in the calculation currency)</param>
        public IEnumerable<DiscountCollection> GetDiscounts(CalculatorData data, decimal orderPrice)
        {
            var request = data.Request;

            // Calculate order price threshold to the main currency
            var mainCurrency = mSiteMainCurrencySource.GetSiteMainCurrencyCode(request.Site);
            var totalPriceInMainCurrency = mConverterFactory.GetCurrencyConverter(request.Site).Convert(orderPrice, request.Currency.CurrencyCode, mainCurrency);

            // Get discounts that might be applied
            var applicableDiscounts = GetApplicableOrderDiscounts(request).ToList();

            // Load coupons that might be used for applicable discounts
            var couponCodes = GetCoupons(request.CouponCodes.GetAllDistinctCouponCodes(), applicableDiscounts).ToList();

            // Filter applicable discounts by applied coupon codes and priority
            var discounts = GetFilteredOrderDiscounts(data, applicableDiscounts, couponCodes, totalPriceInMainCurrency).ToList();

            return discounts
                .GroupBy(discount => discount.DiscountItemOrder)
                .Select(group => CombineWithCouponCodes(group, couponCodes, request))
                .Select(group => new DiscountCollection(group))
                .ToList();
        }


        /// <summary>
        /// Combines the specified <paramref name="discounts"/> with <see cref="CalculationRequest.CouponCodes"/> 
        /// and returns Order discount applications.
        /// </summary>
        /// <param name="discounts">Order discounts.</param>
        /// <param name="couponCodes">Coupon codes pre-selected for given <paramref name="discounts"/>.</param>
        /// <param name="request">Calculation request data.</param>
        private IEnumerable<IDiscount> CombineWithCouponCodes(IEnumerable<DiscountInfo> discounts, IEnumerable<CouponCodeInfo> couponCodes, CalculationRequest request)
        {
            foreach (var discount in discounts)
            {
                if (discount.DiscountUsesCoupons)
                {
                    var code = GetAcceptedCode(discount, couponCodes, request.CouponCodes);
                    if (code != null)
                    {
                        yield return GetOrderDiscount(discount, code.CouponCodeCode, request);
                    }
                }
                else
                {
                    yield return GetOrderDiscount(discount, null, request);
                }
            }
        }


        private IEnumerable<CouponCodeInfo> GetCoupons(ICollection<string> codes, ICollection<DiscountInfo> discounts)
        {
            if (codes.Any())
            {
                return CouponCodeInfoProvider.GetCouponCodes()
                                             .WhereIn("CouponCodeDiscountID", discounts.Select(g => g.DiscountID).ToList())
                                             .WhereIn("CouponCodeCode", codes)
                                             .ToList();
            }

            return Enumerable.Empty<CouponCodeInfo>();
        }


        private IDiscount GetOrderDiscount(DiscountInfo discount, string code, CalculationRequest request)
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


        private IEnumerable<DiscountInfo> GetFilteredOrderDiscounts(CalculatorData data, IEnumerable<DiscountInfo> applicableDiscounts, IEnumerable<CouponCodeInfo> couponCodes, decimal orderPrice)
        {
            // Filter applicable discounts based on coupons
            var discounts = applicableDiscounts
                .Where(d => !d.DiscountUsesCoupons || (d.DiscountUsesCoupons && (GetAcceptedCode(d, couponCodes, data.Request.CouponCodes) != null)));

            // Filter applicable discounts based on priority
            var discountFilter = new CartDiscountsFilter(data);

            return discountFilter
                .Filter(discounts, orderPrice)
                .OfType<DiscountInfo>();
        }


        private IEnumerable<DiscountInfo> GetApplicableOrderDiscounts(CalculationRequest request)
        {
            int siteID = request.Site;

            var discounts = CacheHelper.Cache(() => GetOrderDiscounts(siteID),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetOrderDiscounts", siteID)
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


        private IEnumerable<DiscountInfo> GetOrderDiscounts(int siteID)
        {
            return DiscountInfoProvider.GetDiscounts(siteID, true)
                                       .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Order.ToStringRepresentation())
                                       .OrderBy("DiscountOrder")
                                       .ToList();
        }


        private CouponCodeInfo GetAcceptedCode(DiscountInfo discount, IEnumerable<CouponCodeInfo> coupons, IReadOnlyCouponCodeCollection requestCouponCodes)
        {
            var codes = coupons.Where(c =>
                                    c.CouponCodeDiscountID == discount.DiscountID
                                    && (requestCouponCodes.OrderAppliedCodes.Select(x => x.Code).Contains(c.CouponCodeCode, ECommerceHelper.CouponCodeComparer)
                                    || !c.UseLimitExceeded))
                                .ToList();

            var alreadyAppliedCode = codes.FirstOrDefault(c => requestCouponCodes.AllAppliedCodes.Select(i => i.Code).Contains(c.CouponCodeCode, ECommerceHelper.CouponCodeComparer));

            if (alreadyAppliedCode != null)
            {
                return alreadyAppliedCode;
            }

            return codes.FirstOrDefault();
        }
    }
}