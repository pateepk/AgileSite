using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ECOMMERCE, "CatalogDiscounts", typeof(CatalogDiscountsLiveTileModelProvider))]

namespace CMS.Ecommerce.Web.UI
{
    internal class CatalogDiscountsLiveTileModelProvider : AbstractCountLiveTileModelProvider
    {
        /// <summary>
        /// Gets total number of catalog discounts running in given time.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        /// <returns>Number of active catalog discounts.</returns>
        protected override int GetCount(LiveTileContext context)
        {
            return DiscountInfoProvider.GetRunningDiscounts(context.SiteInfo.SiteID, DateTime.Now, DiscountApplicationEnum.Products).Count;
        }


        /// <summary>
        /// Returns description in singular or plural according to given count.
        /// </summary>
        /// <param name="count">Number of running catalog discounts.</param>
        protected override string GetDescription(int count)
        {
            var key = (count == 1) ? "com.discount.runningdiscount" : "com.discount.runningdiscounts";

            return ResHelper.GetString(key);
        }


        /// <summary>
        /// Returns settings used for caching results.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        protected override CacheSettings GetCacheSettings(LiveTileContext context)
        {
            return new CacheSettings(2, "CatalogDiscountsLiveTileModelProvider", context.SiteInfo.SiteID)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { DiscountInfo.OBJECT_TYPE + "|all" })
            };
        }
    }
}


