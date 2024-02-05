using System;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Routing.Web;

[assembly: RegisterHttpHandler("CMSPages/GetDocLink.ashx", typeof(GetDocLinkHandler), Order = 1)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// <para>
    /// Handles requests for documentation. Redirects the client to documentation service with proper product version specified.
    /// </para>
    /// <para>
    /// Accessing the handler without "link" query parameter specified redirects the client to product documentation root.
    /// Accessing the handler with "link" query parameter redirects the client to documentation topic identified by "link".
    /// </para>
    /// </summary>
    /// <remarks>
    /// The handler is designed as reusable and therefore must be thread-safe.
    /// Keep that in mind when making any modifications.
    /// </remarks>
    internal class GetDocLinkHandler : IHttpHandler
    {

        /// <summary>
        /// Gets the permanent link from URL query string.
        /// Permanent link identifies the documentation topic.
        /// </summary>
        private string PermanentLink
        {
            get
            {
                return QueryHelper.GetString("link", String.Empty);
            }
        }


        /// <summary>
        /// Processes the documentation request.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            string link = PermanentLink;

            // Do not allow redirection to absolute URLs via this handler. When someone needs an absolute URL, this handler is redundant.
            // Absolute URLs pose a risk of unvalidated redirect.
            if (IsAbsoluteUrl(link))
            {
                EventLogProvider.LogEvent(EventType.WARNING, "GetDocLinkHandler", "ProcessRequest", "Documentation link handler does not support redirection to absolute URLs. This warning is either result of a mistake in a documentation link, or a malicious input is trying to perform a deceptive redirect. If redirection to absolute URL is desired, the URL can be used directly instead of link to this handler (the handler performs additional actions only for links using tiny IDs).");

                // Force redirect to the documentation root. The absolute URL might either be a mistake or a malicious input
                link = null;
            }
            string url = (String.IsNullOrEmpty(link)) ? DocumentationHelper.GetDocumentationRootUrl() : DocumentationHelper.GetDocumentationTopicUrl(link);

            URLHelper.Redirect(url);
        }


        /// <summary>
        /// The handler is reusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Returns true if <paramref name="url"/> is an absolute URL.
        /// </summary>
        /// <param name="url">URL to be validated.</param>
        /// <returns>True if URL is absolute, false otherwise.</returns>
        private bool IsAbsoluteUrl(string url)
        {
            Uri res;
            return Uri.TryCreate(url, UriKind.Absolute, out res);
        }
    }
}
