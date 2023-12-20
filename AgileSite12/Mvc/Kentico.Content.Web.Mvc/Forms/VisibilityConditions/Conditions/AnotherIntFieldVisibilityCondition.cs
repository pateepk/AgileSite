using System;
using System.ComponentModel.DataAnnotations;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormVisibilityCondition(AnotherIntFieldVisibilityCondition.IDENTIFIER, typeof(AnotherIntFieldVisibilityCondition), "Value of another field is")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Form component visibility condition specifying dependency on another field of integer type whose value satisfies condition based on <see cref="ComparisonType"/> and <see cref="CompareToValue"/>.
    /// </summary>
    /// <seealso cref="VisibilityConditionEditingComponentOrder"/>
    [Serializable]
    public class AnotherIntFieldVisibilityCondition : AnotherFieldVisibilityCondition<int?>, IDefaultVisibilityCondition
    {
        /// <summary>
        /// Represents the <see cref="AnotherIntFieldVisibilityCondition"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.AnotherIntField";


        /// <summary>
        /// Type of comparison to perform.
        /// </summary>
        [EditingComponent(NumericComparisonTypeSelectorComponent.IDENTIFIER, Label = "", Order = VisibilityConditionEditingComponentOrder.COMPARISON_TYPE)]
        public NumericFieldComparisonTypes ComparisonType { get; set; }


        /// <summary>
        /// Value to compare against.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "", Order = VisibilityConditionEditingComponentOrder.COMPARE_TO_VALUE)]
        [Required]
        public int CompareToValue { get; set; }


        /// <summary>
        /// Gets a value indicating whether a form component is visible.
        /// </summary>
        /// <returns>
        /// Returns true if <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldValue"/> and <see cref="CompareToValue"/> satisfy specified <see cref="ComparisonType"/>.
        /// If <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldValue"/> is null, returns false.
        /// </returns>
        public override bool IsVisible()
        {
            if (DependeeFieldValue == null)
            {
                return false;
            }

            return ComparisonUtils.Compare(DependeeFieldValue.Value, ComparisonType, CompareToValue);
        }
    }
}
