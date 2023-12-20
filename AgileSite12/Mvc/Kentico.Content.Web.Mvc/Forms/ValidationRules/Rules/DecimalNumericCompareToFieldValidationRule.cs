using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(DecimalNumericCompareToFieldValidationRule.IDENTIFIER, typeof(DecimalNumericCompareToFieldValidationRule), "{$kentico.formbuilder.validationrule.comparetofield.name$}", Description = "{$kentico.formbuilder.validationrule.comparetofield.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule is used to compare two fields of decimal value types.
    /// </summary>
    [Serializable]
    public class DecimalNumericCompareToFieldValidationRule : NumericCompareToFieldValidationRule<decimal>
    {
        /// <summary>
        /// Represents the <see cref="DecimalNumericCompareToFieldValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.DecimalCompareToField";
    }
}
