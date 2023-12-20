using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine.Internal;

using Kentico.Content.Web.Mvc.Routing;

[assembly: RegisterImplementation(typeof(IAlternativeUrlsService), typeof(AlternativeUrlsService), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Service for using alternative URLs.
    /// </summary>
    public class AlternativeUrlsService : IAlternativeUrlsService
    {
        private readonly ISiteService mSiteService;
        private readonly IAlternativeUrlsCache mAlternativeUrlsCache;


        /// <summary>
        /// Creates instance of <see cref="AlternativeUrlsService"/>.
        /// </summary>
        /// <param name="siteService">Site service.</param>
        /// <param name="alternativeUrlsCache">Caching for alternative URLs.</param>
        public AlternativeUrlsService(ISiteService siteService, IAlternativeUrlsCache alternativeUrlsCache)
        {
            mSiteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            mAlternativeUrlsCache = alternativeUrlsCache ?? throw new ArgumentNullException(nameof(alternativeUrlsCache));
        }


        /// <summary>
        /// Returns data of main document for given <paramref name="alternativeUrl"/>. <paramref name="alternativeUrl"/> is normalized and checked whether matches any excluded URLs and should not be processed.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL for which main document data is retrieved.</param>
        /// <returns>Data of main document if such is found, otherwise <c>null</c>.</returns>
        public MainDocumentData GetMainDocumentData(string alternativeUrl)
        {
            var normalizedUrl = AlternativeUrlHelper.NormalizeAlternativeUrl(alternativeUrl);
            if (!String.IsNullOrEmpty(normalizedUrl.NormalizedUrl) && AlternativeUrlHelper.MatchesAnyExcludedUrl(normalizedUrl, mSiteService.CurrentSite.SiteID))
            {
                return null;
            }

            return mAlternativeUrlsCache.GetDocumentData(normalizedUrl);
        }
    }
}
