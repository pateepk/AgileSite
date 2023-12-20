using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(MinimumLengthValidationRule.IDENTIFIER, typeof(MinimumLengthValidationRule), "{$kentico.formbuilder.validationrule.minimumlength.name$}", Description = "{$kentico.formbuilder.validationrule.minimumlength.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule validates that user input value is not shorther than specified value.
    /// </summary>
    [Serializable]
    public class MinimumLengthValidationRule : ValidationRule<string>
    {
        /// <summary>
        /// Represents the <see cref="MinimumLengthValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.MinimumLength";


        /// <summary>
        /// Minimum length user input is validated against.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.minimumlength.minimumlength.label$}")]
        [Required]
        public int MinimumLength { get; set; } = 1;


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Minimum length is 100'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.minimumlength.title", MinimumLength);
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if it is longer or equal to the <see cref="MinimumLength"/>.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected override bool Validate(string value)
        {
            return (value?.Length ?? 0) >= MinimumLength;
        }
    }
}
