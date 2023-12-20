using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Definition of a registered form component validation rule.
    /// </summary>
    public class ValidationRuleDefinition : ComponentDefinitionBase, IFormBuilderDefinition
    {
        /// <summary>
        /// Gets the type of the form component validation rule. The type inherits <see cref="ValidationRule"/>.
        /// </summary>
        public Type ValidationRuleType { get; }


        /// <summary>
        /// Value type, which this validation rule is supposed to validate.
        /// </summary>
        public Type ValidatedDataType { get; }


        /// <summary>
        /// Gets or sets the description of the form component validation rule.
        /// </summary>
        public string Description { get; set; }


        Type IFormBuilderDefinition.DefinitionType => ValidationRuleType;


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRuleDefinition"/> class using given identifier, form component validation rule type and name.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component validation rule.</param>
        /// <param name="validationRuleType">Type of the form component validation rule.</param>
        /// <param name="name">Name of the form component validation rule.</param>
        /// <exception cref="ArgumentException">
        /// <para>Specified <paramref name="identifier"/> is null, an empty string or identifier does not specify a valid code name.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="validationRuleType"/> does not inherit <see cref="ValidationRule"/>, is an abstract type or is a generic type which is not constructed.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="name"/> is null or an empty string.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRuleType"/> is null.</exception>
        public ValidationRuleDefinition(string identifier, Type validationRuleType, string name)
            : base(identifier, name)
        {
            ValidateValidationRuleType(validationRuleType);

            ValidationRuleType = validationRuleType;
            ValidatedDataType = validationRuleType.FindTypeByGenericDefinition(typeof(ValidationRule<>)).GetGenericArguments()[0];
        }


        private void ValidateValidationRuleType(Type validationRuleType)
        {
            if (validationRuleType == null)
            {
                throw new ArgumentNullException(nameof(validationRuleType), "The form component validation rule type must be specified.");
            }

            if (validationRuleType.FindTypeByGenericDefinition(typeof(ValidationRule<>)) == null)
            {
                throw new ArgumentException($"Implementation of the '{validationRuleType.FullName}' form component validation rule must inherit the '{typeof(ValidationRule<>).FullName}' class.", nameof(validationRuleType));
            }

            if (validationRuleType.IsAbstract)
            {
                throw new ArgumentException($"Implementation of the '{validationRuleType.FullName}' form component validation rule type cannot be abstract.", nameof(validationRuleType));
            }

            if (validationRuleType.IsGenericType && !validationRuleType.IsConstructedGenericType)
            {
                throw new ArgumentException($"Implementation of the '{validationRuleType.FullName}' form component validation rule must be a constructed generic type.", nameof(validationRuleType));
            }
        }
    }
}
