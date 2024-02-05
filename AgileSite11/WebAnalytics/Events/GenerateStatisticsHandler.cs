using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for generating sample web analytics statistics.
    /// </summary>
    public class GenerateStatisticsHandler : AdvancedHandler<GenerateStatisticsHandler, GenerateStatisticsEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="date">Date for which the statistics should be generated</param>
        /// <param name="visitors">Contains number of visitor per every culture</param>
        public GenerateStatisticsHandler StartEvent(DateTime date, IDictionary<string, int> visitors)
        {
            var e = new GenerateStatisticsEventArgs
            {
                Date = date,
                Visitors = visitors,
            };

            return StartEvent(e);
        }
    }
}