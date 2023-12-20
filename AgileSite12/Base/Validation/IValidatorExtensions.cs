using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Provides a set of extension methods for <see cref="IValidator"/>.
    /// </summary>
    public static class IValidatorExtensions
    {
        /// <summary>
        /// Returns a value indicating whether a validation error of type <typeparamref name="TValidatorError"/> is in <see cref="IValidator.Errors"/>.
        /// </summary>
        /// <typeparam name="TValidatorError">Validation error type to search for.</typeparam>
        /// <param name="validator">Validator whose errors to examine.</param>
        /// <returns>Returns true if a validation error of type <typeparamref name="TValidatorError"/> is in <see cref="IValidator.Errors"/>, otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
        public static bool Contains<TValidatorError>(this IValidator validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return validator.Errors.Any(error => error.GetType() == typeof(TValidatorError));
        }


        /// <summary>
        /// Returns <see cref="Type"/> of the first validation error in <see cref="IValidator.Errors"/>.
        /// </summary>
        /// <param name="validator">Validator whose first error type to retrieve.</param>
        /// <returns>Returns <see cref="Type"/> of the first validation error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="IValidator.Errors"/> property of <paramref name="validator"/> is an empty enumeration.</exception>
        public static Type FirstErrorType(this IValidator validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return validator.Errors.First().GetType();
        }


        /// <summary>
        /// Returns collection of localized error messages that occurred during validation.
        /// </summary>
        /// <param name="validator">Validator for which to get error message.</param>
        /// <param name="culture">Culture to use for localization.</param>
        /// <remarks>
        /// The default culture is used for messages which are missing localization in specified <paramref name="culture"/>.
        /// </remarks>
        /// <returns>Collection containing error messages of the validator.</returns>
        public static IEnumerable<string> GetErrorMessages(this IValidator validator, string culture = null)
        {
            return validator.Errors.Select(e => e.GetMessage(culture));
        }
    }
}
