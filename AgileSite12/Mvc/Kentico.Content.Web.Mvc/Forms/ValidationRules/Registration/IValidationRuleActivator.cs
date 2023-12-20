using System;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IValidationRuleActivator), typeof(ValidationRuleActivator), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for creating <see cref="ValidationRule"/>s.
    /// </summary>
    public interface IValidationRuleActivator
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ValidationRule"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="validationRuleIdentifier">Identifies <see cref="ValidationRule"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="ValidationRule"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRuleIdentifier"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ValidationRule"/> with given <paramref name="validationRuleIdentifier"/> is not registered in the system.</exception>
        ValidationRule CreateValidationRule(string validationRuleIdentifier);


        /// <summary>
        /// Creates a new instance of the <see cref="ValidationRule"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Defines <see cref="ValidationRule"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="ValidationRule"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        ValidationRule CreateValidationRule(ValidationRuleDefinition definition);
    }
}
