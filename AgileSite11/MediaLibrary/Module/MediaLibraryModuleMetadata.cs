using CMS.Core;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Represents the Media Library module metadata.
    /// </summary>
    public class MediaLibraryModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MediaLibraryModuleMetadata()
            : base(ModuleName.MEDIALIBRARY)
        {
            RootPath = "~/CMSModules/MediaLibrary/";
        }
    }
}