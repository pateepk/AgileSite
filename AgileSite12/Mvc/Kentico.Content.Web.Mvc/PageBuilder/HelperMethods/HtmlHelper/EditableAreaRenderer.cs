using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using CMS.Core;
using CMS.EventLog;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Renders editable area HTML markup.
    /// </summary>
    internal sealed class EditableAreaRenderer : IEditableAreaRenderer
    {
        private readonly string areaIdentifier;


        /// <summary>
        /// Creates an instance of <see cref="EditableAreaRenderer"/> class.
        /// </summary>
        /// <param name="areaIdentifier">Area identifier.</param>
        public EditableAreaRenderer(string areaIdentifier)
        {
            this.areaIdentifier = areaIdentifier.Trim();
        }


        /// <summary>
        /// Renders editable area container for edit mode.
        /// </summary>
        /// <param name="defaultSectionIdentifier">Identifier of a default area section.</param>
        /// <param name="allowedWidgets">Identifiers of allowed widgets in the area. If empty all widgets are allowed.</param>
        /// <returns>Returns HTML markup representing editable area container for edit mode.</returns>
        public IHtmlString RenderAreaContainer(string defaultSectionIdentifier, IEnumerable<string> allowedWidgets)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.Attributes.Add("data-kentico-editable-area-id", areaIdentifier);
            tagBuilder.Attributes.Add("data-kentico-allowed-widgets", JsonConvert.SerializeObject(allowedWidgets));
            tagBuilder.Attributes.Add("data-kentico-default-section", defaultSectionIdentifier);

            return new HtmlString(tagBuilder.ToString());
        }


        /// <summary>
        /// Renders sections in editable area for the live site.
        /// </summary>
        /// <param name="helper">HTML helper.</param>
        /// <param name="configuration">Editable areas configuration.</param>
        /// <param name="provider">Provider to retrieve section definitions.</param>
        public void RenderSections(HtmlHelper helper, EditableAreasConfiguration configuration, IComponentDefinitionProvider<SectionDefinition> provider)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var area = configuration.EditableAreas.FirstOrDefault(a => a.Identifier.Equals(areaIdentifier, StringComparison.InvariantCultureIgnoreCase));
            if (area == null)
            {
                return;
            }

            var types = provider.GetAll().ToList();
            foreach(var section in area.Sections)
            {
                var type = types.FirstOrDefault(s => s.Identifier.Equals(section.TypeIdentifier, StringComparison.InvariantCultureIgnoreCase));
                if (type == null)
                {
                    Service.Resolve<IEventLogService>().LogEvent(EventType.ERROR, "EditableAreaRenderer", "RenderSections", $"The '{section.TypeIdentifier}' section is not registered.");

                    continue;
                }

                helper.RenderAction(PageBuilderRoutes.DEFAULT_ACTION_NAME, type.ControllerName, new RouteValueDictionary
                {
                    { PageBuilderConstants.AREA_IDENTIFIER_ROUTE_DATA_KEY, areaIdentifier },
                    { PageBuilderConstants.SECTION_IDENTIFIER_ROUTE_DATA_KEY, section.Identifier },
                    { PageBuilderConstants.COMPONENT_DEFINITION_ROUTE_DATA_KEY, type },
                    { PageBuilderConstants.COMPONENT_PROPERTIES_ROUTE_DATA_KEY, section.Properties },
                    { PageBuilderConstants.CULTURE_ROUTE_KEY, Thread.CurrentThread.CurrentCulture.Name },
                    // If MVC areas are used, always search for the section controller outside of the current area scope
                    { "area",  String.Empty}
                });
            }
        }
    }
}
