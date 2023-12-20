using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform.HttpContext.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IHttpCookie"/> interface.
    /// </summary>
    internal static class IHttpCookieExtensions
    {
        /// <summary>
        /// Returns instance of <see cref="HttpCookie"/> with same values as <see cref="IHttpCookie"/>.
        /// </summary>
        public static HttpCookie ToHttpCookie(this IHttpCookie cookie)
        {
            if (cookie is HttpCookieImpl impl)
            {
                return impl.Cookie;
            }

            return new HttpCookie(cookie.Name)
            {
                Value = cookie.Value,
                Expires = cookie.Expires,
                HttpOnly = cookie.HttpOnly,
                Domain = cookie.Domain,
                Path = cookie.Path
            };
        }
    }
}
