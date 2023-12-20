using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Localization;

using Kentico.Content.Web.Mvc.Routing;

[assembly: RegisterImplementation(typeof(IAlternativeUrlsCache), typeof(AlternativeUrlsCache), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Provides caching for alternative URLs feature.
    /// </summary>
    public class AlternativeUrlsCache : IAlternativeUrlsCache
    {
        internal const string CACHE_ITEM_NAME = "DocumentDataForAlternativeUrl";
        private const int CACHE_DURATION_IN_MINUTES = 24 * 60;

        private readonly ISiteService mSiteService;


        /// <summary>
        /// Initializes new instance of <see cref="AlternativeUrlsCache"/>.
        /// </summary>
        public AlternativeUrlsCache(ISiteService siteService)
        {
            mSiteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }


        /// <summary>
        /// Returns data of document with given <paramref name="alternativeUrl"/>, if no such document exists returns <c>null</c>.
        /// </summary>
        /// <param name="alternativeUrl">Alternative relative URL.</param>
        public MainDocumentData GetDocumentData(NormalizedAlternativeUrl alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            if (mSiteService.CurrentSite == null)
            {
                return null;
            }

            var cacheSettings = new CacheSettings(CACHE_DURATION_IN_MINUTES, CACHE_ITEM_NAME, mSiteService.CurrentSite.SiteID, alternativeUrl.NormalizedUrl);

            return CacheHelper.Cache(() => GetDocumentData(cacheSettings, alternativeUrl), cacheSettings);
        }


        private MainDocumentData GetDocumentData(CacheSettings settings, NormalizedAlternativeUrl alternativeUrl)
        {
            var url = AlternativeUrlInfoProvider.GetAlternativeUrl(alternativeUrl, mSiteService.CurrentSite.SiteID);
            if (url != null)
            {
                var document = AlternativeUrlHelper.GetMainDocument(url);
                if (document != null)
                {
                    var documentUrl = DocumentURLProvider.GetUrl(document);

                    settings.CacheDependency = GetCacheDependencyForExistingAlternativeUrl(url, document);

                    return new MainDocumentData(documentUrl, document.DocumentCulture);
                }
            }

            settings.CacheDependency = GetCacheDependencyForNonexistingAlternativeUrl();

            return null;
        }


        /// <summary>
        /// Returns cache dependency for given <paramref name="alternativeUrl"/> and <paramref name="document" />.
        /// </summary>
        /// <returns>Instance of <see cref="CMSCacheDependency"/> representing all cache dependencies.</returns>
        protected virtual CMSCacheDependency GetCacheDependencyForExistingAlternativeUrl(AlternativeUrlInfo alternativeUrl, TreeNode document)
        {
            var cacheDependency = CacheHelper.GetCacheDependency(new[]
            {
                CacheHelper.GetCacheItemName(null, AlternativeUrlInfo.OBJECT_TYPE, "byid", alternativeUrl.AlternativeUrlID),
                CacheHelper.GetCacheItemName(null, PredefinedObjectType.DOCUMENTTYPE, "byname", document.ClassName),
                CacheHelper.GetCacheItemName(null, "documentid", document.DocumentID),
                CacheHelper.GetCacheItemName(null, CultureInfo.OBJECT_TYPE, "byid", CultureInfoProvider.GetCultureID(document.DocumentCulture))
            });

            return cacheDependency;
        }


        private CMSCacheDependency GetCacheDependencyForNonexistingAlternativeUrl()
        {
            var cacheDependency = CacheHelper.GetCacheDependency(new[]
            {
                CacheHelper.GetCacheItemName(null, AlternativeUrlInfo.OBJECT_TYPE, "all")
            });

            return cacheDependency;
        }
    }
}
