using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Decorates the "location" response HTTP header with the virtual context information.
    /// </summary>
    internal class PreviewResponseRedirectionModule
    {
        private const int HTTP_STATUS_CODE_REDIRECT = 302;
        private const int HTTP_STATUS_CODE_PERMANENT_REDIRECT = 301;
        private const string LOCATION_KEY = "location";

        private bool initialized = false;

        private static Lazy<PreviewResponseRedirectionModule> instance = new Lazy<PreviewResponseRedirectionModule>(() => new PreviewResponseRedirectionModule());


        /// <summary>
        /// Returns a singleton instance of the <see cref="PreviewResponseRedirectionModule"/>.
        /// </summary>
        public static PreviewResponseRedirectionModule Instance => instance.Value;


        /// <summary>
        /// Initializes the module and prepares it to handle requests.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times. It is ensured that request events will be bound only once.
        /// </remarks>
        public static void Initialize()
        {
            Instance.InitializeOnce();
        }


        private void InitializeOnce()
        {
            if (!initialized)
            {
                RequestEvents.PreSendRequestHeaders.Execute += PreSendRequestHeaders_Execute;

                initialized = true;
            }
        }


        private static void PreSendRequestHeaders_Execute(object sender, EventArgs e)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
            var previewPathDecorator = new PreviewPathDecorator(readonlyMode: VirtualContext.ReadonlyMode);

            DecorateLocationHttpHeader(httpContextWrapper, previewPathDecorator);
        }


        /// <summary>
        /// Decorates the location HTTP header with Virtual context data.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="previewPathDecorator">The preview path decorator.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="httpContext" /> or <paramref name="previewPathDecorator" /> is <c>null</c>.</exception>
        internal static void DecorateLocationHttpHeader(HttpContextBase httpContext, IPathDecorator previewPathDecorator)
        {
            httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            previewPathDecorator = previewPathDecorator ?? throw new ArgumentNullException(nameof(previewPathDecorator));

            string location = httpContext.Response.Headers[LOCATION_KEY];

            if (!String.IsNullOrEmpty(location)
                && !IsAbsoluteUrl(location)
                && !VirtualContext.ContainsVirtualContextPrefix(location)
                && IsHttpRedirect(httpContext)
                && httpContext.Kentico().Preview().Enabled)
            {
                httpContext.Response.Headers[LOCATION_KEY] = previewPathDecorator.Decorate(location);
            }
        }


        /// <summary>
        /// Indicates whether the current response leads to a client redirection.
        /// </summary>
        private static bool IsHttpRedirect(HttpContextBase httpContext)
        {
            int responseStatusCode = httpContext.Response.StatusCode;

            return (responseStatusCode == HTTP_STATUS_CODE_REDIRECT) || (responseStatusCode == HTTP_STATUS_CODE_PERMANENT_REDIRECT);
        }


        /// <summary>
        /// Indicates whether the given <paramref name="url"/> is an absolute URL.
        /// </summary>
        private static bool IsAbsoluteUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("//", StringComparison.Ordinal);
        }
    }
}
