using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates <see cref="Mvc.ValidationRule"/> with its <see cref="ValidationRuleDefinition"/> identifier.
    /// </summary>
    /// <seealso cref="Mvc.ValidationRule"/>
    /// <seealso cref="ValidationRuleDefinition"/>
    public sealed class ValidationRuleConfiguration
    {
        /// <summary>
        /// Gets or sets identifier of the <see cref="ValidationRule"/>.
        /// </summary>
        public string Identifier { get; set; }


        /// <summary>
        /// Gets or sets validation rule.
        /// </summary>
        public ValidationRule ValidationRule { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRuleConfiguration"/> class.
        /// This constructor serves for the purpose of deserialization.
        /// </summary>
        public ValidationRuleConfiguration() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRuleConfiguration"/> class.
        /// </summary>
        /// <param name="identifier">Identifies type of the <paramref name="validationRule"/>.</param>
        /// <param name="validationRule">Validation rule.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRule"/> is null.</exception>
        public ValidationRuleConfiguration(string identifier, ValidationRule validationRule)
        {
            Identifier = String.IsNullOrEmpty(identifier) ? throw new ArgumentException(nameof(identifier)) : identifier;
            ValidationRule = validationRule ?? throw new ArgumentNullException(nameof(validationRule));
        }
    }
}
