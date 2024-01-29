using CMS.Core;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents the Web Analytics Web UI module metadata.
    /// </summary>
    public class WebAnalyticsWebUIModuleMetadata : ModuleMetadata
    {
        private const string MODULE_NAME = "CMS.WebAnalytics.Web.UI";

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebAnalyticsWebUIModuleMetadata()
            : base(MODULE_NAME)
        {
        }
    }
}