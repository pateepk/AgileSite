using CMS.Core;

namespace CMS.Reporting
{
    /// <summary>
    /// Represents the Reporting module metadata.
    /// </summary>
    public class ReportingModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ReportingModuleMetadata()
            : base(ModuleName.REPORTING)
        {
            RootPath = "~/CMSModules/Reporting/";
        }
    }
}