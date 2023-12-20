using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(MaximumLengthValidationRule.IDENTIFIER, typeof(MaximumLengthValidationRule), "{$kentico.formbuilder.validationrule.maximumlength.name$}", Description = "{$kentico.formbuilder.validationrule.maximumlength.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule validates that user input value is not longer than specified value.
    /// </summary>
    [Serializable]
    public class MaximumLengthValidationRule : ValidationRule<string>
    {
        /// <summary>
        /// Represents the <see cref="MaximumLengthValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.MaximumLength";


        /// <summary>
        /// Maximum length user input is validated against.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.maximumlength.maximumlength.label$}")]
        [Required]
        public int MaximumLength { get; set; } = 100;


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Maximum length is 100'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.maximumlength.title", MaximumLength);
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if it is shorter or equal to the <see cref="MaximumLength"/>.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected override bool Validate(string value)
        {
            return (value?.Length ?? 0) <= MaximumLength;
        }
    }
}
