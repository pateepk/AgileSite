using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Arguments for event handlers related to logging analytics via JS.
    /// </summary>
    public class AnalyticsJSEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Parameters to be passed via Ajax request when JavaScript logging is enabled.
        /// </summary>
        public Dictionary<string, string> QueryParameters
        {
            get;
            set;
        }
    }
}
