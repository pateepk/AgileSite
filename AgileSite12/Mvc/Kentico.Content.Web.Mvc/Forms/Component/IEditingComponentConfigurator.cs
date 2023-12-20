using System;
using System.Reflection;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IEditingComponentConfigurator), typeof(EditingComponentConfigurator), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates logic for configuring <see cref="FormComponentProperties"/> of a <see cref="FormComponent{TProperties, TValue}"/> using attributes.
    /// </summary>
    /// <seealso cref="EditingComponentAttribute"/>
    /// <seealso cref="EditingComponentPropertyAttribute"/>
    public interface IEditingComponentConfigurator
    {
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
        void ConfigureFormComponentProperties(PropertyInfo propertyInfo, FormComponentProperties formComponentProperties);
    }
}
