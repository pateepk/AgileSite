using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormVisibilityCondition(AnotherBoolFieldVisibilityCondition.IDENTIFIER, typeof(AnotherBoolFieldVisibilityCondition), "Value of another field is")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Form component visibility condition specifying dependency on another field of boolean type whose value is equal to the <see cref="ExpectedValue"/>.
    /// </summary>
    /// <seealso cref="VisibilityConditionEditingComponentOrder"/>
    public class AnotherBoolFieldVisibilityCondition : AnotherFieldVisibilityCondition<bool?>, IDefaultVisibilityCondition
    {
        /// <summary>
        /// Represents the <see cref="AnotherBoolFieldVisibilityCondition"/> identifier.
        /// </summary>
        internal const string IDENTIFIER = "Kentico.AnotherBoolField";


        /// <summary>
        /// Expected value of the boolean field.
        /// </summary>
        [EditingComponent(BoolComparisonTypeSelectorComponent.IDENTIFIER, Label = "", Order = VisibilityConditionEditingComponentOrder.COMPARISON_TYPE)]
        public BoolFieldValueTypes ExpectedValue { get; set; }


        /// <summary>
        /// Gets a value indicating whether a form component is visible.
        /// </summary>
        /// <returns>Returns true if <see cref="AnotherFieldVisibilityCondition{TValue}.DependeeFieldValue"/> is equal to the expected value.</returns>
        public override bool IsVisible()
        {
            if (DependeeFieldValue == null)
            {
                return false;
            }

            switch (ExpectedValue)
            {
                case BoolFieldValueTypes.IsFalse:
                    return !DependeeFieldValue.Value;
                case BoolFieldValueTypes.IsTrue:
                    return DependeeFieldValue.Value;
                default:
                    throw new NotSupportedException($"Boolean value type '{nameof(ExpectedValue)}' is not supported.");
            }

        }
    }
}
