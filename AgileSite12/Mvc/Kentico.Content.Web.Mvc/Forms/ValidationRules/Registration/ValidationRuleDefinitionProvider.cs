using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered component validation rule definitions for Form builder.
    /// </summary>
    public sealed class ValidationRuleDefinitionProvider : IValidationRuleDefinitionProvider
    {
        /// <summary>
        /// Gets a form component validation rule definition by its identifier.
        /// </summary>
        /// <param name="identifier">Identifier of the form component validation rule definition to retrieve.</param>
        /// <returns>Returns form component validation rule definition with given identifier, or null when not found.</returns>
        public ValidationRuleDefinition Get(string identifier)
        {
            return ComponentDefinitionStore<ValidationRuleDefinition>.Instance.Get(identifier);
        }


        /// <summary>
        /// Gets an enumeration of all registered form control validation rule definitions.
        /// </summary>
        public IEnumerable<ValidationRuleDefinition> GetAll()
        {
            return ComponentDefinitionStore<ValidationRuleDefinition>.Instance.GetAll();
        }
    }
}
