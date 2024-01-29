using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ECOMMERCE, "GiftCards", typeof(GiftCardsLiveTileModelProvider))]

namespace CMS.Ecommerce.Web.UI
{
    internal class GiftCardsLiveTileModelProvider : AbstractCountLiveTileModelProvider
    {
        /// <summary>
        /// Gets total number of gift cards running in given time.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        /// <returns>Number of active gift cards.</returns>
        protected override int GetCount(LiveTileContext context)
        {
            return GiftCardInfoProvider.GetRunningGiftCards(context.SiteInfo.SiteID, DateTime.Now).Count;
        }


        /// <summary>
        /// Returns description in singular or plural according to given count.
        /// </summary>
        /// <param name="count">Number of running gift cards.</param>
        protected override string GetDescription(int count)
        {
            var key = (count == 1) ? "com.giftcard.runninggiftcard" : "com.giftcard.runninggiftcards";

            return ResHelper.GetString(key);
        }


        /// <summary>
        /// Returns settings used for caching results.
        /// </summary>
        /// <param name="context">Live tile context.</param>
        protected override CacheSettings GetCacheSettings(LiveTileContext context)
        {
            return new CacheSettings(2, "GiftCardsLiveTileModelProvider", context.SiteInfo.SiteID)
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { GiftCardInfo.OBJECT_TYPE + "|all" })
            };
        }
    }
}