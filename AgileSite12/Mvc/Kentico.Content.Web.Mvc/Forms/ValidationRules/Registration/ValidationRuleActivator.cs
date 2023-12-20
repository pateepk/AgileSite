using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains methods for creating <see cref="ValidationRule"/>s.
    /// </summary>
    public class ValidationRuleActivator : IValidationRuleActivator
    {
        private readonly IValidationRuleDefinitionProvider validationRuleDefinitionProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRuleActivator"/> class.
        /// </summary>
        /// <param name="validationRuleDefinitionProvider">Retrieves <see cref="ValidationRuleDefinition"/>s.</param>
        public ValidationRuleActivator(IValidationRuleDefinitionProvider validationRuleDefinitionProvider)
        {
            this.validationRuleDefinitionProvider = validationRuleDefinitionProvider;
        }


        /// <summary>
        /// Creates a new instance of the <see cref="ValidationRule"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="validationRuleIdentifier">Identifies <see cref="ValidationRule"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="ValidationRule"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationRuleIdentifier"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ValidationRule"/> with given <paramref name="validationRuleIdentifier"/> is not registered in the system.</exception>
        public ValidationRule CreateValidationRule(string validationRuleIdentifier)
        {
            var definition = validationRuleDefinitionProvider.Get(validationRuleIdentifier);
            if (definition == null)
            {
                throw new InvalidOperationException($"Validation rule with identifier '{validationRuleIdentifier}' is not registered in the system.");
            }

            return CreateValidationRule(definition);
        }


        /// <summary>
        /// Creates a new instance of the <see cref="ValidationRule"/> specified by its definition with default property values.
        /// </summary>
        /// <param name="definition">Defines <see cref="ValidationRule"/> which is to be created.</param>
        /// <returns>Returns an instance of <see cref="ValidationRule"/> as described by its definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
        public ValidationRule CreateValidationRule(ValidationRuleDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return (ValidationRule)Activator.CreateInstance(definition.ValidationRuleType);
        }
    }
}
