using System;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(BoolIsSetValidationRule.IDENTIFIER, typeof(BoolIsSetValidationRule), "{$kentico.formbuilder.validationrule.boolisset.name$}", Description = "{$kentico.formbuilder.validationrule.boolisset.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule used to validate that a boolean type component's value (e.g. checkbox) is <c>true</c>.
    /// </summary>
    [Serializable]
    public class BoolIsSetValidationRule : ValidationRule<bool>
    {
        /// <summary>
        /// Represents the <see cref="BoolIsSetValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.BoolIsSet";


        /// <summary>
        /// Gets a title for this instance of boolean validation rule.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetString("kentico.formbuilder.validationrule.boolisset.title");
        }

        /// <summary>
        /// Returns true if given value is <c>true</c>.
        /// </summary>
        /// <param name="value">Form component's value.</param>
        protected override bool Validate(bool value)
        {
            return value;
        }
    }
}
