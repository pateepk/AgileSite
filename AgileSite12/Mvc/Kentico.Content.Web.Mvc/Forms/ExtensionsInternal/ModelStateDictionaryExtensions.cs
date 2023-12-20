using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains helper methods for working with <see cref="ModelStateDictionary"/>.
    /// </summary>
    internal static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds validation errors originated in Form builder into the model state.
        /// </summary>
        /// <param name="modelState">Model state to add errors into.</param>
        /// <param name="prefixedComponentName">Prefix of the form component.</param>
        /// <param name="validationErrors">Validation errors to add.</param>
        public static void AddErrorMessages(this ModelStateDictionary modelState, string prefixedComponentName, IEnumerable<ValidationResult> validationErrors)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }
            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            foreach (var error in validationErrors)
            {
                if (error.MemberNames.Any())
                {
                    foreach (var memberName in error.MemberNames)
                    {
                        modelState.AddModelError($"{prefixedComponentName}.{memberName}", error.ErrorMessage);
                    }
                }
                else
                {
                    modelState.AddModelError(prefixedComponentName, error.ErrorMessage);
                }
            }
        }
    }
}
