using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(RegularExpressionValidationRule.IDENTIFIER, typeof(RegularExpressionValidationRule), "{$kentico.formbuilder.validationrule.regularexpression.name$}", Description = "{$kentico.formbuilder.validationrule.regularexpression.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents regular expression validation rule.
    /// </summary>
    [Serializable]
    public class RegularExpressionValidationRule : ValidationRule<string>
    {
        /// <summary>
        /// Represents the <see cref="RegularExpressionValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.RegularExpression";


        /// <summary>
        /// Gets or sets the regular expression used to validate form component's value.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.regularexpression.regularexpression.label$}")]
        [Required]
        public string RegularExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Regular expression matches [a-Z+^]'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.regularexpression.title", RegularExpression);
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if the value is valid.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected override bool Validate(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return RegexHelper.GetRegex(RegularExpression).IsMatch(value);
        }
    }
}