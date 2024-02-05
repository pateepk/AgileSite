using System;
using System.Globalization;

using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Prepares an activity URL for further processing.
    /// </summary>
    internal class ActivityUrlPreprocessor : IActivityUrlPreprocessor
    {
        /// <summary>
        /// Processes the activity URL for further processing (eg. hash computation)
        /// </summary>
        /// <param name="activityUrl">Activity URL</param>
        /// <returns>URL without protocol and query parameters</returns>
        public string PreprocessActivityUrl(string activityUrl)
        {
            var url = URLHelper.RemoveQuery(activityUrl);
            url = RemoveAnchor(url);
            url = URLHelper.RemoveProtocol(url);
            url = URLHelper.RemoveTrailingSlashFromURL(url);

            return url.ToLower(CultureInfo.InvariantCulture);
        }


        private string RemoveAnchor(string url)
        {
            var index = url.IndexOf("#", StringComparison.Ordinal);
            if (index > 0)
            {
                return url.Substring(0, index);
            }

            return url;
        }
    }
}
