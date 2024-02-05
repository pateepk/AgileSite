using CMS.Core;

namespace CMS.Activities
{
    /// <summary>
    /// Represents the Activities module metadata.
    /// </summary>
    internal class ActivitiesModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivitiesModuleMetadata()
            : base(ModuleName.ACTIVITIES)
        {
            RootPath = "~/CMSModules/Activities/";
        }
    }
}