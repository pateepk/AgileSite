using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using CMS.Core;
using CMS.EventLog;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Renders widget zone HTML markup.
    /// </summary>
    internal sealed class WidgetZoneRenderer : IWidgetZoneRenderer
    {
        /// <summary>
        /// Renders widget zone container for edit mode.
        /// </summary>
        /// <returns>Returns HTML markup representing widget zone container for edit mode.</returns>
        public IHtmlString RenderZoneContainer()
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.Attributes.Add("data-kentico-widget-zone", "");

            return new HtmlString(tagBuilder.ToString());
        }


        /// <summary>
        /// Renders zone widgets for the live site.
        /// </summary>
        /// <param name="helper">HTML helper.</param>
        /// <param name="configuration">Editable areas configuration.</param>
        /// <param name="provider">Provider to retrieve widget definitions.</param>
        /// <param name="evaluator">Evaluator to retrieve widget variant to be used for rendering.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/>, <paramref name="provider"/> or <paramref name="evaluator"/> is <c>null</c>.</exception>
        public void RenderWidgets(HtmlHelper helper, EditableAreasConfiguration configuration, IComponentDefinitionProvider<WidgetDefinition> provider, IWidgetVariantEvaluator evaluator)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (evaluator == null)
            {
                throw new ArgumentNullException(nameof(evaluator));
            }

            var routeData = helper.ViewContext.RouteData.Values;
            var area = GetArea(configuration, routeData);
            if (area == null)
            {
                return;
            }

            var section = GetSection(area, routeData);
            if (section == null)
            {
                return;
            }

            var zoneIndex = GetZoneIndex(routeData);
            IncrementZoneIndex(routeData);

            if (section.Zones.Count <= zoneIndex)
            {
                return;
            }

            var zone = section.Zones[zoneIndex];
            var types = provider.GetAll().ToList();
            foreach (var widget in zone.Widgets)
            {
                var type = types.FirstOrDefault(w => w.Identifier.Equals(widget.TypeIdentifier, StringComparison.InvariantCultureIgnoreCase));
                if (type == null)
                {
                    Service.Resolve<IEventLogService>().LogEvent(EventType.ERROR, "WidgetZoneRenderer", "RenderWidgets", $"The '{widget.TypeIdentifier}' widget type is not registered.");

                    continue;
                }

                var variant = evaluator.Evaluate(widget);
                helper.RenderAction(PageBuilderRoutes.DEFAULT_ACTION_NAME, type.ControllerName, new RouteValueDictionary
                {
                    { PageBuilderConstants.COMPONENT_DEFINITION_ROUTE_DATA_KEY, type },
                    { PageBuilderConstants.COMPONENT_PROPERTIES_ROUTE_DATA_KEY, variant.Properties },
                    { PageBuilderConstants.CULTURE_ROUTE_KEY, Thread.CurrentThread.CurrentCulture.Name },
                });
            }
        }


        private static SectionConfiguration GetSection(EditableAreaConfiguration area, RouteValueDictionary routeData)
        {
            if (!routeData.TryGetValue(PageBuilderConstants.SECTION_IDENTIFIER_ROUTE_DATA_KEY, out var sectionIdentifier))
            {
                return null;
            }

            return area.Sections.FirstOrDefault(s => s.Identifier == (Guid)sectionIdentifier);
        }


        private static EditableAreaConfiguration GetArea(EditableAreasConfiguration configuration, RouteValueDictionary routeData)
        {
            if (!routeData.TryGetValue(PageBuilderConstants.AREA_IDENTIFIER_ROUTE_DATA_KEY, out var areaIdentifier))
            {
                return null;
            }

            return configuration.EditableAreas.FirstOrDefault(a => a.Identifier.Equals((string)areaIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }


        private static int GetZoneIndex(RouteValueDictionary routeData)
        {
            if (!routeData.ContainsKey(PageBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY))
            {
                routeData[PageBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY] = 0;
            }

            return (int)routeData[PageBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY];
        }


        private static void IncrementZoneIndex(RouteValueDictionary routeData)
        {
            var index = (int)routeData[PageBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY];
            routeData[PageBuilderConstants.ZONE_INDEX_ROUTE_DATA_KEY] = index + 1;
        }
    }
}
