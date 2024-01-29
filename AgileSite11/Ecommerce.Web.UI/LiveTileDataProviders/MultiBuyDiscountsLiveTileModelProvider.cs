using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ECOMMERCE, "BuyXGetYDiscounts", typeof(MultiBuyDiscountsLiveTileModelProvider))]

namespace CMS.Ecommerce.Web.UI
{
    internal class MultiBuyDiscountsLiveTileModelProvider : AbstractCountLiveTileModelProvider
    {
        /// <summary>
        /// Gets total number of multibuy discounts running in given time.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        /// <returns>Number of active multibuy discounts.</returns>
        protected override int GetCount(LiveTileContext context)
        {
            return MultiBuyDiscountInfoProvider.GetRunningMultiBuyDiscounts(context.SiteInfo.SiteID, DateTime.Now).Count;
        }


        /// <summary>
        /// Returns description in singular or plural according to given count.
        /// </summary>
        /// <param name="count">Number of running multibuy discounts.</param>
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
            return new CacheSettings(2, "MultiBuyDiscountsLiveTileModelProvider", context.SiteInfo.SiteID)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { MultiBuyDiscountInfo.OBJECT_TYPE + "|all" })
            };
        }
    }
}
