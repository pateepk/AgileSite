using CMS.Core;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Represents the Message Board module metadata.
    /// </summary>
    public class MessageBoardModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MessageBoardModuleMetadata()
            : base(ModuleName.MESSAGEBOARD)
        {
            RootPath = "~/CMSModules/MessageBoards/";
        }
    }
}