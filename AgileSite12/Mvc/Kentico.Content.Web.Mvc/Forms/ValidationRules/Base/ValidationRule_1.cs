using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for a form component validation rule.
    /// Inherit this class to create custom validation rules.
    /// </summary>
    /// <typeparam name="TValue">Type of value which is supposed to be validated by this rule</typeparam>
    /// <seealso cref="ValidationRuleEditingComponentOrder"/>
    [Serializable]
    public abstract class ValidationRule<TValue> : ValidationRule
    {
        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if the value is valid.
        /// This method is used by system.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        public sealed override bool IsValueValid(object value)
        {
            return Validate((TValue)value);
        }


        /// <summary>
        /// Wrapped by <see cref="IsValueValid(object)"/> which converts the value to type specified by the generic type.
        /// Override this method to apply desired validation.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        protected abstract bool Validate(TValue value);
    }
}
