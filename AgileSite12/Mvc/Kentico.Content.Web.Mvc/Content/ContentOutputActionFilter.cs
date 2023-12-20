using System;
using System.Web;
using System.Web.Mvc;

using CMS.Base;
using CMS.Helpers;

using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Sets the response filter which applies content-related modifications.
    /// </summary>
    internal class ContentOutputActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var context = filterContext.HttpContext;
            
            if (ResponseFilter.IsAvailable && ResponseFilter.IsHtmlResponse)
            {
                ResponseFilter.Instance.Add("ContentResponseFilter", (string html) =>
                {
                    // IMPORTANT: This is memory critical section. Minimize copying and mutations on 'html' as it may be very large at size (represents a whole page).
                    ResolveUrls(ref html, context.Request.ApplicationPath);
                    EnsureVirtualContextPrefixes(ref html, context);

                    return html;
                });
            }
        }


        /// <summary>
        /// Calculates hash for the given <paramref name="path"/> and appends it to the URL with administration domain.
        /// </summary>
        private static string AddHashWithAdministrationDomainToPath(string path)
        {
            var pathWithHash = VirtualContext.AddPathHash(path);

            return VirtualContext.AddAdministrationDomain(pathWithHash);
        }


        /// <summary>
        /// Resolves relative URLs (prefixed with "~") contained in <paramref name="html"/> using a given <paramref name="applicationPath"/>.
        /// </summary>
        /// <param name="html">HTML code.</param>
        /// <param name="applicationPath">Current application path.</param>
        private static void ResolveUrls(ref string html, string applicationPath)
        {
            bool applyFilter = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSMVCResolveRelativeUrls"], true);

            if (applyFilter && html.IndexOf("~/", StringComparison.Ordinal) >= 0)
            {
                html = HTMLHelper.ResolveUrls(html, applicationPath, true);
            }
        }


        /// <summary>
        /// Appends virtual context prefixes to URLs contained in <paramref name="html"/> if a preview mode is on.
        /// </summary>
        /// <param name="html">HTML code.</param>
        /// <param name="context">HTTP context.</param>
        private static void EnsureVirtualContextPrefixes(ref string html, HttpContextBase context)
        {
            if (VirtualContext.IsPreviewLinkInitialized)
            {
                var preprocessedHtml = HTMLHelper.EnsureURLPrefixes(html, context.Request.ApplicationPath, VirtualContext.CurrentURLPrefix, AddHashWithAdministrationDomainToPath);

                html = FormActionModifier.EnsurePreviewActionUrl(preprocessedHtml);
            }
        }
    }
}