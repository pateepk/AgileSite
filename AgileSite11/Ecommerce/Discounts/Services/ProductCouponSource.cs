using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of product coupon discounts.
    /// </summary>
    internal class ProductCouponSource : AbstractCartItemDiscountSource, IProductCouponSource
    {
        public ProductCouponSource(IRoundingServiceFactory roundingServiceFactory, ISiteMainCurrencySource mainCurrencySource, ICurrencyConverterFactory converterFactory)
            : base(roundingServiceFactory, mainCurrencySource, converterFactory)
        {
        }


        /// <summary>
        /// Returns product coupon discounts for the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Parameters used to filter discounts.</param>
        public IEnumerable<IMultiBuyDiscount> GetDiscounts(DiscountsParameters parameters)
        {
            var applicableDiscounts = GetProductCouponDiscounts(parameters).ToList();

            return CombineWithCouponCodes(applicableDiscounts, parameters);
        }


        private static IEnumerable<MultiBuyDiscountInfo> GetProductCouponDiscounts(DiscountsParameters parameters)
        {
            var discounts = CacheHelper.Cache(() => GetDiscountsForCache(parameters, () => MultiBuyDiscountInfoProvider.GetProductCouponDiscounts(parameters.SiteID)),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetProductCouponDiscounts", GetCacheKeyPart(parameters))
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|all"
                    })
                });

            return FilterByParameters(discounts, parameters);
        }


        private IEnumerable<IMultiBuyDiscount> CombineWithCouponCodes(ICollection<MultiBuyDiscountInfo> discounts, DiscountsParameters parameters)
        {
            var couponCodes = parameters.CouponCodes.Codes.Select(x => x.Code).Distinct(ECommerceHelper.CouponCodeComparer).ToList();
            var codes = GetMultiBuyCouponCodes(discounts, couponCodes);

            foreach (var discount in discounts)
            {
                var code = GetAcceptedCode(discount, codes, parameters.CouponCodes);
                if (code == null)
                {
                    continue;
                }

                yield return GetMultiBuyDiscount(discount, parameters, code);
            }
        }
    }
}