using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ECOMMERCE, "DiscountCoupons", typeof(ProductCouponDiscountsLiveTileModelProvider))]

namespace CMS.Ecommerce.Web.UI
{
    internal class ProductCouponDiscountsLiveTileModelProvider : AbstractCountLiveTileModelProvider
    {
        /// <summary>
        /// Gets total number of product coupon discounts running in given time.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        /// <returns>Number of active product coupon discounts.</returns>
        protected override int GetCount(LiveTileContext context)
        {
            return MultiBuyDiscountInfoProvider.GetRunningProductCouponDiscounts(context.SiteInfo.SiteID, DateTime.Now).Count;
        }


        /// <summary>
        /// Returns description in singular or plural according to given count.
        /// </summary>
        /// <param name="count">Number of running product coupon discounts.</param>
        protected override string GetDescription(int count)
        {
            var key = (count == 1) ? "com.discount.runningproductcoupondiscount" : "com.discount.runningproductcoupondiscounts";

            return ResHelper.GetString(key);
        }


        /// <summary>
        /// Returns settings used for caching results.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        protected override CacheSettings GetCacheSettings(LiveTileContext context)
        {
            return new CacheSettings(2, "ProductCouponDiscountsLiveTileModelProvider", context.SiteInfo.SiteID)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON + "|all" })
            };
        }
    }
}
