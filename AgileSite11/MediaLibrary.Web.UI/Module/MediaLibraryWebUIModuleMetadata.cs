using CMS.Core;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Represents the MediaLibrary.Web.UI module metadata.
    /// </summary>
    internal class MediaLibraryWebUIModuleMetadata : ModuleMetadata
    {
        private const string MODULE_NAME = "CMS.MediaLibrary.Web.UI";

         /// <summary>
        /// Default constructor
        /// </summary>
        public MediaLibraryWebUIModuleMetadata()
            : base(MODULE_NAME)
        {
        }
    }
}
