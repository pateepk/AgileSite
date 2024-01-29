using CMS.Core;

namespace CMS.Polls
{
    /// <summary>
    /// Represents the Polls module metadata.
    /// </summary>
    public class PollsModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PollsModuleMetadata()
            : base(ModuleName.POLLS)
        {
            RootPath = "~/CMSModules/Polls/";
        }
    }
}