using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Defines methods that are used for filtering page templates. 
    /// </summary>
    public interface IPageTemplateFilter : IComponentFilter
    {
        /// <summary>
        /// Applies filtering on the given <paramref name="pageTemplates" /> collection based on the given <paramref name="context" />.
        /// </summary>
        /// <param name="pageTemplates">Page template collection to filter.</param>
        /// <param name="context">The context that provides information about where the page templates are about to be used.</param>
        /// <returns>
        /// Filtered page templates.
        /// </returns>
        IEnumerable<PageTemplateDefinition> Filter(IEnumerable<PageTemplateDefinition> pageTemplates, PageTemplateFilterContext context);
    }
}
