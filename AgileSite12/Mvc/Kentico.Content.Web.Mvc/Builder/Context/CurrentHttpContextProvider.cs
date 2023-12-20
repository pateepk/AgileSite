using System.Web;

using CMS.Helpers;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Defines a method for retrieving current HTTP context.
    /// </summary>
    internal class CurrentHttpContextProvider : ICurrentHttpContextProvider
    {
        /// <summary>
        /// Gets an instance of current HTTP context.
        /// </summary>
        /// <returns>Returns an instance of HTTP context.</returns>
        public HttpContextBase Get()
        {
            return CMSHttpContext.Current;
        }
    }
}
