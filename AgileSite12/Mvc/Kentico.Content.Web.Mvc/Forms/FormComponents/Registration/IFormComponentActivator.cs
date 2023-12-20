using System;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentActivator), typeof(FormComponentActivator), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for creating form components and their properties.
    /// </summary>
    public interface IFormComponentActivator
    {
        /// <summary>
        /// Creates a new instance of the form component specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a component instance.</param>
        /// <param name="context">Contextual information specifying where the form component is being used.</param>
        /// <returns>Returns an instance of form component as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        FormComponent CreateFormComponent(FormComponentDefinition definition, FormComponentContext context);


        /// <summary>
        /// Creates a new instance of the form component specified by its definition using the properties given.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a component instance.</param>
        /// <param name="properties">Properties to be loaded into the component.</param>
        /// <param name="context">Contextual information specifying where the form component is being used.</param>
        /// <returns>Returns an instance of form component as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> or <paramref name="properties"/> is null.</exception>
        FormComponent CreateFormComponent(FormComponentDefinition definition, FormComponentProperties properties, FormComponentContext context);


        /// <summary>
        /// Creates a new instance of the form component's properties specified by component's definition.
        /// </summary>
        /// <param name="definition">Form component definition for which to create a default properties instance.</param>
        /// <returns>Returns an instance of form component properties as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        FormComponentProperties CreateDefaultProperties(FormComponentDefinition definition);
    }
}
