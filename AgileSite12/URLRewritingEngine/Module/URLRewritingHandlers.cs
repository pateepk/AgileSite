using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Membership;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.Protection;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Membership handlers
    /// </summary>
    internal class URLRewritingHandlers
    {
        /// <summary>
        /// UniGrid icons
        /// </summary>
        private const string UNIGRID_ICONS = "/ug/";

        /// <summary>
        /// Tree icons
        /// </summary>
        private const string TREE_ICONS = "/t/";

        /// <summary>
        /// Tree icons for RTL layout
        /// </summary>
        private const string TREE_ICONS_RTL = "/rt/";

        /// <summary>
        /// Small flag icons
        /// </summary>
        private const string FLAG_ICONS = "/f/";

        /// <summary>
        /// Flag icons for size 48x48
        /// </summary>
        private const string FLAG_ICONS_48 = "/f48/";


        /// <summary>
        /// Initializes the membership handlers
        /// </summary>
        public static void Init()
        {
            DocumentEvents.Delete.After += RefreshRoutesAfterDocumentDelete;
            DocumentEvents.Update.Before += RefreshRoutes;
            DocumentEvents.Insert.After += CreateRoute;
            AttachmentHandlerEvents.ProcessRequest.Execute += ValidateAttachmentCulture;

            var aliasEvents = DocumentAliasInfo.TYPEINFO.Events;

            aliasEvents.Insert.After += RefreshAliasRoutes;
            aliasEvents.Delete.After += RefreshAliasRoutes;
            aliasEvents.Update.Before += RefreshAliasRoutesIfDependentDataChanged;

            RequestEvents.Begin.Execute += BeginRequest;
            RequestEvents.Authorize.Execute += AuthorizeRequest;

            // Originally the RewriteUrl method was mapped to the MapRequestHandler
            // but we had to execute it sooner in a request lifecycle because
            // our HTTP handlers need a session in order to work properly.
            // See the CM-4373 for a detailed explanation.
            RequestEvents.PostResolveRequestCache.Execute += RewriteUrl;
            RequestEvents.PostMapRequestHandler.Execute += PostMapRequestHandler;
            RequestEvents.AcquireRequestState.Execute += CheckSecurity;
            RequestEvents.End.Execute += HandlePageNotFound;
        }


        private static void ValidateAttachmentCulture(object sender, EventArgs e)
        {
            // Validate the culture
            PreferredCultureOnDemand culture = new PreferredCultureOnDemand();
            SiteNameOnDemand siteName = new SiteNameOnDemand();
            ViewModeOnDemand viewMode = new ViewModeOnDemand();

            URLRewriter.ValidateCulture(siteName, viewMode, culture, null);
        }


        /// <summary>
        /// Handles the page not found status of the current request
        /// </summary>
        private static void HandlePageNotFound(object sender, EventArgs e)
        {
            if (CMSHttpContext.Current.Response.StatusCode == 404)
            {
                URLRewriter.HandlePageNotFound();
            }
        }


        /// <summary>
        /// Actions executes during the begin request event
        /// </summary>
        private static void BeginRequest(object sender, EventArgs e)
        {
            // Check if Database installation needed
            if (ShortImageRedirect())
            {
                return;
            }

            // Check if Database installation needed
            CMSApplication.InstallRedirect(false);
        }


        /// <summary>
        /// Executes actions needed on PostMapRequestHandler event
        /// </summary>
        private static void PostMapRequestHandler(object sender, EventArgs e)
        {
            PerformPlannedRedirect();

            ApplyRequestHandler();
        }


        /// <summary>
        /// Executes actions needed on AcquireRequestState event
        /// </summary>
        private static void CheckSecurity(object sender, EventArgs eventArgs)
        {
            // Check the page security
            RequestStatusEnum status = RequestContext.CurrentStatus;

            var siteName = new SiteNameOnDemand();
            var viewMode = new ViewModeOnDemand();

            var relativePath = RequestContext.CurrentRelativePath;

            CheckSecurity(status, siteName, viewMode, relativePath);
        }


        /// <summary>
        /// Applies the request handler from URL rewriting context if available
        /// </summary>
        private static void ApplyRequestHandler()
        {
            var handler = URLRewritingContext.HttpHandler;
            if (handler != null)
            {
                CMSHttpContext.Current.Handler = handler;
            }
        }


        /// <summary>
        /// Performs a redirect planned by URL rewriting
        /// </summary>
        private static void PerformPlannedRedirect()
        {
            // Try to redirect as planned first
            if (URLRewriter.FixRewriteRedirect)
            {
                URLRewriter.PerformPlannedRedirect();
            }
        }


        /// <summary>
        /// Checks the request security and path.
        /// </summary>
        /// <param name="status">URL rewriting status</param>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="relativePath">Relative path</param>
        private static void CheckSecurity(RequestStatusEnum status, SiteNameOnDemand siteName, ViewModeOnDemand viewMode, string relativePath)
        {
            // Process only for content pages
            CheckBannedIP(status);

            if (status == RequestStatusEnum.PathRewritten ||
                status == RequestStatusEnum.PageNotFound)
            {
                // Check page security
                if ((CMSHttpContext.Current.Session != null) && !OutputFilterContext.OutputFilterEndRequestRequired)
                {
                    var aliasPath = DocumentContext.CurrentAliasPath;
                    var pageInfo = DocumentContext.CurrentPageInfo;

                    if ((pageInfo != null) && (aliasPath != pageInfo.NodeAliasPath))
                    {
                        // Set alias path to root if current page info is root page info
                        if (pageInfo.NodeAliasPath == "/")
                        {
                            DocumentContext.CurrentAliasPath = "/";
                        }
                        // Display nothing if current alias path is not equal to page info alias path
                        else
                        {
                            DocumentContext.CurrentPageInfo = null;
                            pageInfo = DocumentContext.CurrentPageInfo;
                        }
                    }

                    PageSecurityHelper.CheckPageSecurity(pageInfo, siteName, viewMode, relativePath, !SearchCrawler.IsCrawlerRequest());
                }

                AuthenticationHelper.ExtendAuthenticationCookieExpiration();
            }
            else if ((CMSHttpContext.Current.Session != null) && !OutputFilterContext.OutputFilterEndRequestRequired)
            {
                PageSecurityHelper.HandlePreviewLink();
            }
        }


        /// <summary>
        /// Checks if IP is banned for the current request
        /// </summary>
        /// <param name="status">Rewriting status</param>
        private static void CheckBannedIP(RequestStatusEnum status)
        {
            switch (status)
            {
                case RequestStatusEnum.PathRewritten:
                case RequestStatusEnum.GetFileHandler:
                case RequestStatusEnum.GetProduct:
                case RequestStatusEnum.SystemPage:
                    // Check whether session is available
                    if (CMSHttpContext.Current.Session != null)
                    {
                        BannedIPInfoProvider.CheckBannedIP();
                    }
                    break;
            }
        }


        /// <summary>
        /// Performs the URL rewriting on the current request
        /// </summary>
        private static void RewriteUrl(object sender, EventArgs e)
        {
            var status = RequestContext.CurrentStatus;
            if (status != RequestStatusEnum.SentFromCache)
            {
                // Get request parameters
                string relativePath = RequestContext.CurrentRelativePath;

                var excludedEnum = GetCurrentExcludedEnum(relativePath);

                // Perform the URL rewriting
                URLRewriter.RewriteUrl(status, relativePath, excludedEnum);
            }
        }


        /// <summary>
        /// Get excluded enum for the current request
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        private static ExcludedSystemEnum GetCurrentExcludedEnum(string relativePath)
        {
            var excludedEnum = RequestContext.CurrentExcludedStatus;
            if (excludedEnum == ExcludedSystemEnum.Unknown)
            {
                excludedEnum = URLHelper.IsExcludedSystemEnum(relativePath);
            }

            return excludedEnum;
        }


        /// <summary>
        /// Attempts to send the output from cache
        /// </summary>
        private static void AuthorizeRequest(object sender, EventArgs e)
        {
            string relativePath = RequestContext.CurrentRelativePath;

            // Check the excluded status
            ExcludedSystemEnum excludedEnum = URLHelper.IsExcludedSystemEnum(relativePath);

            RequestContext.CurrentExcludedStatus = excludedEnum;

            // Try to send output from cache
            if (SendOutputFromCache(relativePath, excludedEnum))
            {
                return;
            }

            try
            {
                // Handle the virtual context
                if (VirtualContext.HandleVirtualContext(ref relativePath))
                {
                    // Rewrite the path to the new relative path without the prefix
                    URLRewriter.RewritePath("~" + relativePath, RequestContext.CurrentQueryString.TrimStart('?'));
                }
            }
            catch (InvalidVirtualContextException ex)
            {
                URLHelper.Redirect(AdministrationUrlHelper.GetAccessDeniedUrlWithMessage(ex.Message));
            }

            CMSDocumentRouteHelper.EnsureRoutes(SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Attempts to send the output from cache
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="excludedEnum">Excluded enum</param>
        private static bool SendOutputFromCache(string relativePath, ExcludedSystemEnum excludedEnum)
        {
            ViewModeOnDemand viewMode = new ViewModeOnDemand();
            SiteNameOnDemand siteName = new SiteNameOnDemand();

            // Try to send the output from the cache without URL rewriting
            if (URLRewriter.SendOutputFromCache(relativePath, excludedEnum, viewMode, siteName))
            {
                if (OutputFilterContext.OutputFilterEndRequestRequired)
                {
                    string newQuery = null;

                    // Ensure the raw URL as a part of the request
                    if (URLRewriter.FixRewriteRedirect)
                    {
                        newQuery = "rawUrl=" + HttpUtility.UrlEncode(CMSHttpContext.Current.Request.RawUrl);
                    }

                    URLRewriter.RewritePath("~/CMSPages/blank.aspx", newQuery, false);
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Refreshes alias routes if dependent data changed.
        /// </summary>
        private static void RefreshAliasRoutesIfDependentDataChanged(object sender, ObjectEventArgs e)
        {
            var ai = (DocumentAliasInfo)e.Object;

            // Check whether URL or culture changed
            var refreshRoutes = ai.AnyItemChanged("AliasURLPath", "AliasCulture", "AliasActionMode");
            if (refreshRoutes)
            {
                e.CallWhenFinished(() =>
                    {
                        if (ai != null)
                        {
                            CMSDocumentRouteHelper.RefreshAliasRoutes(ai.AliasNodeID, ai.Generalized.ObjectSiteName);
                        }
                    });
            }
        }


        /// <summary>
        /// Refreshes document alias routes
        /// </summary>
        private static void RefreshAliasRoutes(object sender, ObjectEventArgs e)
        {
            var ai = (DocumentAliasInfo)e.Object;

            if (ai != null)
            {
                CMSDocumentRouteHelper.RefreshAliasRoutes(ai.AliasNodeID, ai.Generalized.ObjectSiteName);
            }
        }


        /// <summary>
        /// Refreshes routes for deleted document and all aliases
        /// </summary>
        private static void RefreshRoutesAfterDocumentDelete(object sender, DocumentEventArgs e)
        {
            var node = e.Node;

            if (node != null)
            {
                string documentUrlPath = node.DocumentUrlPath;

                // Remove the potential routes
                node.DocumentUrlPath = "";

                // Document is defined by a route
                if (IsRoutePath(documentUrlPath))
                {
                    CMSDocumentRouteHelper.RefreshRoutes(node);
                }

                // Remove all related alias routes of the document
                CMSDocumentRouteHelper.RefreshAliasRoutes(node.NodeID, node.NodeSiteName);
            }
        }


        /// <summary>
        /// Refreshes the document routes
        /// </summary>
        private static void CreateRoute(object sender, DocumentEventArgs e)
        {
            e.CallWhenFinished(() =>
            {
                var node = e.Node;

                if (node != null)
                {
                    // Document is defined by a route
                    if (IsRoutePath(node.DocumentUrlPath))
                    {
                        // Register a new route
                        CMSDocumentRouteHelper.RefreshRoutes(node);
                    }
                }
            });
        }


        /// <summary>
        /// Refreshes the document routes
        /// </summary>
        private static void RefreshRoutes(object sender, DocumentEventArgs e)
        {
            if (e.Node != null)
            {
                String originalPath = ValidationHelper.GetString(e.Node.GetOriginalValue("DocumentUrlPath"), e.Node.DocumentUrlPath);
                e.CallWhenFinished(() =>
                {
                    // Compare changed document URL path with the old one
                    if ((IsRoutePath(originalPath) || IsRoutePath(e.Node.DocumentUrlPath))
                        && (originalPath != e.Node.DocumentUrlPath))
                    {
                        CMSDocumentRouteHelper.RefreshRoutes(e.Node);
                    }
                });
            }
        }


        /// <summary>
        /// Determines whether the given URL path specifies a route.
        /// </summary>
        /// <param name="urlPath">The URL path</param>
        private static bool IsRoutePath(string urlPath)
        {
            return urlPath?.StartsWith(TreePathUtils.URL_PREFIX_ROUTE, StringComparison.InvariantCulture) ?? false;
        }


        /// <summary>
        /// Redirects the file to the images folder.
        /// </summary>
        private static bool ShortImageRedirect()
        {
            string cmsimg = QueryHelper.GetString("cmsimg", null);
            if ((cmsimg != null) && cmsimg.StartsWithCSafe("/"))
            {
                if (cmsimg.StartsWithCSafe(UNIGRID_ICONS))
                {
                    // Unigrid actions
                    cmsimg = "Design/Controls/UniGrid/Actions" + cmsimg.Substring(3);
                }
                else if (cmsimg.StartsWithCSafe(TREE_ICONS))
                {
                    // Tree icons
                    cmsimg = "Design/Controls/Tree" + cmsimg.Substring(2);
                }
                else if (cmsimg.StartsWithCSafe(TREE_ICONS_RTL))
                {
                    // Tree icons RTL
                    cmsimg = "RTL/Design/Controls/Tree" + cmsimg.Substring(3);
                }
                else if (cmsimg.StartsWithCSafe(FLAG_ICONS))
                {
                    // Flag icons
                    cmsimg = "Flags/16x16" + cmsimg.Substring(2);
                }
                else if (cmsimg.StartsWithCSafe(FLAG_ICONS_48))
                {
                    // Large flag icons
                    cmsimg = "Flags/48x48" + cmsimg.Substring(4);
                }

                // Redirect to the correct location
                URLHelper.RedirectPermanent(AdministrationUrlHelper.GetImageUrl(cmsimg), SiteContext.CurrentSiteName);

                return true;
            }

            return false;
        }
    }
}
