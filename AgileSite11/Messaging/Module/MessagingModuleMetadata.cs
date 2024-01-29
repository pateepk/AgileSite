using CMS.Core;

namespace CMS.Messaging
{
    /// <summary>
    /// Represents the Messaging module metadata.
    /// </summary>
    public class MessagingModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MessagingModuleMetadata()
            : base(ModuleName.MESSAGING)
        {
            RootPath = "~/CMSModules/Messaging/";
        }
    }
}