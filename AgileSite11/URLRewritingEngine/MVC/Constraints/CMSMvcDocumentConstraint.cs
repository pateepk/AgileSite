using System;
using System.Web;
using System.Web.Routing;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.SiteProvider;

using RequestContext = CMS.Helpers.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Constraint to check the context of a document in the web site.
    /// </summary>
    public class CMSMvcDocumentConstraint : BaseDocumentConstraint, IRouteConstraint
    {
        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="siteName">Site name for matching of the requests</param>
        /// <param name="aliasPath">Alias path of the matching document</param>
        /// <param name="nodeId">Node ID of the matching document</param>
        /// <param name="culture">Culture of the matching document</param>
        /// <param name="urlPath">URL path with the pattern</param>
        /// <param name="pageInfoSource">Page info source</param>
        public CMSMvcDocumentConstraint(string siteName, string aliasPath, int nodeId, string culture, string urlPath, PageInfoSource pageInfoSource)
            : base(siteName, aliasPath, nodeId, culture, urlPath, pageInfoSource)
        {
        }

        #endregion


        #region "IRouteConstraint Members"

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

            // Get the page info and set the context
            PageInfo pageInfo = GetPageInfo(route, culture, TreePathUtils.URL_PREFIX_MVC);

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
                // Process the request values, controller and view
                var context = new System.Web.Routing.RequestContext(httpContext, new RouteData(route, null));

                if (!CMSMvcHelper.ProcessValues(context, route, values, mSiteName, mAliasPath, mUrlPath))
                {
                    NotMatched();
                    return false;
                }

                // Try AB Testing page
                pageInfo = URLRewriter.ProcessABTest(siteName.Value, viewMode.Value, pageInfo);
                mAliasPath = pageInfo.NodeAliasPath;

                // Set current status and request properties
                DocumentContext.CurrentAliasPath = mAliasPath;
                DocumentContext.CurrentPageInfo = pageInfo;

                RequestContext.CurrentStatus = RequestStatusEnum.MVCPage;

                RequestContext.LogPageHit = true;
                
                // Set the output caching
                OutputHelper.SetCaching(mSiteName, pageInfo, HttpContext.Current.Response);
                
                return true;
            }

            NotMatched();
            return false;
        }
        
        #endregion
    }
}