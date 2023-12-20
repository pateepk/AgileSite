using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Adds CSP header with whitelisted current request url and administration url.
    /// </summary>
    /// <remarks>
    /// For preview, page and form builder links is following header added:
    /// Content-Security-Policy: frame-ancestors [administration url] [current request url]
    /// In case current request url or administration url don't belong to current site following header is added:
    /// Content-Security-Policy: frame-ancestors 'self'
    /// </remarks>
    internal static class VirtualContextLinksClickjackingProtection
    {
        private const string CONTENT_SECURITY_POLICY_HEADER_NAME = "Content-Security-Policy";
        private const string CONTENT_SECURITY_POLICY_HEADER_VALUE = "frame-ancestors {0}";
        private const string CONTENT_SECURITY_POLICY_HEADER_SELF_VALUE = "'self'";


        /// <summary>
        /// Initializes class.
        /// </summary>
        public static void Initialize()
        {
            if (ClickjackingProtectionEnabled)
            {
                RequestEvents.PreSendRequestHeaders.Execute += OnPreSendRequestHeaders;
            }
        }

        
        /// <summary>
        /// Indicates whether the Clickjacking protection is enabled (by default) or disabled via the application settings.
        /// </summary>
        private static bool ClickjackingProtectionEnabled => ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableClickjackingProtection"], true);


        /// <summary>
        /// Executes just before ASP.NET sends HTTP headers to the client.
        /// </summary>
        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var httpContext = Service.Resolve<IHttpContextAccessor>().HttpContext;

            SendCSPHeaders(httpContext);
        }


        /// <summary>
        /// Adds CSP header with whitelisted current request url and administration url.
        /// </summary>
        /// <param name="httpContext">HTTP context.</param>
        internal static void SendCSPHeaders(IHttpContext httpContext)
        {
            // Http context is null in special cases like SignalR (WebSockets) requests
            if (httpContext == null)
            {
                return;
            }

            var isEmbededInAdministration = ValidationHelper.GetBoolean(VirtualContext.GetItem(VirtualContext.PARAM_EMBEDED_IN_ADMINISTRATION), false);

            if (isEmbededInAdministration)
            {
                var administrationUrl = httpContext.Request?.QueryString[VirtualContext.ADMINISTRATION_DOMAIN_PARAMETER];
                var currentRequestUrl = httpContext.Request.Url.GetLeftPart(UriPartial.Authority) + SystemContext.ApplicationPath;

                var administrationUri = Uri.IsWellFormedUriString(administrationUrl, UriKind.Absolute) ? new Uri(administrationUrl) : null;
                var currentRequestUri = new Uri(currentRequestUrl);

                if (UriBelongsToCurrentSite(administrationUri) && UriBelongsToCurrentSite(currentRequestUri))
                {
                    httpContext.SetContentSecurityHeader(
                        administrationUri.GetLeftPart(UriPartial.Authority),
                        currentRequestUri.GetLeftPart(UriPartial.Authority));
                }
                else
                {
                    httpContext.SetContentSecurityHeader(CONTENT_SECURITY_POLICY_HEADER_SELF_VALUE);
                }
            }
        }


        private static bool UriBelongsToCurrentSite(Uri uri)
        {
            return (uri != null) && SiteContext.CurrentSiteName.Equals(
                SiteInfoProvider.GetSiteNameFromUrl(uri.GetLeftPart(UriPartial.Path), uri.AbsolutePath),
                StringComparison.OrdinalIgnoreCase);
        }


        private static void SetContentSecurityHeader(this IHttpContext httpContext, params string[] values)
        {
            httpContext.Response.Headers.Add(CONTENT_SECURITY_POLICY_HEADER_NAME,
                        String.Format(CONTENT_SECURITY_POLICY_HEADER_VALUE, String.Join(" ", values.Distinct())));
        }
    }
}
