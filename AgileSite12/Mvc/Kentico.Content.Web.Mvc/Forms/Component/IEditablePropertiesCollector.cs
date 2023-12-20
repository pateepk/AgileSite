using System;
using System.Collections.Generic;
using System.Reflection;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IEditablePropertiesCollector), typeof(EditablePropertiesCollector), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for collecting properties editable via <see cref="FormComponent{TProperties, TValue}"/>s from a model.
    /// </summary>
    /// <seealso cref="FormComponentProperties"/>
    /// <seealso cref="FormComponent{TProperties, TValue}"/>
    public interface IEditablePropertiesCollector
    {
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
        IEnumerable<FormComponent> GetFormComponents(object model, FormComponentContext context);


        /// <summary>
        /// Returns a collection of <see cref="PropertyInfo"/>s editable in Form builder's UI.
        /// </summary>
        /// <param name="model">Object with editable properties.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        IEnumerable<PropertyInfo> GetEditableProperties(object model);


        /// <summary>
        /// Returns key value pair collection of <see cref="FormComponent"/>s paired with the <see cref="PropertyInfo"/>
        /// that is edited via given <see cref="FormComponent"/> in Form builder's UI.
        /// </summary>
        /// <param name="model">Object with editable properties.</param>
        /// <param name="context">Contextual information specifying where the form components are being used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        IEnumerable<KeyValuePair<PropertyInfo, FormComponent>> GetEditablePropertiesWithEditors(object model, FormComponentContext context);
    }
}