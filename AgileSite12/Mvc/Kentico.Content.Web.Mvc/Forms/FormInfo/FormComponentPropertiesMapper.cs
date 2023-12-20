using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains mapping methods for conversions between form component properties and form field definition.
    /// </summary>
    /// <seealso cref="FormComponentProperties"/>
    /// <seealso cref="FormFieldInfo"/>
    public class FormComponentPropertiesMapper : IFormComponentPropertiesMapper
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;
        private readonly IEditablePropertiesCollector editablePropertiesCollector;
        private readonly IValidationRuleConfigurationsXmlSerializer validationRuleConfigurationsXmlSerializer;
        private readonly IVisibilityConditionConfigurationXmlSerializer visibilityConditionConfigurationXmlSerializer;

        private readonly HashSet<string> implicitlyHandledFormComponentPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(FormComponentProperties.Required),
            nameof(FormComponentProperties.DataType),
            nameof(FormComponentProperties<object>.DefaultValue),
            nameof(FormComponentProperties.ExplanationText),
            nameof(FormComponentProperties.Guid),
            nameof(FormComponentProperties.Label),
            nameof(FormComponentProperties.Name),
            nameof(FormComponentProperties.Precision),
            nameof(FormComponentProperties.Size),
            nameof(FormComponentProperties.Tooltip),
            nameof(FormComponentProperties.SmartField)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentPropertiesMapper"/> class.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Provider for registered form components retrieval.</param>
        /// <param name="formComponentActivator">Activator for form components.</param>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="validationRuleConfigurationsXmlSerializer">Serializer for validation rules stored in <see cref="FormFieldInfo.ValidationRuleConfigurationsXmlData"/>.</param>
        /// <param name="visibilityConditionConfigurationXmlSerializer">Serializer for visibility condition stored in <see cref="FormFieldInfo.VisibilityConditionConfigurationXmlData"/>.</param>
        public FormComponentPropertiesMapper(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator, IEditablePropertiesCollector editablePropertiesCollector,
            IValidationRuleConfigurationsXmlSerializer validationRuleConfigurationsXmlSerializer, IVisibilityConditionConfigurationXmlSerializer visibilityConditionConfigurationXmlSerializer)
        {
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
            this.editablePropertiesCollector = editablePropertiesCollector ?? throw new ArgumentNullException(nameof(editablePropertiesCollector));
            this.validationRuleConfigurationsXmlSerializer = validationRuleConfigurationsXmlSerializer ?? throw new ArgumentNullException(nameof(validationRuleConfigurationsXmlSerializer));
            this.visibilityConditionConfigurationXmlSerializer = visibilityConditionConfigurationXmlSerializer ?? throw new ArgumentNullException(nameof(visibilityConditionConfigurationXmlSerializer));
        }


        /// <summary>
        /// Maps an instance of <see cref="FormFieldInfo"/> to corresponding <see cref="FormComponentProperties"/>.
        /// To extract identifier of corresponding <see cref="FormComponentDefinition"/> use <see cref="GetComponentIdentifier"/>.
        /// </summary>
        /// <param name="formFieldInfo">Form field to be mapped.</param>
        /// <returns>Returns an instance of <see cref="FormComponentProperties"/> which corresponds to given form field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFieldInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// <para>Thrown when <paramref name="formFieldInfo"/> contains an unknown form component identifier</para>
        /// <para>-or-</para>
        /// <para>when corresponding component properties contain a property of type which is not supported by <see cref="DataTypeManager"/>.</para>
        /// </exception>
        public virtual FormComponentProperties FromFieldInfo(FormFieldInfo formFieldInfo)
        {
            if (formFieldInfo == null)
            {
                throw new ArgumentNullException(nameof(formFieldInfo));
            }

            var typeIdentifier = GetComponentIdentifier(formFieldInfo);
            var definition = formComponentDefinitionProvider.Get(typeIdentifier);
            if (definition == null)
            {
                throw new InvalidOperationException($"Cannot find form component definition '{typeIdentifier}'. Corresponding form component properties cannot be created.");
            }

            var properties = formComponentActivator.CreateDefaultProperties(definition);

            properties.DataType = formFieldInfo.DataType;
            properties.Size = formFieldInfo.Size;
            properties.Precision = formFieldInfo.Precision;

            properties.Guid = formFieldInfo.Guid;
            properties.Name = formFieldInfo.Name;
            properties.Required = !formFieldInfo.AllowEmpty;

            properties.Label = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption);
            properties.ExplanationText = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.ExplanationText);
            properties.Tooltip = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription);
            properties.SmartField = formFieldInfo.IsSmartField();

            var defaultValue = formFieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue);
            if (defaultValue != null)
            {
                var componentDataType = DataTypeManager.GetDataType(TypeEnum.Field, formFieldInfo.DataType);
                var convertedDefaultValue = componentDataType.TextSerializer.Deserialize(defaultValue);

                properties.SetDefaultValue(convertedDefaultValue);
            }
            else
            {
                var type = properties.GetType().FindTypeByGenericDefinition(typeof(FormComponentProperties<>));
                var value = type.GetDefaultValue();

                properties.SetDefaultValue(value);
            }

            MapCustomProperties(properties, formFieldInfo);

            MapValidationRules(formFieldInfo, properties);
            MapVisibilityCondition(formFieldInfo, properties);

            return properties;
        }


        /// <summary>
        /// Maps an instance of <see cref="FormComponentProperties"/> to corresponding <see cref="FormFieldInfo"/>.
        /// </summary>
        /// <param name="formComponentProperties">Form component properties to be mapped.</param>
        /// <param name="componentIdentifier">Identifier of corresponding <see cref="FormComponentDefinition"/> to be stored along with <paramref name="formComponentProperties"/>.</param>
        /// <returns>Returns an instance of <see cref="FormFieldInfo"/> which corresponds to given properties and type identifier.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formComponentProperties"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="formComponentProperties"/> contain a property of type which is not supported by <see cref="DataTypeManager"/>.</exception>
        public virtual FormFieldInfo ToFormFieldInfo(FormComponentProperties formComponentProperties, string componentIdentifier)
        {
            if (formComponentProperties == null)
            {
                throw new ArgumentNullException(nameof(formComponentProperties));
            }

            FormFieldInfo formFieldInfo = new FormFieldInfo
            {
                DataType = formComponentProperties.DataType,
                Size = FormComponentDataTypeHelper.EnsureSize(formComponentProperties.DataType, formComponentProperties.Size),
                Precision = FormComponentDataTypeHelper.EnsurePrecision(formComponentProperties.DataType, formComponentProperties.Precision),

                Guid = formComponentProperties.Guid,
                Name = formComponentProperties.Name,
                AllowEmpty = !formComponentProperties.Required,
                PublicField = true
            };

            formFieldInfo.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, formComponentProperties.Label);
            formFieldInfo.SetPropertyValue(FormFieldPropertyEnum.ExplanationText, formComponentProperties.ExplanationText);
            formFieldInfo.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, formComponentProperties.Tooltip);
            formFieldInfo.SetPropertyValue(FormFieldPropertyEnum.Smart, formComponentProperties.SmartField.ToString(CultureInfo.InvariantCulture));
            
            var componentDataType = DataTypeManager.GetDataType(TypeEnum.Field, formComponentProperties.DataType);
            var convertedDefaultValue = componentDataType.TextSerializer.Serialize(formComponentProperties.GetDefaultValue());

            formFieldInfo.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, convertedDefaultValue);

            formFieldInfo.Settings[FormInfoConstants.SETTINGS_FORM_COMPONENT_IDENTIFIER] = componentIdentifier;

            var propertiesToSave = editablePropertiesCollector.GetEditableProperties(formComponentProperties)
                                                                   .Where(prop => !implicitlyHandledFormComponentPropertyNames.Contains(prop.Name));

            foreach (var property in propertiesToSave)
            {
                if (DataTypeManager.GetDataType(property.PropertyType) == null)
                {
                    throw new InvalidOperationException($"Form component properties '{formComponentProperties.GetType()}' contain property '{property.Name}' of unsupported type '{property.PropertyType}'. A new type can be registered to the system using '{typeof(DataTypeManager)}.{nameof(DataTypeManager.RegisterDataTypes)}'.");
                }

                var propertyValue = property.GetValue(formComponentProperties, null);
                formFieldInfo.Settings[property.Name] = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldDataType.Text, propertyValue);
            }

            MapValidationRules(formComponentProperties, formFieldInfo);
            MapVisibilityCondition(formComponentProperties, formFieldInfo);

            return formFieldInfo;
        }


        /// <summary>
        /// Gets <see cref="FormComponentDefinition"/> identifier stored in <paramref name="formFieldInfo"/>.
        /// </summary>
        /// <param name="formFieldInfo">Form field info for which to obtain <see cref="FormComponentDefinition"/>'s identifier.</param>
        /// <returns>Return identifier obtained from <paramref name="formFieldInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFieldInfo"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="formFieldInfo"/> does not specify a form component identifier.</exception>
        public virtual string GetComponentIdentifier(FormFieldInfo formFieldInfo)
        {
            if (formFieldInfo == null)
            {
                throw new ArgumentNullException(nameof(formFieldInfo));
            }

            var componentIdentifier = formFieldInfo.Settings[FormInfoConstants.SETTINGS_FORM_COMPONENT_IDENTIFIER] as string;
            if (String.IsNullOrEmpty(componentIdentifier))
            {
                throw new InvalidOperationException($"Form field '{formFieldInfo.Name}' does not contain a form component identifier in its settings.");
            }

            return componentIdentifier;
        }


        /// <summary>
        /// Maps properties of <paramref name="formFieldInfo"/> that are not handled implicitly to <paramref name="properties"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="properties"/> contains a property of type which is not supported by <see cref="DataTypeManager"/>.</exception>
        private void MapCustomProperties(FormComponentProperties properties, FormFieldInfo formFieldInfo)
        {
            var propertiesToLoad = editablePropertiesCollector.GetEditableProperties(properties)
                                                                   .Where(prop => !implicitlyHandledFormComponentPropertyNames.Contains(prop.Name));
            foreach (var property in propertiesToLoad)
            {
                var propertyName = property.Name;

                if (formFieldInfo.Settings.ContainsKey(propertyName))
                {
                    var propertyDataType = DataTypeManager.GetDataType(property.PropertyType);
                    if (propertyDataType == null)
                    {
                        throw new InvalidOperationException($"Form component properties '{properties.GetType()}' contain property '{property.Name}' of unsupported type '{property.PropertyType}'. A new type can be registered to the system using '{typeof(DataTypeManager)}.{nameof(DataTypeManager.RegisterDataTypes)}'.");
                    }

                    var propertyValueString = formFieldInfo.Settings[propertyName];
                    property.SetValue(properties, DataTypeManager.ConvertToSystemType(TypeEnum.Field, propertyDataType.FieldType, propertyValueString));
                }
                else
                {
                    var value = property.PropertyType.GetDefaultValue();

                    property.SetValue(properties, value);
                }
            }
        }


        /// <summary>
        /// Maps validation rules from <paramref name="fromFormFieldInfo"/>'s <see cref="FormFieldInfo.ValidationRuleConfigurationsXmlData"/> to <paramref name="toProperties"/>.
        /// </summary>
        private void MapValidationRules(FormFieldInfo fromFormFieldInfo, FormComponentProperties toProperties)
        {
            toProperties.ValidationRuleConfigurations = validationRuleConfigurationsXmlSerializer.Deserialize(fromFormFieldInfo.ValidationRuleConfigurationsXmlData);
        }


        /// <summary>
        /// Maps visibility condition from <paramref name="fromFormFieldInfo"/>'s <see cref="FormFieldInfo.VisibilityConditionConfigurationXmlData"/> to <paramref name="toProperties"/>.
        /// </summary>
        /// <param name="fromFormFieldInfo"></param>
        /// <param name="toProperties"></param>
        private void MapVisibilityCondition(FormFieldInfo fromFormFieldInfo, FormComponentProperties toProperties)
        {
            toProperties.VisibilityConditionConfiguration = visibilityConditionConfigurationXmlSerializer.Deserialize(fromFormFieldInfo.VisibilityConditionConfigurationXmlData);
        }


        /// <summary>
        /// Maps validation rules from <paramref name="fromProperties"/> to <paramref name="toFormFieldInfo"/>'s <see cref="FormFieldInfo.ValidationRuleConfigurationsXmlData"/>.
        /// </summary>
        private void MapValidationRules(FormComponentProperties fromProperties, FormFieldInfo toFormFieldInfo)
        {
            toFormFieldInfo.ValidationRuleConfigurationsXmlData = validationRuleConfigurationsXmlSerializer.Serialize(fromProperties.ValidationRuleConfigurations);
        }


        /// <summary>
        /// Maps visibility condition from <paramref name="fromProperties"/> to <paramref name="toFormFieldInfo"/>'s <see cref="FormFieldInfo.VisibilityConditionConfigurationXmlData"/>.
        /// </summary>
        private void MapVisibilityCondition(FormComponentProperties fromProperties, FormFieldInfo toFormFieldInfo)
        {
            toFormFieldInfo.VisibilityConditionConfigurationXmlData = visibilityConditionConfigurationXmlSerializer.Serialize(fromProperties.VisibilityConditionConfiguration);
        }
    }
}
