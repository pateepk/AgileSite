using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for editable area renderer.
    /// </summary>
    internal interface IEditableAreaRenderer
    {
        /// <summary>
        /// Renders editable area container for edit mode.
        /// </summary>
        /// <param name="defaultSectionIdentifier">Identifier of a default area section.</param>
        /// <param name="allowedWidgets">Identifiers of allowed widgets in the area. If empty all widgets are allowed.</param>
        /// <returns>Returns HTML markup representing editable area container for edit mode.</returns>
        IHtmlString RenderAreaContainer(string defaultSectionIdentifier, IEnumerable<string> allowedWidgets);


        /// <summary>
        /// Renders sections in editable area for the live site.
        /// </summary>
        /// <param name="helper">HTML helper.</param>
        /// <param name="configuration">Editable areas configuration.</param>
        /// <param name="provider">Provider to retrieve section definitions.</param>
        void RenderSections(HtmlHelper helper, EditableAreasConfiguration configuration, IComponentDefinitionProvider<SectionDefinition> provider);
    }
}