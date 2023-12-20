using CMS.Base;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Arguments of event represented by <see cref="GetConfigurationHandler"/>.
    /// </summary>
    public class GetConfigurationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets the Page builder script configuration.
        /// </summary>
        public PageBuilderScriptConfiguration Configuration
        {
            get;
            set;
        }
    }
}
