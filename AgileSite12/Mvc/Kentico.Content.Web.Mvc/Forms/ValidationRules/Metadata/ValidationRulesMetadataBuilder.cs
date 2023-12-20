using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Builds validation rules metadata.
    /// </summary>
    internal sealed class ValidationRulesMetadataBuilder : IValidationRulesMetadataBuilder
    {
        private readonly IValidationRuleDefinitionProvider validationRuleDefinitionProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRulesMetadataBuilder"/> class.
        /// </summary>
        /// <param name="validationRuleDefinitionProvider">Provider for registered validation rules retrieval.</param>
        public ValidationRulesMetadataBuilder(IValidationRuleDefinitionProvider validationRuleDefinitionProvider)
        {
            this.validationRuleDefinitionProvider = validationRuleDefinitionProvider ?? throw new ArgumentNullException(nameof(validationRuleDefinitionProvider));
        }


        /// <summary>
        /// Gets metadata of all registered validation rules.
        /// </summary>
        public IList<ValidationRuleMetadata> GetAll()
        {
            var registeredValidationRules = validationRuleDefinitionProvider.GetAll();

            return CreateValidationRulesMetadata(registeredValidationRules);
        }


        private IList<ValidationRuleMetadata> CreateValidationRulesMetadata(IEnumerable<ValidationRuleDefinition> validationRules)
        {
            return validationRules.Select(CreateValidationRuleMetadata).ToList();
        }


        private ValidationRuleMetadata CreateValidationRuleMetadata(ValidationRuleDefinition validationRule)
        {
            return new ValidationRuleMetadata
            {
                Identifier = validationRule.Identifier,
                Name = ResHelper.LocalizeString(validationRule.Name),
                Description = ResHelper.LocalizeString(validationRule.Description),
                ValidatedDataType = validationRule.ValidatedDataType.FullName.ToLowerInvariant()
            };
        }
    }
}
