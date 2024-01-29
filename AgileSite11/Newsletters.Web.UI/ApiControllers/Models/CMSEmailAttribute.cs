using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Validates email <see cref="ValidationHelper.IsEmail"/>.
    /// </summary>
    internal class CMSEmailAttribute : ValidationAttribute
    {
        /// <summary>
        /// Checks if object is valid.
        /// </summary>
        /// <param name="value">Object to validate.</param>
        /// <param name="validationContext">Validation context.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationHelper.IsEmail(value) ? 
                ValidationResult.Success :  
                new ValidationResult(ResHelper.GetString("campaign.create.email.campaign.senderaddress.invalid.email"));
        }
    }
}