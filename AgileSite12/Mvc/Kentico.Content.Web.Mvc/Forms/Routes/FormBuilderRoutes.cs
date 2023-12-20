namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides Form builder route templates and their names.
    /// </summary>
    internal static class FormBuilderRoutes
    {
        /// <summary>
        /// Name of the route for the form builder.
        /// </summary>
        internal const string FORMBUILDER_ROUTE_NAME = "Kentico.FormBuilder";


        /// <summary>
        /// Route template for the form builder.
        /// Serves a page with initialized form builder for a form with given id.
        /// </summary>
        /// <remarks>
        /// Same route is used by form's application to display form builder in the Kentico administration.
        /// </remarks>
        internal const string FORMBUILDER_ROUTE_TEMPLATE = "Kentico.FormBuilder/Index/{formId}";


        /// <summary>
        /// Name of the route for form configuration loading.
        /// </summary>
        internal const string CONFIGURATION_LOAD_ROUTE_NAME = "Kentico.FormBuilder.FormComponents.Load";


        /// <summary>
        /// Route template for the form configuration load action.
        /// </summary>
        internal const string CONFIGURATION_LOAD_ROUTE_TEMPLATE = "Kentico.FormBuilder/Configuration/Get/{formId}";


        /// <summary>
        /// Name of the route for form configuration storing.
        /// </summary>
        internal const string CONFIGURATION_STORE_ROUTE_NAME = "Kentico.FormBuilder.FormComponents.Store";


        /// <summary>
        /// Route template for the form configuration store action.
        /// </summary>
        internal const string CONFIGURATION_STORE_ROUTE_TEMPLATE = "Kentico.FormBuilder/Configuration/Set";


        /// <summary>
        /// Name of the route for form components markup retrieval.
        /// </summary>
        public const string MARKUP_ROUTE_NAME = "Kentico.FormBuilder.FormComponents.Markup";


        /// <summary>
        /// Route template for the form components markup retrieval action.
        /// </summary>
        public const string MARKUP_ROUTE_TEMPLATE = "Kentico.FormBuilder/Markup/EditorRow/{identifier}/{formId}";


        /// <summary>
        /// Name of the route for the Form builder's sections markup retrieval.
        /// </summary>
        public const string SECTION_MARKUP_ROUTE_NAME = "Kentico.FormBuilder.Sections";


        /// <summary>
        /// Route template for the Form builder's sections markup retrieval action.
        /// </summary>
        public const string SECTION_MARKUP_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormSections/{controller}";


        /// <summary>
        /// Default action name for Form builder sections markup retrieval.
        /// </summary>
        public const string DEFAULT_ACTION_NAME = "Index";


        /// <summary>
        /// Name of the form components metadata route.
        /// </summary>
        public const string METADATA_ROUTE_NAME = "Kentico.FormBuilder.FormComponents.Metadata";


        /// <summary>
        /// Route template for the form components metadata retrieval action.
        /// </summary>
        public const string METADATA_ROUTE_TEMPLATE = "Kentico.FormBuilder/Metadata/GetAll/{formId}";


        /// <summary>
        /// Name of the route for form components default properties retrieval.
        /// </summary>
        internal const string PROPERTIES_ROUTE_NAME = "Kentico.FormBuilder.FormComponents.GetDefaultProperties";


        /// <summary>
        /// Route template for the form components default properties retrieval action.
        /// </summary>
        internal const string PROPERTIES_ROUTE_TEMPLATE = "Kentico.FormBuilder/Configuration/GetDefaultProperties/{identifier}/{formId}";


        /// <summary>
        /// Name of the route for form builder default properties panel.
        /// </summary>
        internal const string FORMBUILDER_PROPERTIES_TAB_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderPropertiesPanel";


        /// <summary>
        /// Route template for form builder properties panel.
        /// </summary>
        internal const string FORMBUILDER_PROPERTIES_TAB_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/GetPropertiesMarkup/{formId}";


        /// <summary>
        /// Name of the route for form component properties validation.
        /// </summary>
        internal const string VALIDATE_FORMCOMPONENT_PROPERTIES_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderValidateProperties";


        /// <summary>
        /// Route template for form component properties validation.
        /// </summary>
        internal const string VALIDATE_FORMCOMPONENT_PROPERTIES_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/ValidateProperties/{formComponentInstanceIdentifier}/{formComponentTypeIdentifier}/{formId}/{formFieldName}";


        /// <summary>
        /// Name of the route for retrieving markup to configure validation rule.
        /// </summary>
        internal const string FORMBUILDER_GET_VALIDATION_RULE_CONFIGURATION_MARKUP_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderValidationRuleConfiguration";


        /// <summary>
        /// Route template for retrieving markup to configure validation rule.
        /// </summary>
        internal const string FORMBUILDER_GET_VALIDATION_RULE_CONFIGURATION_MARKUP_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/GetValidationRuleConfiguration/{formId}";


        /// <summary>
        /// Name of the route for validation of validation rule's configuration.
        /// </summary>
        internal const string FORMBUILDER_VALIDATE_VALIDATION_RULE_CONFIGURATION_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderValidateValidationRuleConfiguration";


        /// <summary>
        /// Route template for validation of validation rule's configuration.
        /// </summary>
        internal const string FORMBUILDER_VALIDATE_VALIDATION_RULE_CONFIGURATION_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/ValidateValidationRuleConfiguration/{formId}/{formFieldName}";


        /// <summary>
        /// Name of the route for validation of validation rule's configuration.
        /// </summary>
        internal const string FORMBUILDER_GET_VISIBILITY_CONDITION_CONFIGURATION_MARKUP_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderVisibilityConditionConfigurationMarkup";


        /// <summary>
        /// Route template for validation of validation rule's configuration.
        /// </summary>
        internal const string FORMBUILDER_GET_VISIBILITY_CONDITION_CONFIGURATION_MARKUP_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/GetVisibilityConditionConfigurationMarkup/{formId}";
        
        
        /// <summary>
        /// Name of the route for validation of visibility condition configuration.
        /// </summary>
        internal const string FORMBUILDER_VALIDATE_VISIBILITY_CONDITION_CONFIGURATION_ROUTE_NAME = "Kentico.FormBuilder.FormBuilderValidateVisiblityConditionConfiguration";


        /// <summary>
        /// Route template for validation of visibility condition's configuration.
        /// </summary>
        internal const string FORMBUILDER_VALIDATE_VISIBILITY_CONDITION_CONFIGURATION_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormBuilderPropertiesPanel/ValidateVisibilityConditionConfiguration/{formId}/{formFieldName}/{formComponentInstanceIdentifier}";

        
        /// <summary>
        /// Name of the route for form item preview.
        /// </summary>
        internal const string FORM_ITEM_PREVIEW_ROUTE_NAME = "Kentico.FormBuilder.FormItemPreview";
        
        
        /// <summary>
        /// Route template for form item preview.
        /// </summary>
        internal const string FORM_ITEM_PREVIEW_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormItem/Preview/{formId}/{itemId}";

        
        /// <summary>
        /// Name of the route for edit form item.
        /// </summary>
        internal const string FORM_ITEM_EDIT_ROUTE_NAME = "Kentico.FormBuilder.EditFormItem";
        
        
        /// <summary>
        /// Route template for edit form item.
        /// </summary>
        internal const string FORM_ITEM_EDIT_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormItem/Edit/{formId}/{itemId}";



        /// <summary>
        /// Name of the route for submitting edited form item.
        /// </summary>
        internal const string FORM_ITEM_EDIT_SUBMIT_ROUTE_NAME = "Kentico.FormBuilder.EditFormItemSubmit";


        /// <summary>
        /// Route template for submitting edited form item.
        /// </summary>
        internal const string FORM_ITEM_EDIT_SUBMIT_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormItem/Submit";


        /// <summary>
        /// Name of the route for validation rule metadata.
        /// </summary>
        internal const string VALIDATION_RULE_METADATA_ROUTE_NAME = "Kentico.FormBuilder.ValidationRulesMetadata";


        /// <summary>
        /// Route template for validation rule metadata.
        /// </summary>
        internal const string VALIDATION_RULE_METADATA_ROUTE_TEMPLATE = "Kentico.FormBuilder/ValidationRules/GetAll";


        /// <summary>
        /// Name of the route for posting the form uploader files.
        /// </summary>
        internal const string FILE_UPLOADER_POST_ROUTE_NAME = "Kentico.FormBuilder.FormFileUploader.PostFile";


        /// <summary>
        /// Route template for posting the form uploader files.
        /// </summary>
        internal const string FILE_UPLOADER_POST_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormFileUploader/Post";
        

        /// <summary>
        /// Name of the route for deletion of the form uploader files.
        /// </summary>
        internal const string FILE_UPLOADER_DELETE_ROUTE_NAME = "Kentico.FormBuilder.FormFileUploader.DeleteFile";
        

        /// <summary>
        /// Route template for deletion of the form uploader files.
        /// </summary>
        internal const string FILE_UPLOADER_DELETE_ROUTE_TEMPLATE = "Kentico.FormBuilder/FormFileUploader/Delete";


        /// <summary>
        /// Route template for getting an uploaded form file.
        /// </summary>
        internal const string GET_FILE_ROUTE_TEMPLATE = "Kentico.FormBuilder/File/Get";
    }
}
