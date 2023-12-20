using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormVisibilityCondition(AnotherStringFieldVisibilityCondition.IDENTIFIER, typeof(AnotherStringFieldVisibilityCondition), "Value of another field is")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Form component visibility condition specifying dependency on another field of string type whose value satisfies condition based on <see cref="ComparisonType"/> and <see cref="CompareToValue"/>.
    /// </summary>
    /// <seealso cref="VisibilityConditionEditingComponentOrder"/>
    [Serializable]
    public class AnotherStringFieldVisibilityCondition : AnotherFieldVisibilityCondition<string>, IDefaultVisibilityCondition
    {
        /// <summary>
        /// Represents the <see cref="AnotherStringFieldVisibilityCondition"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.AnotherStringField";


        /// <summary>
        /// Type of comparison to perform.
        /// </summary>
        [EditingComponent(StringComparisonTypeSelectorComponent.IDENTIFIER, Label = "", Order = VisibilityConditionEditingComponentOrder.COMPARISON_TYPE)]
        public StringFieldComparisonTypes ComparisonType { get; set; }


        /// <summary>
        /// Value to compare against.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "", Order = VisibilityConditionEditingComponentOrder.COMPARE_TO_VALUE)]
        public string CompareToValue { get; set; }


        /// <summary>
        /// Gets a value indicating whether a form component is visible.
        /// </summary>
        /// <returns>Returns true if <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldValue"/> and <see cref="CompareToValue"/> satisfy specified <see cref="ComparisonType"/>.</returns>
        public override bool IsVisible()
        {
            return ComparisonUtils.Compare(DependeeFieldValue, ComparisonType, CompareToValue, StringComparison.InvariantCulture);
        }
    }
}
