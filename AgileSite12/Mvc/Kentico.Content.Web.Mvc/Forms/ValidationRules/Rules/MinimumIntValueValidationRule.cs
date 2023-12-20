using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(MinimumIntValueValidationRule.IDENTIFIER, typeof(MinimumIntValueValidationRule), "{$kentico.formbuilder.validationrule.minimumvalue.name$}", Description = "{$kentico.formbuilder.validationrule.minimumvalue.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule validates that user input value is not lower than specified value.
    /// Variant for <see cref="int"/> value types.
    /// </summary>
    [Serializable]
    public class MinimumIntValueValidationRule : ValidationRule<int>
    {
        /// <summary>
        /// Represents the <see cref="MinimumIntValueValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.MinimumIntValue";


        /// <summary>
        /// Minimum value user input is validated against.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.minimumvalue.minimumvalue.label$}")]
        [Required]
        public int MinimumValue { get; set; } = 1;


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Minimum value is 100'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.minimumvalue.title", MinimumValue);
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if it is greater or equal to the <see cref="MinimumValue"/>.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected override bool Validate(int value)
        {
            return MinimumValue <= value;
        }
    }
}
