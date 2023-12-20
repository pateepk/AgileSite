using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(DoubleNumericCompareToFieldValidationRule.IDENTIFIER, typeof(DoubleNumericCompareToFieldValidationRule), "{$kentico.formbuilder.validationrule.comparetofield.name$}", Description = "{$kentico.formbuilder.validationrule.comparetofield.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule is used to compare two fields of double value types.
    /// </summary>
    [Serializable]
    public class DoubleNumericCompareToFieldValidationRule : NumericCompareToFieldValidationRule<double>
    {
        /// <summary>
        /// Represents the <see cref="DoubleNumericCompareToFieldValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.DoubleCompareToField";
    }
}
