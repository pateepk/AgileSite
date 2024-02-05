using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.Internal;

namespace CMS.ContactManagement
{
    internal class ContactProcessingChecker : IContactProcessingChecker
    {
        internal ILicenseService mLicenseService;
        private readonly ICurrentCookieLevelProvider mCookieLevelProvider;
        private readonly ICrawlerChecker mCrawlerChecker;
        private readonly ISiteService mSiteService;
        private readonly ISettingsService mSettingsService;


        public ContactProcessingChecker(ICurrentCookieLevelProvider cookieLevelProvider, ICrawlerChecker crawlerChecker, ISiteService siteService, ISettingsService settingsService)
        {
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
            mCookieLevelProvider = cookieLevelProvider;
            mCrawlerChecker = crawlerChecker;
            mSiteService = siteService;
            mSettingsService = settingsService;
        }


        /// <summary>
        /// Checks whether the contact processing can continue in the context of current HTTP request.
        /// </summary>
        /// <returns><c>True</c> if the contact processing can continue; otherwise, <c>false</c></returns>
        public bool CanProcessContactInCurrentContext()
        {
            return IsOnlineMarketingEnabled() &&
                   !mCrawlerChecker.IsCrawler() &&
                   CookieLevelIsAtLeastVisitor() &&
                   mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement);
        }


        private bool IsOnlineMarketingEnabled()
        {
            return mSettingsService[mSiteService.CurrentSite?.SiteName + ".CMSEnableOnlineMarketing"].ToBoolean(false);
        }


        private bool CookieLevelIsAtLeastVisitor()
        {
            var currentCookieLevel = mCookieLevelProvider.GetCurrentCookieLevel();
            return currentCookieLevel >= CookieLevel.Visitor;
        }
    }
}