using System;

using CMS.Base;

namespace CMS.Helpers.Extensions
{
    /// <summary>
    /// Extension methods for request objects.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Returns effective URL with correct port number based on <see cref="RequestContext.IsSSL"/> property for <see cref="IRequest.Url"/> property.
        /// </summary>
        /// <param name="request"><see cref="IRequest"/> object.</param>
        /// <remarks>
        /// When SSL offloading proxy is used <see cref="IRequest.Url"/> does not contain valid port and scheme.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="request"/></exception>
        public static Uri GetEffectiveUrl(this IRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return GetEffectiveUrl(request.Url, request.IsSecureConnection);
        }


        /// <summary>
        /// Returns effective URL with correct port number based on <see cref="RequestContext.IsSSL"/> property for <see cref="IRequest.Url"/> property.
        /// </summary>
        /// <param name="url">Url of current request.</param>
        /// <param name="isSecureConnection">Indication whether HTTP connection uses secure sockets.</param>
        /// <remarks>
        ///This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public static Uri GetEffectiveUrl(Uri url, bool isSecureConnection)
        {
            if (url != null)
            {
                var isSSL = RequestContext.IsSSLExplicit.GetValueOrDefault(false) || isSecureConnection || url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

                // Ensure correct uri scheme when the IsSSL property was manually set
                if (isSSL && (url.Scheme == "http"))
                {
                    UriBuilder uriBuilder = new UriBuilder(url);
                    uriBuilder.Scheme = "https";
                    url = uriBuilder.Uri;
                }

                url = URLHelper.EnsureProperPort(isSSL, url);
            }

            return url;
        }
    }
}
