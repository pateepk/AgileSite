using CMS.Core;

namespace CMS.Automation
{
    /// <summary>
    /// Represents the Automation module metadata.
    /// </summary>
    public class AutomationModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AutomationModuleMetadata()
            : base(ModuleName.AUTOMATION)
        {
            RootPath = "~/CMSModules/Automation/";
        }
    }
}