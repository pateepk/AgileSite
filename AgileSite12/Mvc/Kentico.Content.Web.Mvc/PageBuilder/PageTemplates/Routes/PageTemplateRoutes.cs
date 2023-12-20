namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    internal static class PageTemplateRoutes
    {
        /// <summary>
        /// Name of the controller for page templates metadata retrieval.
        /// </summary>
        public const string TEMPLATE_METADATA_CONTROLLER_NAME = "KenticoPageTemplatesViewModel";


        /// <summary>
        /// Name of the page templates metadata retrieval route.
        /// </summary>
        public const string TEMPLATE_ROUTE_NAME = "Kentico.PageTemplates.Metadata";


        /// <summary>
        /// Route for page templates retrieval action.
        /// </summary>
        public const string TEMPLATE_METADATA_ROUTE = "Kentico.PageBuilder/PageTemplates/GetFiltered";


        /// <summary>
        /// Name of the route for page template selector dialog.
        /// </summary>
        public const string TEMPLATE_SELECTOR_DIALOG_ROUTE_NAME = "Kentico.PageTemplates.SelectorDialog";


        /// <summary>
        /// Name of controller for page template selector dialog.
        /// </summary>
        public const string TEMPLATE_SELECTOR_DIALOG_CONTROLLER_NAME = "KenticoPageTemplatesSelectorDialog";


        /// <summary>
        /// Route for page template selector.
        /// </summary>
        public const string TEMPLATE_SELECTOR_DIALOG_ROUTE = "Kentico.PageBuilder/PageTemplates/SelectorDialog";


        /// <summary>
        /// Name of the action to retrieve default page template properties.
        /// </summary>
        public const string DEFAULT_TEMPLATE_PROPERTIES_ACTION_NAME = "GetPageTemplateDefaultProperties";

        
        /// <summary>
        /// Name of controller for page template properties form.
        /// </summary>
        public const string TEMPLATE_PROPERTIES_FORM_CONTROLLER_NAME = "KenticoPageTemplatePropertiesForm";
    }
}
