using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Arguments for event handlers related to generating sample statistics data. 
    /// </summary>
    public class GenerateStatisticsEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Date for which the statistics should be generated.
        /// </summary>
        public DateTime Date
        {
            get;
            set;
        }


        /// <summary>
        /// Contains number of visitor per every culture.
        /// </summary>
        public IDictionary<string, int> Visitors
        {
            get;
            set;
        }
    }
}
