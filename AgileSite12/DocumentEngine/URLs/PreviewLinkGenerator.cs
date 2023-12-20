using System;

using CMS.Helpers;
using CMS.DocumentEngine.Internal;
using System.Collections.Specialized;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Generates preview link for a page.
    /// </summary>
    internal sealed class PreviewLinkGenerator
    {
        private readonly TreeNode page;


        /// <summary>
        /// Creates an instance of <see cref="PreviewLinkGenerator"/> class.
        /// </summary>
        /// <param name="page">Page for which the preview link should be generated.</param>
        public PreviewLinkGenerator(TreeNode page)
        {
            this.page = page ?? throw new ArgumentNullException(nameof(page));
        }


        /// <summary>
        /// Generates preview link for specified page and user.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="ensureQueryHash">Indicates if query string hash should be added. For content-only nodes, hash is added always.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        /// <param name="embededInAdministration">Indicates if page is embedded in administration inside an iframe.</param>
        /// <param name="queryString">Optional additional query string parameters.</param>
        public string Generate(string userName, bool ensureQueryHash = false, bool readonlyMode = true, bool embededInAdministration = false, NameValueCollection queryString = null)
        {
            if (page.DocumentWorkflowCycleGUID == Guid.Empty)
            {
                return null;
            }

            var presentationUrl = page.Site.SitePresentationURL;
            var presentationUrlAvailable = !string.IsNullOrEmpty(presentationUrl);

            if (page.Site.SiteIsContentOnly && !presentationUrlAvailable)
            {
                throw new InvalidOperationException($"The Presentation URL is not specified for '{page.Site.DisplayName}' site");
            }

            string url;
            if (page.NodeIsContentOnly)
            {
                var urlPattern = page.DataClassInfo.ClassURLPattern;
                if (string.IsNullOrEmpty(urlPattern))
                {
                    // Page has no preview link without URL pattern
                    return null;
                }

                // Get standard URL (content only page does not have permanent URL)
                url = DocumentURLProvider.GetUrl(page);

                if (!page.Site.SiteIsContentOnly)
                {
                    // If not MVC site do not create virtual context and handle URL as portal engine page
                    return url;
                }

                ensureQueryHash = true;
            }
            else
            {
                url = DocumentURLProvider.GetPermanentDocUrl(page.NodeGUID, VirtualContext.PARAM_PREVIEW_LINK, page.NodeSiteName, PageInfoProvider.PREFIX_CMS_GETDOC, ".aspx");
            }

            url = DecorateUrlWithQueryStringParameters(url, queryString);

            if (ensureQueryHash)
            {
                url = URLHelper.RemoveApplicationPath(url.TrimStart('~')).ToLowerInvariant();
                url = "~/" + VirtualContext.AddPathHash(url).TrimStart('/');
            }

            var param = VirtualContext.GetPreviewParameters(userName, page.DocumentCulture, page.DocumentWorkflowCycleGUID, readonlyMode, embededInAdministration);
            url = VirtualContext.GetVirtualContextPath(url, param);

            // Append site presentation URL if present
            return presentationUrlAvailable ? URLHelper.CombinePath(url, '/', presentationUrl, null) : url;
        }


        /// <summary>
        /// Decorates <paramref name="url"/> with query strings.
        /// </summary>
        private string DecorateUrlWithQueryStringParameters(string url, NameValueCollection queryString)
        {
            if (queryString != null)
            {
                foreach (string key in queryString)
                {
                    url = URLHelper.AddParameterToUrl(url, key, queryString.Get(key));
                }
            }

            var eventArgs = new GeneratePreviewLinkEventArgs(page);
            DocumentEventsInternal.GeneratePreviewLink.StartEvent(eventArgs);
            foreach (var kvp in eventArgs.QueryStringParameters)
            {
                url = URLHelper.AddParameterToUrl(url, kvp.Key, kvp.Value);
            }

            return url;
        }
    }
}
