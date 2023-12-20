using CMS.Core;

namespace CMS.Community
{
    /// <summary>
    /// Represents the Community module metadata.
    /// </summary>
    public class CommunityModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CommunityModuleMetadata()
            : base(ModuleName.COMMUNITY)
        {
            RootPath = "~/CMSModules/Community/";
        }
    }
}