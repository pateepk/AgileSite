using System;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Shared validation rule implementation used to compare two fields of numeric value types.
    /// </summary>
    /// <typeparam name="TValue">Specific type of the compared numeric values.</typeparam>
    public abstract class NumericCompareToFieldValidationRule<TValue> : CompareToFieldValidationRule<TValue>
        where TValue : IComparable<TValue>
    {
        /// <summary>
        /// Determines which type of comparison is used.
        /// </summary>
        [EditingComponent(NumericComparisonTypeSelectorComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.comparetofield.comparisontype.label$}")]
        public NumericFieldComparisonTypes ComparisonType { get; set; }


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
        protected override bool Validate(TValue value)
        {
            return ComparisonUtils.Compare(value, ComparisonType, DependeeFieldValue);
        }
    }
}
