using System;
using System.Web;
using System.Web.Routing;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;

using RequestContext = CMS.Helpers.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Base class for the document constraints
    /// </summary>
    public abstract class BaseDocumentConstraint
    {
        #region "Variables"

        /// <summary>
        /// Site name for the current handler.
        /// </summary>
        protected string mSiteName = null;

        /// <summary>
        /// Alias path for the current handler
        /// </summary>
        protected string mAliasPath = null;

        /// <summary>
        /// Node ID for the current handler
        /// </summary>
        protected int mNodeId = 0;

        /// <summary>
        /// Culture for the current handler
        /// </summary>
        protected string mCulture = null;

        /// <summary>
        /// Url path
        /// </summary>
        protected string mUrlPath = null;

        /// <summary>
        /// Page info source
        /// </summary>
        protected PageInfoSource mPageInfoSource = PageInfoSource.UrlPath;

        /// <summary>
        /// If true (default), the routing behaves the same way as URL path = switches the current culture to the route culture
        /// </summary>
        private static bool? mSwitchToRouteCulture = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true (default), the routing behaves the same way as URL path = switches the current culture to the route culture
        /// </summary>
        public bool SwitchToRouteCulture
        {
            get
            {
                if (mSwitchToRouteCulture == null)
                {
                    mSwitchToRouteCulture = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSwitchToRouteCulture"], true);
                }

                return mSwitchToRouteCulture.Value;
            }
            set
            {
                mSwitchToRouteCulture = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteName">Site name for matching of the requests</param>
        /// <param name="aliasPath">Alias path of the matching document</param>
        /// <param name="nodeId">Node ID of the matching document</param>
        /// <param name="culture">Culture of the matching document</param>
        /// <param name="urlPath">URL path with the pattern</param>
        /// <param name="pageInfoSource">Page info source</param>
        public BaseDocumentConstraint(string siteName, string aliasPath, int nodeId, string culture, string urlPath, PageInfoSource pageInfoSource)
        {
            mSiteName = siteName;
            mAliasPath = aliasPath;
            mNodeId = nodeId;
            mCulture = culture;
            mUrlPath = urlPath;
            mPageInfoSource = pageInfoSource;
        }


        /// <summary>
        /// Handles the processed operations when the constraint is not matched
        /// </summary>
        protected static void NotMatched()
        {
            //VirtualContext.Reset();
        }


        /// <summary>
        /// Preprocesses the constraint, matches the site name and culture, and attempts to send output from cache
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="culture">Returns the culture to be used</param>
        /// <param name="excludedEnum">Returns the exclusion status</param>
        /// <param name="relativePath">Returns the relative path of the request</param>
        protected bool PreProcess(SiteNameOnDemand siteName, ViewModeOnDemand viewMode, ref string culture, ref string relativePath, ref ExcludedSystemEnum excludedEnum)
        {
            // If the output was already sent from cache, automatically skip
            RequestStatusEnum status = RequestContext.CurrentStatus;
            if (status == RequestStatusEnum.SentFromCache)
            {
                return false;
            }

            // Check site
            if (!CheckSite(siteName))
            {
                return false;
            }

            // Check culture
            if (!CheckCulture(ref culture))
            {
                return false;
            }

            relativePath = RequestContext.CurrentRelativePath;

            // Check the excluded status
            if (excludedEnum == ExcludedSystemEnum.Unknown)
            {
                excludedEnum = URLHelper.IsExcludedSystemEnum(relativePath);
            }

            if (excludedEnum != ExcludedSystemEnum.NotExcluded)
            {
                NotMatched();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Validates the culture of the request against the constraint
        /// </summary>
        /// <param name="culture">Outputs the culture to be used</param>
        protected bool CheckCulture(ref string culture)
        {
            // Ensure the culture
            if (String.IsNullOrEmpty(culture))
            {
                culture = LocalizationContext.PreferredCultureCode;
            }

            // Check culture
            if (!String.IsNullOrEmpty(mCulture))
            {
                // Preferred culture must match the route culture
                if (!culture.Equals(mCulture, StringComparison.InvariantCulture))
                {
                    if (SwitchToRouteCulture)
                    {
                        // Switch to the route culture
                        LocalizationContext.PreferredCultureCode = mCulture;
                        culture = mCulture;

                        return true;
                    }
                    else
                    {
                        // Do not match the culture
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Validates current site against the constraint
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected bool CheckSite(SiteNameOnDemand siteName)
        {
            // Check site
            if (!mSiteName.Equals(siteName.Value, StringComparison.InvariantCulture))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets the page info with the correct PageResult object for the given url.
        /// </summary>
        /// <param name="route">The current route</param>
        /// <param name="culture">The culture</param>
        /// <param name="pathPrefix">The path type prefix ("MVC:", "ROUTE:" prefix).</param>
        protected PageInfo GetPageInfo(Route route, string culture, string pathPrefix)
        {
            bool combineWithDefaultCulture = SiteInfoProvider.CombineWithDefaultCulture(mSiteName);

            // Get the page info
            PageInfo pageInfo = PageInfoProvider.GetPageInfo(mSiteName, mAliasPath, culture, null, mNodeId, combineWithDefaultCulture);

            // Setup the PageResult object only if it has not been setup yet
            if ((mPageInfoSource == PageInfoSource.DocumentAlias)
                && string.IsNullOrEmpty(pageInfo.PageResult.DocumentAliasUrlPath)
                && (route.DataTokens != null)
                && route.DataTokens.ContainsKey(CMSDocumentRouteHelper.ALIAS_PAGE_INFO_RESULT))
            {
                PageInfoResult pageResult = (PageInfoResult)route.DataTokens[CMSDocumentRouteHelper.ALIAS_PAGE_INFO_RESULT];
                if ((pageResult != null) && (pageInfo != null))
                {
                    pageResult.PageSource = mPageInfoSource;

                    // Store the page result data in the current page info object
                    pageInfo.SetPageResult(pageResult.DocumentAliasUrlPath, pageResult);
                }
            }

            return pageInfo;
        }

        #endregion
    }
}