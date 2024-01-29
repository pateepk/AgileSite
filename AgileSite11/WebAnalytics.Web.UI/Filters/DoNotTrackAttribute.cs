using System.Web.Http.Filters;

namespace CMS.WebAnalytics.Web.UI.Filters
{
    /// <summary>
    /// Adds header to the response telling the crawlers not to track the site the request was made for.
    /// </summary>
    internal class DoNotTrackAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            actionExecutedContext.Response.Headers.Add("X-Robots-Tag", "none");
        }
    }
}
