using System;
using System.Web;
using System.Web.Routing;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;

using RequestContext = CMS.Helpers.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Constraint to check the context of a document in the web site.
    /// </summary>
    public class CMSDocumentRouteConstraint : BaseDocumentConstraint, IRouteConstraint
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteName">Site name for matching of the requests</param>
        /// <param name="aliasPath">Alias path of the matching document</param>
        /// <param name="nodeId">Node ID of the matching document</param>
        /// <param name="culture">Culture of the matching document</param>
        /// <param name="pageInfoSource">Page info source</param>
        public CMSDocumentRouteConstraint(string siteName, string aliasPath, int nodeId, string culture, PageInfoSource pageInfoSource)
            : base(siteName, aliasPath, nodeId, culture, null, pageInfoSource)
        {
        }


        /// <summary>
        /// Returns true if the route matches the context.
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <param name="route">Route</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="values">Values of the route</param>
        /// <param name="routeDirection">Direction of the route</param>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            SiteNameOnDemand siteName = new SiteNameOnDemand();
            ViewModeOnDemand viewMode = new ViewModeOnDemand();


            string culture = null;
            string relativePath = null;

            ExcludedSystemEnum excludedEnum = RequestContext.CurrentExcludedStatus;

            // Do the preprocessing
            if (!PreProcess(siteName, viewMode, ref culture, ref relativePath, ref excludedEnum))
            {
                return false;
            }

            // Get the page info
            var pageInfo = GetPageInfo(route, culture, TreePathUtils.URL_PREFIX_ROUTE);

            if (pageInfo != null)
            {
                if (viewMode.Value == ViewModeEnum.LiveSite)
                {
                    // Set current page info source
                    URLRewritingContext.CurrentPageInfoSource = mPageInfoSource;
                }

                // Check if the page is published
                URLRewriter.CheckPublishedState(ref pageInfo, siteName, viewMode);
            }

            if (pageInfo != null)
            {
                // Redirect to main url if required
                if ((viewMode.IsLiveSite()) && !RequestHelper.IsPostBack() && !SearchCrawler.IsCrawlerRequest())
                {
                    PreferredCultureOnDemand cultureOnDemand = new PreferredCultureOnDemand();
                    cultureOnDemand.Value = culture;
                    URLRewriter.RedirectToMainUrl(siteName, pageInfo, mPageInfoSource, String.Empty, String.Empty, null, cultureOnDemand);
                }

                // Try AB Testing page
                pageInfo = URLRewriter.ProcessABTest(siteName.Value, viewMode.Value, pageInfo);

                DocumentContext.CurrentPageInfo = pageInfo;
                DocumentContext.CurrentAliasPath = pageInfo.NodeAliasPath;

                RequestContext.CurrentRelativePath = relativePath;

                return true;
            }

            return false;
        }

        #endregion
    }
}