using System;

using CMS.FormEngine;

namespace Kentico.Forms.Web.Mvc
{
    internal sealed class InvalidComponentConfigurationBuilder : IInvalidComponentConfigurationBuilder
    {
        private readonly IFormComponentActivator formComponentActivator;
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Definition provider for form component properties.</param>
        /// <param name="formComponentActivator">Activator for form component properties.</param>
        public InvalidComponentConfigurationBuilder(IFormComponentActivator formComponentActivator, IFormComponentDefinitionProvider formComponentDefinitionProvider)
        {
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
        }


        /// <summary>
        /// Creates invalid component configuration from <seealso cref="FormFieldInfo"/> with <seealso cref="Exception"/> and localized error message.
        /// </summary>
        /// <param name="formFieldInfo">Source form field info.</param>
        /// <param name="errorMessage">Localized error message.</param>
        /// <param name="exception">Exception related to the invalid component.</param>
        public FormComponentConfiguration CreateInvalidFormComponentConfiguration(FormFieldInfo formFieldInfo, string errorMessage, Exception exception)
        {
            var invalidComponentDefinition = formComponentDefinitionProvider.Get(FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER);
            var invalidComponentProperties = formComponentActivator.CreateDefaultProperties(invalidComponentDefinition) as InvalidComponentProperties;

            invalidComponentProperties.Label = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption);
            invalidComponentProperties.ExplanationText = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.ExplanationText);
            invalidComponentProperties.Tooltip = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription);

            invalidComponentProperties.Guid = formFieldInfo.Guid;
            invalidComponentProperties.ErrorMessage = errorMessage;
            invalidComponentProperties.Exception = exception;

            var invalidFormComponentConfiguration = new FormComponentConfiguration
            {
                TypeIdentifier = FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER,
                Properties = invalidComponentProperties
            };

            return invalidFormComponentConfiguration;
        }


    }
}
