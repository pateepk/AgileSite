using System.Collections.Generic;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for inserting web analytics javascript snippet to the page.
    /// </summary>
    public class InsertAnalyticsJSHandler : AdvancedHandler<InsertAnalyticsJSHandler, AnalyticsJSEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="queryParams">Query parameters to be sent via Ajax</param>
        public InsertAnalyticsJSHandler StartEvent(Dictionary<string, string> queryParams)
        {
            var e = new AnalyticsJSEventArgs
            {
                QueryParameters = queryParams,
            };

            return StartEvent(e);
        }
    }
}