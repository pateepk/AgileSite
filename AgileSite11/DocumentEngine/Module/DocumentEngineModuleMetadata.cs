using CMS.Core;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the Document Engine module metadata.
    /// </summary>
    public class DocumentEngineModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentEngineModuleMetadata()
            : base(ModuleName.DOCUMENTENGINE)
        {
        }
    }
}