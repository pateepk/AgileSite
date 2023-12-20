using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Page security methods
    /// </summary>
    public class PageSecurityHelper
    {
        #region "Variables"

        private const int REQUIRE_SSL_YES = 1;
        private const int REQUIRE_SSL_NEVER = 2;

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the location where permissions of the page should be checked.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static PageLocationEnum CheckPagePermissions(string siteName)
        {
            switch (SettingsKeyInfoProvider.GetValue(siteName + ".CMSCheckPagePermissions").ToLowerCSafe())
            {
                case "all":
                    return PageLocationEnum.All;

                case "no":
                case "none":
                    return PageLocationEnum.None;

                case "securedareas":
                    return PageLocationEnum.SecuredAreas;

                default:
                    return PageLocationEnum.SecuredAreas;
            }
        }


        /// <summary>
        /// Gets the URL of the page where the user should be redirected when not permitted to read the document.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string AccessDeniedPageURL(string siteName)
        {
            string url = SettingsKeyInfoProvider.GetValue(siteName + ".CMSAccessDeniedPageURL");
            if (url == "")
            {
                url = "~/CMSMessages/accessdeniedtopage.aspx";
            }

            return url;
        }


        /// <summary>
        /// Redirects the user to the access denied page.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void RedirectToAccessDenied(string siteName)
        {
            var url = AccessDeniedPageURL(siteName);
            url = URLHelper.AddParameterToUrl(url, "returnurl", HttpUtility.UrlEncode(RequestContext.CurrentURL));
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirect to logon page if actual page requires authentication.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="pi">PageInfo</param>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        /// <param name="viewMode">View mode</param>
        public static void CheckSecuredAreas(string siteName, PageInfo pi, bool excludeSystem, ViewModeEnum viewMode)
        {
            // Require context
            if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
            {
                return;
            }

            // Get current site
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }

            if (siteName == "")
            {
                return;
            }

            // Check authentication
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            if (!viewMode.IsLiveSite() || RequestContext.IsUserAuthenticated)
            {
                return;
            }

            if ((pi == null) || !pi.RequiresAuthentication)
            {
                return;
            }

            string logonPage = AuthenticationHelper.GetSecuredAreasLogonPage(siteName);
            
            // Check if logon page from settings is system page
            if (!URLHelper.IsExcludedSystem(logonPage.TrimStart('~')))
            {
                logonPage = URLHelper.GetAbsoluteUrl(logonPage);

                // Check if logon page lays on current site
                var logonSiteName = SiteInfoProvider.GetSiteNameFromUrl(logonPage);
                if (logonSiteName.EqualsCSafe(siteName))
                {
                    // Do not redirect if current page is logon page
                    var logonPi = PageInfoProvider.GetPageInfoForUrl(logonPage, LocalizationContext.PreferredCultureCode, null, SiteInfoProvider.CombineWithDefaultCulture(siteName), true, siteName);
                    if ((logonPi != null) && (logonPi.DocumentID == pi.DocumentID))
                    {
                        return;
                    }
                }
            }

            CheckSecuredAreasInternal(siteName, excludeSystem);
        }


        /// <summary>
        /// Redirect to logon page if the user is not authenticated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        /// <param name="viewMode">View mode</param>
        public static void CheckSecuredAreas(string siteName, bool excludeSystem, ViewModeEnum viewMode)
        {
            // Require context
            if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
            {
                return;
            }

            // Get current site
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }
            if (siteName == "")
            {
                return;
            }

            // Check authentication
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            if (!viewMode.IsLiveSite() || RequestContext.IsUserAuthenticated)
            {
                return;
            }

            CheckSecuredAreasInternal(siteName, excludeSystem);
        }


        /// <summary>
        /// Redirects to the logon page if user is not authenticated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        public static void CheckSecured(string siteName, ViewModeEnum viewMode)
        {
            // Require context
            if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
            {
                return;
            }

            // Get current site
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }
            if (siteName == "")
            {
                return;
            }

            // Check authentication
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            if (!viewMode.IsLiveSite() || RequestContext.IsUserAuthenticated)
            {
                return;
            }

            CheckSecuredInternal(siteName);
        }


        /// <summary>
        /// Redirect to logon page if actual page requires authentication.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="pi">PageInfo</param>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        /// <param name="relativePath">Relative path</param>
        public static void CheckPermissions(string siteName, PageInfo pi, bool excludeSystem, string relativePath)
        {
            if (CMSHttpContext.Current == null)
            {
                return;
            }

            if (excludeSystem && URLHelper.IsExcludedSystem(relativePath))
            {
                return;
            }

            if (pi == null)
            {
                return;
            }

            // Get the site name
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }

            bool checkPermissions = false;
            switch (CheckPagePermissions(siteName))
            {
                case PageLocationEnum.All:
                    checkPermissions = true;
                    break;

                case PageLocationEnum.SecuredAreas:
                    checkPermissions = pi.RequiresAuthentication;
                    break;
            }

            if (!checkPermissions)
            {
                return;
            }

            // Check the read permission for the page
            var document = DocumentContext.CurrentDocument;
            if ((document == null) || (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(document, NodePermissionsEnum.Read) == AuthorizationResultEnum.Denied))
            {
                RedirectToAccessDenied(siteName);
            }
        }


        /// <summary>
        /// Redirect to https:// if page require it
        /// </summary>
        /// <param name="pi">PageInfo</param>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        public static void RequestSecurePage(PageInfo pi, bool excludeSystem, ViewModeEnum viewMode, string siteName)
        {
            if (pi == null)
            {
                return;
            }

            // Get View mode
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            if (pi.RequiresSSLValue == REQUIRE_SSL_YES)
            {
                RequestSecurePage(excludeSystem, REQUIRE_SSL_YES, viewMode, siteName);
            }

            if (pi.RequiresSSLValue == REQUIRE_SSL_NEVER)
            {
                RequestSecurePage(excludeSystem, REQUIRE_SSL_NEVER, viewMode, siteName);
            }
        }


        /// <summary>
        /// Redirect to  https:// or to http:// if page require it
        /// </summary>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        /// <param name="requireSSLValue">Require SSL value</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="siteName">Site name</param>
        public static void RequestSecurePage(bool excludeSystem, int requireSSLValue, ViewModeEnum viewMode, string siteName)
        {
            if (CMSHttpContext.Current == null)
            {
                return;
            }

            // Check view mode
            if (viewMode == ViewModeEnum.Unknown)
            {
                viewMode = PortalContext.ViewMode;
            }

            if (viewMode != ViewModeEnum.LiveSite)
            {
                return;
            }

            // Get relative path
            string relativePath = RequestContext.CurrentRelativePath;
            if (excludeSystem && URLHelper.IsExcludedSystem(relativePath))
            {
                return;
            }

            string requestPath = RequestContext.RawURL;

            // Create absolute path
            if (!requestPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string mDomain = RequestContext.URL.AbsoluteUri;
                mDomain = mDomain.Remove(RequestContext.URL.AbsoluteUri.IndexOf('/', RequestContext.URL.AbsoluteUri.IndexOf("://", StringComparison.Ordinal) + 3));
                requestPath = mDomain.ToLowerInvariant() + requestPath;
            }

            if ((requireSSLValue == REQUIRE_SSL_YES) && (!RequestContext.IsSSL))
            {
                requestPath = URLHelper.RemovePortFromURL(requestPath);
                // Get secured URL
                requestPath = "https://" + URLHelper.RemoveProtocol(requestPath);
                // Redirect to the secure page
                URLHelper.RedirectPermanent(requestPath, siteName);
            }
            else if ((requireSSLValue == REQUIRE_SSL_NEVER) && RequestContext.IsSSL)
            {
                requestPath = URLHelper.RemovePortFromURL(requestPath);
                // Get non-secured URL
                requestPath = "http://" + URLHelper.RemoveProtocol(requestPath);

                // Redirect to the non-secure page
                URLHelper.RedirectPermanent(requestPath, siteName);
            }
        }

        /// <summary>
        /// Handles the preview link for document engine
        /// </summary>
        public static void HandlePreviewLink()
        {
            // Check preview link context
            if (!VirtualContext.IsPreviewLinkInitialized)
            {
                return;
            }

            // Validate page info
            var viewMode = PortalContext.ViewMode;
            var previewGuid = ValidationHelper.GetGuid(VirtualContext.GetItem(VirtualContext.PARAM_WF_GUID), Guid.Empty);
            var tree = new TreeProvider();
            var cycleGuid = tree.SelectNodes()
                                .All()
                                .Column("DocumentWorkflowCycleGUID")
                                .WhereEquals("DocumentWorkflowCycleGUID", previewGuid)
                                .TopN(1)
                                .GetScalarResult(Guid.Empty);

            if (cycleGuid != Guid.Empty)
            {
                CheckPreviewLink(cycleGuid, viewMode, false);
            }
            else
            {
                // Reset the virtual context
                VirtualContext.Reset();

                // GUID values don't match
                URLHelper.Redirect(AdministrationUrlHelper.GetAccessDeniedUrl("virtualcontext.previewlink"));
            }
        }

        /// <summary>
        /// Check preview link context
        /// </summary>
        /// <param name="cycleGuid">GUID which identifies the workflow cycle</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="documentUrl">Indicates if document URL should be checked</param>
        public static void CheckPreviewLink(Guid cycleGuid, ViewModeEnum viewMode, bool documentUrl)
        {
            if (!VirtualContext.IsPreviewLinkInitialized)
            {
                return;
            }

            var previewGuid = ValidationHelper.GetGuid(VirtualContext.GetItem(VirtualContext.PARAM_WF_GUID), Guid.Empty);
            if (previewGuid == Guid.Empty)
            {
                return;
            }

            // Force preview mode
            if (!viewMode.IsOneOf(ViewModeEnum.Preview, ViewModeEnum.LiveSite))
            {
                return;
            }

            // Preview link is valid
            if (cycleGuid == previewGuid)
            {
                if (documentUrl)
                {
                    return;
                }

                // Additional check for links within the document
                if (VirtualContext.ValidatePathHash(RequestContext.CurrentRelativePath))
                {
                    return;
                }
            }

            // Reset the virtual context
            VirtualContext.Reset();

            // GUID values don't match
            URLHelper.Redirect(AdministrationUrlHelper.GetAccessDeniedUrl("virtualcontext.accessdenied"));
        }


        /// <summary>
        /// Checks whether user is authorized to content with non-live site view mode.
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="viewMode">View mode</param>
        public static void CheckViewModePermissions(PageInfo pi, ViewModeEnum viewMode)
        {
            if (pi == null)
            {
                return;
            }

            // Check whether user is allowed to see content in non-live site view mode
            if (viewMode == ViewModeEnum.LiveSite)
            {
                return;
            }

            // Check whether user has read permission to the current node
            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerTreeNode(pi.NodeID, NodePermissionsEnum.Read) == AuthorizationResultEnum.Allowed)
            {
                return;
            }

            var url = AdministrationUrlHelper.GetAccessDeniedUrl(null, null, null, String.Format(ResHelper.GetString("cmsdesk.notauthorizedtoreaddocument"), pi.NodeAliasPath), AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE);
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Checks the security for current page
        /// </summary>
        /// <param name="pageInfo">Page info to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode</param>
        /// <param name="relativePath">Relative path</param>
        /// <param name="checkSecuredAreas">Indicates if secured areas should be checked. It may leads to redirect to login page</param>
        public static void CheckPageSecurity(PageInfo pageInfo, SiteNameOnDemand siteName, ViewModeOnDemand viewMode, string relativePath, bool checkSecuredAreas = true)
        {
            // Check view mode permissions
            CheckViewModePermissions(pageInfo, viewMode);

            // Check path
            string aliasPath = DocumentContext.CurrentAliasPath;

            if (pageInfo != null)
            {
                // Check preview link context
                CheckPreviewLink(pageInfo.DocumentWorkflowCycleGUID, viewMode, true);

                // Check SSL Require
                RequestSecurePage(pageInfo, true, viewMode, siteName);

                if (checkSecuredAreas)
                {
                    // Check secured areas
                    CheckSecuredAreas(siteName, pageInfo, true, viewMode);
                }

                // Check permissions
                CheckPermissions(siteName, pageInfo, true, relativePath);
            }

            // Check default alias path
            if ((aliasPath != "/") || (!viewMode.IsLiveSite()))
            {
                return;
            }

            string defaultAliasPath = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaultAliasPath");
            string lowerDefaultAliasPath = defaultAliasPath.ToLowerCSafe();
            if ((defaultAliasPath == "") || (lowerDefaultAliasPath == aliasPath.ToLowerCSafe()))
            {
                return;
            }

            if (lowerDefaultAliasPath == "/default")
            {
                // Special case - default
                DocumentContext.CurrentAliasPath = defaultAliasPath;
            }
            else
            {
                // Redirect to the new path
                URLHelper.Redirect(DocumentURLProvider.GetUrl(defaultAliasPath));
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Redirect to logon page if the user is not authenticated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="excludeSystem">If true, the system pages are excluded from evaluation</param>
        private static void CheckSecuredAreasInternal(string siteName, bool excludeSystem)
        {
            // Get relative path
            string relativePath = RequestContext.CurrentRelativePath;
            if (!excludeSystem || !URLHelper.IsExcludedSystem(relativePath))
            {
                CheckSecuredInternal(siteName);
            }
        }


        /// <summary>
        /// Redirects to the logon page if user is not authenticated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void CheckSecuredInternal(string siteName)
        {
            // Get the logon page URL
            string logonPage = AuthenticationHelper.GetSecuredAreasLogonPage(siteName);

            // Get the URL
            string currentUrl = RequestContext.CurrentURL;

            // Redirect to logon page
            logonPage = URLHelper.AddParameterToUrl(logonPage, "returnurl", HttpUtility.UrlEncode(currentUrl));
            URLHelper.Redirect(logonPage);
        }

        #endregion
    }
}
