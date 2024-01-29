using System.Collections.Generic;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for the inserting javascript logging snippet to the page.
    /// </summary>
    public class ProcessAnalyticsServiceHandler : AdvancedHandler<ProcessAnalyticsServiceHandler, AnalyticsJSEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="queryParams">Query parameters</param>
        public ProcessAnalyticsServiceHandler StartEvent(Dictionary<string, string> queryParams)
        {
            var e = new AnalyticsJSEventArgs
                {
                    QueryParameters = queryParams,
                };

            return StartEvent(e);
        }
    }
}