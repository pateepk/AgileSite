using CMS.Core;

namespace CMS.Chat
{
    /// <summary>
    /// Represents the Chat module metadata.
    /// </summary>
    public class ChatModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ChatModuleMetadata()
            : base(ModuleName.CHAT)
        {
            RootPath = "~/CMSModules/Chat/";
        }
    }
}