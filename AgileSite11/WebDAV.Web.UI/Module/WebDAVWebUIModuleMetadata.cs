using CMS.Core;

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Represents the WebDAV.Web.UI module metadata.
    /// </summary>
    internal class WebDAVWebUIModuleMetadata : ModuleMetadata
    {
        private const string MODULE_NAME = "CMS.WebDAV.Web.UI";

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebDAVWebUIModuleMetadata()
            : base(MODULE_NAME)
        {
        }
    }
}