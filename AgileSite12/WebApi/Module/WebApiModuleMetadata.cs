using CMS.Core;

namespace CMS.WebApi
{
    /// <summary>
    /// Represents the Web API module metadata.
    /// </summary>
    internal class WebApiModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.WebApi.WebApiModuleMetadata"/> class.
        /// </summary>
        public WebApiModuleMetadata() : base(ModuleName.WEBAPI)
        {

        }
    }
}