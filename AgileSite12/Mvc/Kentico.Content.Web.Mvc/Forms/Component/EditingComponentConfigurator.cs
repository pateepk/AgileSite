using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates logic for configuring <see cref="FormComponentProperties"/> of a <see cref="FormComponent{TProperties, TValue}"/> using attributes.
    /// </summary>
    /// <seealso cref="EditingComponentAttribute"/>
    /// <seealso cref="EditingComponentPropertyAttribute"/>
    internal class EditingComponentConfigurator : IEditingComponentConfigurator
    {
        /// <summary>
        /// Contains property names and <see cref="EditingComponentAttribute"/> value getters to be processed when configuring <see cref="FormComponentProperties"/>.
        /// </summary>
        private readonly NameAndValueGetter<Func<EditingComponentAttribute, object>>[] ProcessedEditingComponentAttributeProperties = new NameAndValueGetter<Func<EditingComponentAttribute, object>>[]
        {
            new NameAndValueGetter<Func<EditingComponentAttribute, object>>(nameof(FormComponentProperties.Label), att => att.Label, true),
            new NameAndValueGetter<Func<EditingComponentAttribute, object>>(nameof(FormComponentProperties<object>.DefaultValue), att => att.DefaultValue, false),
            new NameAndValueGetter<Func<EditingComponentAttribute, object>>(nameof(FormComponentProperties.ExplanationText), att => att.ExplanationText, true),
            new NameAndValueGetter<Func<EditingComponentAttribute, object>>(nameof(FormComponentProperties.Tooltip), att => att.Tooltip, true),
        };


        private struct NameAndValueGetter<TValueGetter>
        {
            public string Name { get; set; }


            public TValueGetter ValueGetter { get; set; }


            public bool LocalizeValue { get; set; }


            public NameAndValueGetter(string name, TValueGetter valueGetter, bool localizeValue)
            {
                Name = name;
                ValueGetter = valueGetter;
                LocalizeValue = localizeValue;
            }
        }


        /// <summary>
        /// Configures given <paramref name="formComponentProperties"/> according to <paramref name="propertyInfo"/>'s attributes. The <see cref="EditingComponentAttribute"/>
        /// and <see cref="EditingComponentPropertyAttribute"/> classes are supported.
        /// </summary>
        /// <param name="propertyInfo">
        /// <see cref="PropertyInfo"/> annotated with <see cref="EditingComponentPropertyAttribute"/>s for configuring
        /// <see cref="FormComponent{TProperties, TValue}"/> that handles editing of <paramref name="propertyInfo"/> in Form builder UI.
        /// </param>
        /// <param name="formComponentProperties">
        /// Properties of the <see cref="FormComponent{TProperties, TValue}"/> used for editing <paramref name="propertyInfo"/> in Form builder UI.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> or <paramref name="formComponentProperties"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an <see cref="EditingComponentPropertyAttribute"/> of <paramref name="propertyInfo"/> specifies a property which does not exist within <paramref name="formComponentProperties"/>.</exception>
        public void ConfigureFormComponentProperties(PropertyInfo propertyInfo, FormComponentProperties formComponentProperties)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (formComponentProperties == null)
            {
                throw new ArgumentNullException(nameof(formComponentProperties));
            }

            var boundProperties = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

            BindEditingComponentAttributeToProperties(propertyInfo, formComponentProperties, boundProperties);
            BindEditingComponentPropertyAttributesToProperties(propertyInfo, formComponentProperties, boundProperties);

            // Use the property name for the Label property, in case the Label property was configured by any other attribute
            if (!boundProperties.Contains(nameof(FormComponentProperties.Label)))
            {
                SetValueForPropertyOfFormComponentProperties(formComponentProperties, nameof(formComponentProperties.Label), propertyInfo.Name);
            }
        }


        /// <summary>
        /// Sets <paramref name="value"/> for member with <paramref name="propertyName"/> in instance <paramref name="formComponentProperties"/>.
        /// </summary>
        private void SetValueForPropertyOfFormComponentProperties(FormComponentProperties formComponentProperties, String propertyName, object value)
        {
            var property = formComponentProperties.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null)
            {
                throw new InvalidOperationException($"Form component properties of type '{formComponentProperties.GetType().ToString()}' has no property named '{propertyName}'.");
            }

            try
            {
                property.SetValue(formComponentProperties, value, null);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException($"Configuring property '{property.Name}' of class '{formComponentProperties.GetType().ToString()}' with value '{value}' is invalid due to incompatible types.", e);
            }
        }


        private void BindEditingComponentAttributeToProperties(PropertyInfo property, FormComponentProperties formComponentProperties, HashSet<string> boundProperties)
        {
            var editingComponentAttribute = property.GetCustomAttributes<EditingComponentAttribute>(false).FirstOrDefault();
            if (editingComponentAttribute == null)
            {
                return;
            }

            foreach(var processedProperty in ProcessedEditingComponentAttributeProperties.Select(p => new { Name = p.Name, Value = p.ValueGetter(editingComponentAttribute), LocalizeValue = p.LocalizeValue }).Where(p => p.Value != null))
            {
                var value = (processedProperty.LocalizeValue && processedProperty.Value is string localizablePropertyValue) ? ResHelper.LocalizeString(localizablePropertyValue) : processedProperty.Value;

                SetValueForPropertyOfFormComponentProperties(formComponentProperties, processedProperty.Name, value);
                boundProperties.Add(processedProperty.Name);
            }
        }


        private void BindEditingComponentPropertyAttributesToProperties(PropertyInfo property, FormComponentProperties formComponentProperties, HashSet<string> boundProperties)
        {
            foreach (var attribute in property.GetCustomAttributes<EditingComponentPropertyAttribute>(false))
            {
                SetValueForPropertyOfFormComponentProperties(formComponentProperties, attribute.PropertyName, attribute.GetPropertyValue());
                boundProperties.Add(attribute.PropertyName);
            }
        }
    }
}
