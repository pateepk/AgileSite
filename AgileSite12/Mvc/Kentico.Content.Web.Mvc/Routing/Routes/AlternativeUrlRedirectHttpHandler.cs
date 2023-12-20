using System.Web;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Redirects user to the url provided by <see cref="AlternativeUrlConstraint"/>.
    /// </summary>
    internal class AlternativeUrlRedirectHttpHandler : IHttpHandler
    {
        /// <summary>
        /// Redirects user to the url provided by <see cref="AlternativeUrlConstraint"/>.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            var redirectUrl = context.Items[AlternativeUrlsRouteConstants.REDIRECT_URL_CONTEXT_ITEM_NAME];

            context.Response.RedirectPermanent(redirectUrl.ToString());
        }

        /// <summary>
        /// This handler is reusable across different requests.
        /// </summary>
        public bool IsReusable => true;
    }
}
