using Kentico.Builder.Web.Mvc.Internal;
using Kentico.Components.Web.Mvc.Internal;
using Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Encapsulates data sent to client and used by Page builder's script.
    /// </summary>
    /// <seealso cref="HtmlHelperExtensions.PageBuilderScripts"/>
    public sealed class PageBuilderScriptConfiguration : BuilderScriptConfiguration
    {
        /// <summary>
        /// Identifier of the page where the Page builder widgets will be initialized.
        /// </summary>
        public int PageIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Configuration for page template feature.
        /// </summary>
        public PageTemplateScriptConfiguration PageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Configuration for selector components.
        /// </summary>
        public SelectorsScriptConfiguration Selectors
        {
            get;
            set;
        }
    }
}