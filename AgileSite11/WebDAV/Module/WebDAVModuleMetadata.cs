using CMS.Core;

namespace CMS.WebDAV
{
    /// <summary>
    /// Represents the WebDAV module metadata.
    /// </summary>
    internal class WebDAVModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebDAVModuleMetadata()
            : base(ModuleName.WEBDAV)
        {
            RootPath = "~/CMSModules/WebDAV/";
        }
    }
}   