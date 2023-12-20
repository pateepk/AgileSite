using System;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(EmailValidationRule.IDENTIFIER, typeof(EmailValidationRule), "{$kentico.formbuilder.validationrule.email.name$}", Description = "{$kentico.formbuilder.validationrule.email.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule used to validate email addresses.
    /// </summary>
    [Serializable]
    public class EmailValidationRule : ValidationRule<string>
    {
        /// <summary>
        /// Represents the <see cref="EmailValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.Email";


        /// <summary>
        /// Gets a title for this instance of email validation rule.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetString("kentico.formbuilder.validationrule.email.title");
        }

        /// <summary>
        /// Returns true if given value is a valid email address.
        /// </summary>
        /// <param name="value">Form component's value.</param>
        /// <returns>True if email is valid, otherwise false.</returns>
        protected override bool Validate(string value)
        {
            return ValidationHelper.IsEmail(value);
        }
    }
}
