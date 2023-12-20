using System.Web;
using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for zone renderer.
    /// </summary>
    internal interface IWidgetZoneRenderer
    {
        /// <summary>
        /// Renders widget zone container for edit mode.
        /// </summary>
        /// <returns>Returns HTML markup representing widget zone container for edit mode.</returns>
        IHtmlString RenderZoneContainer();


        /// <summary>
        /// Renders zone widgets for the live site.
        /// </summary>
        /// <param name="helper">HTML helper.</param>
        /// <param name="configuration">Editable areas configuration.</param>
        /// <param name="provider">Provider to retrieve widget definitions.</param>
        /// <param name="evaluator">Evaluator to retrieve widget variant to be used for rendering.</param>
        void RenderWidgets(HtmlHelper helper, EditableAreasConfiguration configuration, IComponentDefinitionProvider<WidgetDefinition> provider, IWidgetVariantEvaluator evaluator);
    }
}