using CMS.Core;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Represents the Online Marketing module metadata.
    /// </summary>
    internal class ApplicationDashboardModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationDashboardModuleMetadata()
            : base(ModuleName.APPLICATIONDASHBOARD)
        {
            RootPath = "~/CMSModules/ApplicationDashboard/";
        }
    }
}