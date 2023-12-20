namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Constants for Page builder feature.
    /// </summary>
    public sealed class PageBuilderConstants
    {
        /// <summary>
        /// Source of the event log.
        /// </summary>
        internal const string EVENT_LOG_SOURCE = "PageBuilder";

        
        /// <summary>
        /// Route data key used to provide component definition to a controller.
        /// </summary>
        internal const string COMPONENT_DEFINITION_ROUTE_DATA_KEY = "Kentico.PageBuilder.ComponentDefinition";

        
        /// <summary>
        /// Route data key used to provide page template definition to a controller.
        /// </summary>
        internal const string PAGE_TEMPLATE_DEFINITION_ROUTE_DATA_KEY = "Kentico.PageBuilder.PageTemplateDefinition";


        /// <summary>
        /// Route data key used to provide component properties to a controller.
        /// </summary>
        internal const string COMPONENT_PROPERTIES_ROUTE_DATA_KEY = "Kentico.PageBuilder.ComponentProperties";


        /// <summary>
        /// Route data key used to provide area identifier.
        /// </summary>
        internal const string AREA_IDENTIFIER_ROUTE_DATA_KEY = "Kentico.PageBuilder.AreaIdentifier";


        /// <summary>
        /// Route data key used to provide section identifier.
        /// </summary>
        internal const string SECTION_IDENTIFIER_ROUTE_DATA_KEY = "Kentico.PageBuilder.SectionIdentifier";


        /// <summary>
        /// Route data key used to provide culture identifier.
        /// </summary>
        internal const string CULTURE_ROUTE_KEY = "cultureCode";


        /// <summary>
        /// Route data key used to provide component type identifier.
        /// </summary>
        internal const string TYPE_IDENTIFIER_ROUTE_KEY = "typeIdentifier";


        /// <summary>
        /// Route data key used to provide zone index within the section.
        /// </summary>
        internal const string ZONE_INDEX_ROUTE_DATA_KEY = "Kentico.PageBuilder.ZoneIndex";


        /// <summary>
        /// Name of the folder where is the view of a section.
        /// </summary>
        internal const string SECTION_VIEW_FOLDER = "Sections";


        /// <summary>
        /// Name of the folder where is the view of a widget.
        /// </summary>
        internal const string WIDGET_VIEW_FOLDER = "Widgets";


        /// <summary>
        /// Name of the folder where is the view of a page template.
        /// </summary>
        internal const string PAGE_TEMPLATE_VIEW_FOLDER = "PageTemplates";


        /// <summary>
        /// Name of the default component controller.
        /// </summary>
        internal const string COMPONENT_DEFAULT_CONTROLLER_NAME = "KenticoComponentDefault";


        /// <summary>
        /// Name of the default page template controller.
        /// </summary>
        internal const string PAGE_TEMPLATE_DEFAULT_CONTROLLER_NAME = "KenticoPageTemplateDefault";
    }
}