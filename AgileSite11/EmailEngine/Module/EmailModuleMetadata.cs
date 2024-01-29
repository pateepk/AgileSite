using CMS.Core;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents the Email module metadata.
    /// </summary>
    public class EmailModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailModuleMetadata()
            : base(ModuleName.EMAILENGINE)
        {
        }
    }
}