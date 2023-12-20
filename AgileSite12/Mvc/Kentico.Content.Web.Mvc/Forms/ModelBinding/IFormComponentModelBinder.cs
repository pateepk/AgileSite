using System;
using System.Web.Mvc;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentModelBinder), typeof(FormComponentModelBinder), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for binding form data to <see cref="FormComponent"/>.
    /// </summary>
    public interface IFormComponentModelBinder
    {
        /// <summary>
        /// Binds all properties of specified component.
        /// Also validates resulting component value.
        /// </summary>
        /// <param name="controllerContext">Controller context of the current request.</param>
        /// <param name="formComponent">Component to be bound.</param>
        /// <param name="formCollection">Source of data for the binding.</param>
        /// <param name="nameHtmlFieldPrefix">Prefix used in the name attribute of the inputs belonging to <paramref name="formComponent"/> to identify values in form collection.</param>
        /// <param name="shouldValidationContinue">
        /// Indicates whether validation should continue after component is bound.
        /// Validation won't continue in case binding of any one of the partial values fails.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formComponent"/> or <paramref name="formCollection"/> is null.</exception>
        /// <returns>Enumeration of validation errors, empty if validation succeeded.</returns>
        void BindComponent(ControllerContext controllerContext, FormComponent formComponent, FormCollection formCollection, string nameHtmlFieldPrefix, out bool shouldValidationContinue);
    }
}
