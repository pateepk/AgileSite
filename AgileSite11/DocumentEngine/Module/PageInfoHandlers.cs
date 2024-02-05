using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides handlers for PageInfo.
    /// </summary>
    internal static class PageInfoHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            SiteDomainAliasInfo.TYPEINFO.Events.Update.After += ClearPageInfoCache;
        }


        private static void ClearPageInfoCache(object sender, ObjectEventArgs eventArgs)
        {
            var siteDomainAlias = eventArgs.Object as SiteDomainAliasInfo;
            if (siteDomainAlias != null)
            {
                PageInfoCacheHelper.ClearCache(new SiteInfoIdentifier(siteDomainAlias.SiteID));
            }
        }
    }
}
