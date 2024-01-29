using CMS.Helpers;

namespace CMS.Activities
{
    /// <summary>
    /// Provides methods to get correct URL and URL referrer to insert in <see cref="IActivityInfo" />.
    /// </summary>
    internal class ActivityUrlService : IActivityUrlService
    {
        /// <summary>
        /// Gets URL of request the activity was logged for.
        /// </summary>
        /// <returns>URL</returns>
        public string GetActivityUrl()
        {
            return URLHelper.GetAbsoluteUrl(RequestContext.RawURL, RequestContext.URL != null ? RequestContext.URL.Host : null);
        }


        /// <summary>
        /// Gets URL referrer of request the activity was referred from.
        /// </summary>
        /// <returns>URL referrer</returns>
        public string GetActivityUrlReferrer()
        {
            return RequestContext.URLReferrer;
        }
    }
}
