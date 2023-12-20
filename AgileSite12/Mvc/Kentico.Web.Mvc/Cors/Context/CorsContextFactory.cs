using CMS.Base;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Factory that provides <see cref="ICorsRequestContext"/> and <see cref="ICorsResponseContext"/>.
    /// </summary>
    internal class CorsContextFactory
    {
        /// <summary>
        /// Gets <see cref="ICorsRequestContext"/>.
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <returns>New instance of <see cref="CorsRequestContext"/>.</returns>
        public virtual ICorsRequestContext GetCorsRequestContext(IHttpContext httpContext)
        {
            return new CorsRequestContext(httpContext);
        }


        /// <summary>
        /// Gets <see cref="ICorsResponseContext"/>.
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <returns>New instance of <see cref="CorsResponseContext"/>.</returns>
        public virtual ICorsResponseContext GetCorsResponseContext(IHttpContext httpContext)
        {
            return new CorsResponseContext(httpContext);
        }
    }
}
