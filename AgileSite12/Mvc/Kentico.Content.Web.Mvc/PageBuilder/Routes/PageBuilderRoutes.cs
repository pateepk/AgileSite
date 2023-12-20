namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides Page builder routes and constants.
    /// </summary>
    internal static class PageBuilderRoutes
    {
        /// <summary>
        /// Name of the widgets route.
        /// </summary>
        public const string WIDGETS_ROUTE_NAME = "Kentico.PageBuilder.Widgets";


        /// <summary>
        /// Default route for Page builder widgets markup retrieval.
        /// </summary>
        public const string WIDGETS_ROUTE = "Kentico.PageBuilder/Widgets/{" + PageBuilderConstants.CULTURE_ROUTE_KEY + "}/{" + PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY + "}/{action}";


        /// <summary>
        /// Name of the sections route.
        /// </summary>
        public const string SECTIONS_ROUTE_NAME = "Kentico.PageBuilder.Sections";


        /// <summary>
        /// Default route for Page builder sections markup retrieval.
        /// </summary>
        public const string SECTIONS_ROUTE = "Kentico.PageBuilder/Sections/{" + PageBuilderConstants.CULTURE_ROUTE_KEY + "}/{" + PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY + "}/{action}";


        /// <summary>
        /// Name of the personalization condition type route.
        /// </summary>
        public const string PERSONALIZATION_CONDITION_TYPE_ROUTE_NAME = "Kentico.PageBuilder.Personalization";


        /// <summary>
        /// Default route for Page builder personalization condition type form markup retrieval.
        /// </summary>
        public const string PERSONALIZATION_CONDITION_TYPE_ROUTE = "Kentico.PageBuilder/Personalization/ConditionTypes/{" + PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY + "}/{action}";


        /// <summary>
        /// Default action name for Page builder components markup retrieval.
        /// </summary>
        public const string DEFAULT_ACTION_NAME = "Index";


        /// <summary>
        /// Name of the metadata route.
        /// </summary>
        public const string METADATA_ROUTE_NAME = "Kentico.PageBuilder.Widgets.Metadata";

        /// <summary>
        /// Route for Page builder metadata retrieval action.
        /// </summary>
        public const string METADATA_ROUTE = "Kentico.PageBuilder/Metadata/GetAll";


        /// <summary>
        /// Name of the route for configuration storing.
        /// </summary>
        public const string CONFIGURATION_STORE_ROUTE_NAME = "Kentico.PageBuilder.Widgets.Store";


        /// <summary>
        /// Name of the route for changing template.
        /// </summary>
        public const string CONFIGURATION_CHANGE_TEMPLATE_ROUTE_NAME = "Kentico.PageBuilder.Templates.Change";


        /// <summary>
        /// Name of the controller for configuration.
        /// </summary>
        public const string CONFIGURATION_CONTROLLER_NAME = "KenticoEditableAreasConfiguration";


        /// <summary>
        /// Route for Page builder configuration store action.
        /// </summary>
        public const string CONFIGURATION_STORE_ROUTE = "Kentico.PageBuilder/" + CONFIGURATION_CONTROLLER_NAME + "/Set";


        /// <summary>
        /// Route for Page builder change template action.
        /// </summary>
        public const string CONFIGURATION_CHANGE_TEMPLATE_ROUTE = "Kentico.PageBuilder/" + CONFIGURATION_CONTROLLER_NAME + "/ChangeTemplate";


        /// <summary>
        /// Name of the route for component properties retrieval.
        /// </summary>
        public const string DEFAULT_PROPERTIES_ROUTE_NAME = "Kentico.PageBuilder.Components.DefaultProperties";


        /// <summary>
        /// Name of the action to retrieve default widget properties.
        /// </summary>
        public const string DEFAULT_WIDGET_PROPERTIES_ACTION_NAME = "GetWidgetDefaultProperties";


        /// <summary>
        /// Name of the action to retrieve default section properties.
        /// </summary>
        public const string DEFAULT_SECTION_PROPERTIES_ACTION_NAME = "GetSectionDefaultProperties";


        /// <summary>
        /// Route for Page builder component properties retrieval action.
        /// </summary>
        public const string DEFAULT_PROPERTIES_ROUTE = "Kentico.PageBuilder/" + CONFIGURATION_CONTROLLER_NAME + "/DefaultProperties/{" + PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY + "}/{action}";


        /// <summary>
        /// Name of the route for configuration loading.
        /// </summary>
        public const string CONFIGURATION_LOAD_ROUTE_NAME = "Kentico.PageBuilder.Widgets.Load";


        /// <summary>
        /// Route for Page builder configuration load action.
        /// </summary>
        public const string CONFIGURATION_LOAD_ROUTE = "Kentico.PageBuilder/" + CONFIGURATION_CONTROLLER_NAME + "/Get/{pageId}";


        /// <summary>
        /// Name of the route for annotated properties form.
        /// </summary>
        public const string FORM_ROUTE_NAME = "Kentico.PageBuilder.Forms";


        /// <summary>
        /// Name of controller for widget properties form.
        /// </summary>
        public const string WIDGET_PROPERTIES_FORM_CONTROLLER_NAME = "KenticoWidgetPropertiesForm";


        /// <summary>
        /// Name of controller for section properties form.
        /// </summary>
        public const string SECTION_PROPERTIES_FORM_CONTROLLER_NAME = "KenticoSectionPropertiesForm";


        /// <summary>
        /// Name of controller for condition type parameters form.
        /// </summary>
        public const string CONDITION_TYPE_PARAMETERS_FORM_CONTROLLER_NAME = "KenticoConditionTypeParametersForm";


        /// <summary>
        /// Route for annotated properties form.
        /// </summary>
        public const string FORM_ROUTE = "Kentico.PageBuilder/Forms/{controller}/{" + PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY + "}/{action}";


        /// <summary>
        /// Name of the action which proceeds form validation.
        /// </summary>
        public const string FORM_VALIDATION_ACTION_NAME = "Validate";
    }
}