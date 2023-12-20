using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents the page builder filter collections.
    /// </summary>
    public static class PageBuilderFilters
    {
        /// <summary>
        /// Initializes the <see cref="PageBuilderFilters"/> class.
        /// Is called when any of the class properties is being accessed for the first time.
        /// </summary>
        static PageBuilderFilters()
        {
            PageTemplates = new ComponentFilterCollection<IPageTemplateFilter>();
        }


        /// <summary>
        /// Gets the page template filters.
        /// </summary>
        public static ComponentFilterCollection<IPageTemplateFilter> PageTemplates { get; private set; }
    }
}
