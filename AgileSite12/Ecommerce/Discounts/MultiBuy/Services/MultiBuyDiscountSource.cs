using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of multibuy discounts.
    /// </summary>
    internal class MultiBuyDiscountSource : AbstractCartItemDiscountSource, IMultiBuyDiscountSource
    {
        public MultiBuyDiscountSource(IRoundingServiceFactory roundingServiceFactory, ISiteMainCurrencySource mainCurrencySource, ICurrencyConverterFactory converterFactory)
            : base(roundingServiceFactory, mainCurrencySource, converterFactory)
        {
        }


        /// <summary>
        /// Returns multibuy discounts for the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Other parameters used to filter discounts.</param>
        public IEnumerable<IMultiBuyDiscount> GetDiscounts(DiscountsParameters parameters)
        {
            var applicableMBDiscounts = GetMultiBuyDiscounts(parameters).ToList();

            return CombineWithCouponCodes(applicableMBDiscounts, parameters);
        }


        private static IEnumerable<MultiBuyDiscountInfo> GetMultiBuyDiscounts(DiscountsParameters parameters)
        {
            var discounts = CacheHelper.Cache(() => GetDiscountsForCache(parameters, () => MultiBuyDiscountInfoProvider.GetMultiBuyDiscounts(parameters.SiteID)),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetMultibuyDiscounts", GetCacheKeyPart(parameters))
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        MultiBuyDiscountInfo.OBJECT_TYPE + "|all"
                    })
                });

            return FilterByParameters(discounts, parameters);
        }


        private IEnumerable<IMultiBuyDiscount> CombineWithCouponCodes(ICollection<MultiBuyDiscountInfo> discounts, DiscountsParameters parameters)
        {
            var coupons = parameters.CouponCodes.GetAllDistinctCouponCodes();
            var mbCodes = GetMultiBuyCouponCodes(discounts, coupons);

            foreach (var mbDiscount in discounts)
            {
                if (!mbDiscount.DiscountUsesCoupons)
                {
                    yield return GetMultiBuyDiscount(mbDiscount, parameters);
                }

                var code = GetAcceptedCode(mbDiscount, mbCodes, parameters.CouponCodes);
                if (code == null)
                {
                    continue;
                }

                yield return GetMultiBuyDiscount(mbDiscount, parameters, code);
            }
        }
    }
}