using System;
using System.Linq;
using System.Reflection;

using CMS.DataEngine;
using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains methods for creating form components and their properties.
    /// </summary>
    internal class FormComponentActivator : IFormComponentActivator
    {
        /// <summary>
        /// Creates a new instance of <see cref="FormComponentActivator"/>.
        /// </summary>
        public FormComponentActivator()
        {
        }


        /// <summary>
        /// Creates a new instance of the form component specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a component instance.</param>
        /// <param name="context">Contextual information specifying where the form component is being used.</param>
        /// <returns>Returns an instance of form component as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public FormComponent CreateFormComponent(FormComponentDefinition definition, FormComponentContext context)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var defaultProperties = CreateDefaultProperties(definition);
            FormComponent formComponent = CreateFormComponent(definition, defaultProperties, context);

            return formComponent;
        }


        /// <summary>
        /// Creates a new instance of the form component specified by its definition using the properties given.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a component instance.</param>
        /// <param name="properties">Properties to be loaded into the component.</param>
        /// <param name="context">Contextual information specifying where the form component is being used.</param>
        /// <returns>Returns an instance of form component as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> or <paramref name="properties"/> is null.</exception>
        public FormComponent CreateFormComponent(FormComponentDefinition definition, FormComponentProperties properties, FormComponentContext context)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (definition.ComponentType.GetCustomAttributes<CheckLicenseAttribute>(true).Any(x => !x.HasValidLicense()))
            {
                throw new LicenseException($"You don't have sufficient license to use the {definition.ComponentType}.");
            }

            FormComponent formComponent = (FormComponent)Activator.CreateInstance(definition.ComponentType);
            formComponent.BindContext(context);
            formComponent.Definition = definition;
            formComponent.LoadProperties(properties);

            return formComponent;
        }


        /// <summary>
        /// Creates a new instance of the form component default properties.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a default properties instance.</param>
        /// <returns>Returns an instance of form component properties as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public FormComponentProperties CreateDefaultProperties(FormComponentDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var properties = (FormComponentProperties)Activator.CreateInstance(definition.FormComponentPropertiesType);

            if (properties.Precision == -1)
            {
                properties.Precision = DataTypeManager.GetDataType(TypeEnum.Field, properties.DataType)?.DefaultPrecision ?? -1;
            }

            if (String.IsNullOrEmpty(properties.Label))
            {
                properties.Label = ResHelper.LocalizeString(definition.Name);
            }

            EditingComponentUtils.SetDefaultPropertiesValues(properties);

            return properties;
        }
    }
}
