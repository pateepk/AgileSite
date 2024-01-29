using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ECOMMERCE, "ShippingDiscounts", typeof(FreeShippingOffersLiveTileModelProvider))]

namespace CMS.Ecommerce.Web.UI
{
    internal class FreeShippingOffersLiveTileModelProvider : AbstractCountLiveTileModelProvider
    {
        /// <summary>
        /// Gets total number of free shipping offers running in given time.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        /// <returns>Number of active free shipping offers.</returns>
        protected override int GetCount(LiveTileContext context)
        {
            return DiscountInfoProvider.GetRunningDiscounts(context.SiteInfo.SiteID, DateTime.Now, DiscountApplicationEnum.Shipping).Count;
        }


        /// <summary>
        /// Returns description in singular or plural according to given count.
        /// </summary>
        /// <param name="count">Number of running free shipping offers.</param>
        protected override string GetDescription(int count)
        {
            var key = (count == 1) ? "com.discount.runningoffer" : "com.discount.runningoffers";

            return ResHelper.GetString(key);
        }


        /// <summary>
        /// Returns settings used for caching results.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        protected override CacheSettings GetCacheSettings(LiveTileContext context)
        {
            return new CacheSettings(2, "FreeShippingOffersLiveTileModelProvider", context.SiteInfo.SiteID)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { DiscountInfo.OBJECT_TYPE + "|all" })
            };
        }
    }
}

