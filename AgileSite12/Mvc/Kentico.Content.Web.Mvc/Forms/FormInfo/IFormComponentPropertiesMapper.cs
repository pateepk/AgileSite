using System;

using CMS;
using CMS.Core;
using CMS.FormEngine;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentPropertiesMapper), typeof(FormComponentPropertiesMapper), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains mapping methods for conversions between form component properties and form field definition.
    /// </summary>
    /// <seealso cref="FormComponentProperties"/>
    /// <seealso cref="FormFieldInfo"/>
    public interface IFormComponentPropertiesMapper
    {
        /// <summary>
        /// Maps an instance of <see cref="FormFieldInfo"/> to corresponding <see cref="FormComponentProperties"/>.
        /// To extract identifier of corresponding <see cref="FormComponentDefinition"/> use <see cref="GetComponentIdentifier"/>.
        /// </summary>
        /// <param name="formFieldInfo">Form field to be mapped.</param>
        /// <returns>Returns an instance of <see cref="FormComponentProperties"/> which corresponds to given form field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFieldInfo"/> is null.</exception>
        FormComponentProperties FromFieldInfo(FormFieldInfo formFieldInfo);


        /// <summary>
        /// Maps an instance of <see cref="FormComponentProperties"/> to corresponding <see cref="FormFieldInfo"/>.
        /// </summary>
        /// <param name="formComponentProperties">Form component properties to be mapped.</param>
        /// <param name="componentIdentifier">Identifier of corresponding <see cref="FormComponentDefinition"/> to be stored along with <paramref name="formComponentProperties"/>.</param>
        /// <returns>Returns an instance of <see cref="FormFieldInfo"/> which corresponds to given properties and type identifier.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formComponentProperties"/> is null.</exception>
        FormFieldInfo ToFormFieldInfo(FormComponentProperties formComponentProperties, string componentIdentifier);


        /// <summary>
        /// Gets <see cref="FormComponentDefinition"/> identifier stored in <paramref name="formFieldInfo"/>.
        /// </summary>
        /// <param name="formFieldInfo">Form field info for which to obtain <see cref="FormComponentDefinition"/>'s identifier.</param>
        /// <returns>Return identifier obtained from <paramref name="formFieldInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFieldInfo"/> is null.</exception>
        string GetComponentIdentifier(FormFieldInfo formFieldInfo);
    }
}