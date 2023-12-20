using System;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Declares members of a validator.
    /// </summary>
    /// <remarks>
    /// Validators represent a reusable validation logic.
    /// </remarks>
    public interface IValidator
    {
        /// <summary>
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned
        /// if validation succeeded.
        /// </summary>
        /// <remarks>
        /// Accessing this property must be preceded by a call to the <see cref="Validate"/> method.
        /// </remarks>
        IEnumerable<IValidationError> Errors
        {
            get;
        }


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <remarks>
        /// Accessing this property must be preceded by a call to the <see cref="Validate"/> method.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        bool IsValid
        {
            get;
        }


        /// <summary>
        /// Performs validation on this validator. The validation result is returned and can be later retrieved via <see cref="IsValid"/> property.
        /// When validation fails, an enumeration of errors associated with the validation is available in <see cref="Errors"/>.
        /// </summary>
        /// <returns>Returns a value indicating whether validation succeeded.</returns>
        bool Validate();

    }
}
