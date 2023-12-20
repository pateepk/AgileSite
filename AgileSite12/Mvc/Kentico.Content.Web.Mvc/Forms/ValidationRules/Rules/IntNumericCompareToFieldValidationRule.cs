using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(IntNumericCompareToFieldValidationRule.IDENTIFIER, typeof(IntNumericCompareToFieldValidationRule), "{$kentico.formbuilder.validationrule.comparetofield.name$}", Description = "{$kentico.formbuilder.validationrule.comparetofield.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule is used to compare two fields of integer value types.
    /// </summary>
    [Serializable]
    public class IntNumericCompareToFieldValidationRule : NumericCompareToFieldValidationRule<int>
    {
        /// <summary>
        /// Represents the <see cref="IntNumericCompareToFieldValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.IntCompareToField";
    }
}
