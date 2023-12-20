using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(MaximumIntValueValidationRule.IDENTIFIER, typeof(MaximumIntValueValidationRule), "{$kentico.formbuilder.validationrule.maximumvalue.name$}", Description = "{$kentico.formbuilder.validationrule.maximumvalue.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule validates that user input value is not greater than specified value.
    /// Variant for <see cref="int"/> value types.
    /// </summary>
    [Serializable]
    public class MaximumIntValueValidationRule : ValidationRule<int>
    {
        /// <summary>
        /// Represents the <see cref="MaximumIntValueValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.MaximumIntValue";


        /// <summary>
        /// Maximum value user input is validated against.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.maximumvalue.maximumvalue.label$}")]
        [Required]
        public int MaximumValue { get; set; } = 100;


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Maximum value is 100'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.maximumvalue.title", MaximumValue);
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if it is less or equal to the <see cref="MaximumValue"/>.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected override bool Validate(int value)
        {
            return MaximumValue >= value;
        }
    }
}
