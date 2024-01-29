using CMS.Core;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents the Web Analytics module metadata.
    /// </summary>
    public class WebAnalyticsModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebAnalyticsModuleMetadata()
            : base(ModuleName.WEBANALYTICS)
        {
            RootPath = "~/CMSModules/WebAnalytics/";
        }
    }
}