using System;

using CMS.Helpers;
using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormValidationRule(StringCompareToFieldValidationRule.IDENTIFIER, typeof(StringCompareToFieldValidationRule), "{$kentico.formbuilder.validationrule.comparetofield.name$}", Description = "{$kentico.formbuilder.validationrule.comparetofield.description$}")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Rule is used to compare two fields of string value types.
    /// </summary>
    [Serializable]
    public class StringCompareToFieldValidationRule : CompareToFieldValidationRule<string>
    {
        /// <summary>
        /// Represents the <see cref="StringCompareToFieldValidationRule"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.StringCompareToField";


        /// <summary>
        /// Determines which type of comparison is used.
        /// </summary>
        [EditingComponent(StringComparisonTypeSelectorComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.comparetofield.comparisontype.label$}")]
        public StringFieldComparisonTypes ComparisonType { get; set; }


        /// <summary>
        /// Gets a title for this instance of validation rule.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        public override string GetTitle()
        {
            return ResHelper.GetString($"kentico.formbuilder.validationrule.comparetofield.{ComparisonType}.title");
        }


        /// <summary>
        /// Validates that the values are in specified relationship.
        /// </summary>
        /// <param name="value">Value of the field validated against.</param>
        /// <returns>True if valid, otherwise false.</returns>
        protected override bool Validate(string value)
        {
            return ComparisonUtils.Compare(value, ComparisonType, DependeeFieldValue, StringComparison.InvariantCulture);
        }
    }
}
