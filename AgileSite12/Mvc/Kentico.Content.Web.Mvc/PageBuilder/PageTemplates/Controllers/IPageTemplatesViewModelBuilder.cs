using Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Builds page templates view model.
    /// </summary>
    internal interface IPageTemplatesViewModelBuilder
    {
        /// <summary>
        /// Creates view model for all registered default page templates and all custom templates.
        /// </summary>
        /// <param name="filterContext">Filter context which is passed to each filter.</param>
        PageTemplatesViewModel Build(PageTemplateFilterContext filterContext);
    }
}