using System;

using CMS.Activities;
using CMS.Base;
using CMS.DataEngine;
using CMS.WebAnalytics.Internal;

using Kentico.Content.Web.Mvc.Routing;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Modifies URL of <see cref="PredefinedActivityType.PAGE_VISIT"/> activity performed on content only site. This includes replacing of possible alternative URL with URL of main document and computation of URL hash.
    /// </summary>
    internal class UrlActivityModifier : IActivityModifier
    {
        private readonly ISiteService mSiteService;
        private readonly IAlternativeUrlsService mAlternativeUrlsService;
        private readonly IActivityUrlHashService mHashService;


        /// <summary>
        /// Creates instance of <see cref="UrlActivityModifier"/>.
        /// </summary>
        /// <param name="siteService">Site service.</param>
        /// <param name="alternativeUrlsService">Service providing work with alternative URLs.</param>
        /// <param name="hashService">Provides hash computation for activity URL.</param>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="siteService"/> or <paramref name="alternativeUrlsService"/> is <c>null</c>.</exception>
        public UrlActivityModifier(ISiteService siteService, IAlternativeUrlsService alternativeUrlsService, IActivityUrlHashService hashService)
        {
            mSiteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            mAlternativeUrlsService = alternativeUrlsService ?? throw new ArgumentNullException(nameof(alternativeUrlsService));
            mHashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }


        /// <summary>
        /// Replaces alternative URL in given <paramref name="activity"/> with main URL if Alternative URLs feature is in rewrite mode. Then computes hash of <see cref="PredefinedActivityType.PAGE_VISIT"/> activity URL.
        /// </summary>
        public void Modify(IActivityInfo activity)
        {
            if (String.Equals(SettingsKeyInfoProvider.GetValue("CMSAlternativeURLsMode", mSiteService.CurrentSite.SiteID), "rewrite", StringComparison.OrdinalIgnoreCase))
            {
                activity.ActivityURL = GetMainUrl(activity.ActivityURL);
            }

            if (PredefinedActivityType.PAGE_VISIT.Equals(activity.ActivityType, StringComparison.OrdinalIgnoreCase))
            {
                activity.ActivityURLHash = mHashService.GetActivityUrlHash(activity.ActivityURL);
            }
        }


        /// <summary>
        /// Returns main URL for given <paramref name="alternativeUrl"/> if main URL exists.
        /// </summary>
        private string GetMainUrl(string alternativeUrl)
        {
            if (!Uri.TryCreate(alternativeUrl, UriKind.Absolute, out var uri))
            {
                return alternativeUrl;
            }

            if (!uri.TryGetRelativePath(out var relativePath))
            {
                return alternativeUrl;
            }

            var mainDocumentData = mAlternativeUrlsService.GetMainDocumentData(relativePath);
            if (mainDocumentData != null)
            {
                var host = uri.GetLeftPart(UriPartial.Authority);
                var applicationPath = SystemContext.ApplicationPath.TrimEnd('/');
                var mainPath = mainDocumentData.Url.TrimStart('~').TrimStart('/');
                var queryString = uri.Query;

                return $"{host}{applicationPath}/{mainPath}{queryString}";
            }

            return alternativeUrl;
        }
    }
}
