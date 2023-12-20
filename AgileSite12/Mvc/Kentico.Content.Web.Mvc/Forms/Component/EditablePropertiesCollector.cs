using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kentico.Forms.Web.Mvc.AnnotationExtensions;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Collects properties editable via <see cref="FormComponent{TProperties, TValue}"/>s from a model.
    /// </summary>
    /// <seealso cref="FormComponentProperties"/>
    /// <seealso cref="FormComponent{TProperties, TValue}"/>
    internal class EditablePropertiesCollector : IEditablePropertiesCollector
    {
        private const int DEFAULT_ORDER_VALUE = 0;

        private readonly IFormComponentDefinitionProvider mFormComponentDefinitionProvider;
        private readonly IFormComponentActivator mFormComponentActivator;
        private readonly IEditingComponentConfigurator mEditingComponentConfigurator;


        /// <summary>
        /// Initializes a new instance of the <see cref="EditablePropertiesCollector"/> class.
        /// </summary>
        public EditablePropertiesCollector(IFormComponentDefinitionProvider formComponentDefinitionProvider,
            IFormComponentActivator formComponentActivator, IEditingComponentConfigurator editingComponentConfigurator)
        {
            mFormComponentDefinitionProvider = formComponentDefinitionProvider;
            mFormComponentActivator = formComponentActivator;
            mEditingComponentConfigurator = editingComponentConfigurator;
        }


        /// <summary>
        /// Collects properties annotated with <see cref="EditingComponentAttribute"/> from given <paramref name="model"/> and
        /// returns collection of <see cref="FormComponent"/>s used for editing those properties in UI.
        /// </summary>
        /// <param name="model">Object with editable properties.</param>
        /// <param name="context">Contextual information specifying where the form components are being used.</param>
        /// <returns>
        /// Collection of <see cref="FormComponent"/>s that enables to edit <paramref name="model"/>
        /// in Form builder's UI. Values from the <paramref name="model"/> are bound to components that display them.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public IEnumerable<FormComponent> GetFormComponents(object model, FormComponentContext context)
        {
            return GetEditablePropertiesWithEditorsCore(model, context).Select(kvp => kvp.Value);
        }


        /// <summary>
        /// Returns a collection of <see cref="PropertyInfo"/>s editable in Form builder's UI.
        /// </summary>
        /// <param name="model">Object with editable properties.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public IEnumerable<PropertyInfo> GetEditableProperties(object model)
        {
            return GetEditablePropertiesCore(model);
        }



        /// <summary>
        /// Returns key value pair collection of <see cref="FormComponent"/>s paired with the <see cref="PropertyInfo"/>
        /// that is edited via given <see cref="FormComponent"/> in Form builder's UI.
        /// </summary>
        /// <param name="model">Object with editable properties.</param>
        /// <param name="context">Contextual information specifying where the form components are being used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public IEnumerable<KeyValuePair<PropertyInfo, FormComponent>> GetEditablePropertiesWithEditors(object model, FormComponentContext context)
        {
            return GetEditablePropertiesWithEditorsCore(model, context);
        }


        /// <summary>
        /// Returns collection of <see cref="PropertyInfo"/>s of the <paramref name="model"/> annotated with <see cref="EditingComponentAttribute"/>
        /// </summary>
        private IEnumerable<PropertyInfo> GetEditablePropertiesCore(object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var editableProperties = model.GetAnnotatedProperties<EditingComponentAttribute>(false)
                                          .Where(x => x.GetCustomAttributes<CheckLicenseAttribute>(true).All(y => y.HasValidLicense())).ToList();

            var duplicatelyNamedProperty = editableProperties
                                                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                                                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatelyNamedProperty != null)
            {
                throw new InvalidOperationException($"Instance of type '{model.GetType().ToString()}' cannot contain property with name '{duplicatelyNamedProperty.Key}' as it already exists with different casing.");
            }

            return OrderProperties(editableProperties);
        }


        /// <summary>
        /// Returns collection of <see cref="FormComponent"/>s that enables editing of <paramref name="model"/> annotated with <see cref="EditingComponentAttribute"/>
        /// in Form builder's UI. Values from the <paramref name="model"/> are bound to components that display them.
        /// </summary>
        private IEnumerable<KeyValuePair<PropertyInfo, FormComponent>> GetEditablePropertiesWithEditorsCore(object model, FormComponentContext context)
        {
            var editableProperties = GetEditablePropertiesCore(model);
            var processedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            FormComponent formComponentPropertyModel;
            foreach (var property in editableProperties)
            {
                formComponentPropertyModel = CreateFormComponentForProperty(model, property, context);

                if (!processedProperties.Add(property.Name))
                {
                    throw new InvalidOperationException($"Instance of type '{model.GetType().ToString()}' cannot contain property with name '{property.Name}' as it already exists with different casing.");
                }

                yield return new KeyValuePair<PropertyInfo, FormComponent>(property, formComponentPropertyModel);
            }
        }


        private FormComponent CreateFormComponentForProperty(object model, PropertyInfo property, FormComponentContext context)
        {
            var attribute = property.GetCustomAttributes<EditingComponentAttribute>(false).FirstOrDefault();

            // Get definition of form component responsible for editing the property
            var formComponentPropertyDefinition = mFormComponentDefinitionProvider.Get(attribute.FormComponentIdentifier);
            if (formComponentPropertyDefinition == null)
            {
                throw new InvalidOperationException($"Property '{property.Name}' in model '{model.GetType()}' requires editing component with identifier '{attribute.FormComponentIdentifier}' which is not registered.");
            }

            FormComponentProperties formComponentPropertyModelProperties = mFormComponentActivator.CreateDefaultProperties(formComponentPropertyDefinition);
            mEditingComponentConfigurator.ConfigureFormComponentProperties(property, formComponentPropertyModelProperties);

            // Create form component responsible for editing the property
            FormComponent formComponentPropertyModel = mFormComponentActivator.CreateFormComponent(formComponentPropertyDefinition, formComponentPropertyModelProperties, context);
            formComponentPropertyModel.Name = property.Name;
            formComponentPropertyModel.ModelProperty = property;
            var value = property.GetValue(model, null);
            try
            {
                formComponentPropertyModel.SetObjectValue(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Preparing editing component '{attribute.FormComponentIdentifier}' for property '{property.Name}' in model '{model.GetType()}' failed. Could not set value '{value}' to editing component instance of type '{formComponentPropertyDefinition.ComponentType}'.", ex);
            }

            return formComponentPropertyModel;
        }


        /// <summary>
        /// Returns collection of sorted <see cref="PropertyInfo"/>s from the <paramref name="properties"/> collection.
        /// Properties are sorted by ascending <see cref="EditingComponentAttribute.Order"/> value, and in case of ambiguities
        /// alphabetical sort by <see cref="MemberInfo.Name"/> is applied.
        /// </summary>
        internal virtual IEnumerable<PropertyInfo> OrderProperties(IEnumerable<PropertyInfo> properties)
        {
            return properties.OrderBy(p =>
            {
                var order = p.GetCustomAttribute<EditingComponentAttribute>(false)?.Order ?? DEFAULT_ORDER_VALUE;

                return order;
            })
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
