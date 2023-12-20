using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Ensures that current property cannot be set to true when the component is marked as required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class SmartFieldPropertyValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(validationContext.ObjectInstance is FormComponentProperties properties))
            {
                throw new NotSupportedException($"{nameof(SmartFieldPropertyValidationAttribute)} can only be used within FormComponentProperties and derived classes.");
            }

            if (value as bool? == true)
            {
                if (!SmartFieldLicenseHelper.HasLicense())
                {
                    var localizedError = ResHelper.GetString("kentico.formbuilder.validation.smartfield.featurenotavailable");
                    return new ValidationResult(localizedError, new[] { validationContext.MemberName });
                }

                var fieldIsRequired = properties.Required;

                if (fieldIsRequired)
                {
                    var localizedError = ResHelper.GetString("kentico.formbuilder.validation.smartfield.errorrequired");
                    return new ValidationResult(localizedError, new[] { validationContext.MemberName });
                }
            }

            return ValidationResult.Success;
        }
    }
}
