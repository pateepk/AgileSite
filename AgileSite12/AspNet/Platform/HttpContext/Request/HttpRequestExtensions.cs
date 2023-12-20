using System;
using System.Security.Policy;
using System.Web;

using CMS.Helpers;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Extension methods for request objects.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Returns effective URL with correct port number based on <see cref="RequestContext.IsSSL"/> property for <see cref="Url"/> property.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestBase"/> object.</param>
        /// <remarks>
        /// When SSL offloading proxy is used <see cref="HttpRequestBase.Url"/> does not contain valid port and scheme.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="request"/></exception>
        public static Uri GetEffectiveUrl(this HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

#pragma warning disable BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.
            return Helpers.Extensions.HttpRequestExtensions.GetEffectiveUrl(request.Url, request.IsSecureConnection);
#pragma warning restore BH1006 // 'Request.Url' should not be used. Use 'RequestContext.URL' instead.
        }


        /// <summary>
        /// Returns effective URL with correct port number based on <see cref="RequestContext.IsSSL"/> property for <see cref="HttpRequest.Url"/> property.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> object.</param>
        /// <remarks>
        /// When SSL offloading proxy is used <see cref="HttpRequest.Url"/> does not contain valid port and scheme.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="request"/></exception>
        public static Uri GetEffectiveUrl(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var wrapper = new HttpRequestWrapper(request);
            return GetEffectiveUrl(wrapper);
        }
    }
}
