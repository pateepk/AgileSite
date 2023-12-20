using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered component definitions for Form builder.
    /// </summary>
    public sealed class FormComponentDefinitionProvider : IFormComponentDefinitionProvider
    {
        /// <summary>
        /// Gets a component definition by its <see cref="IFormBuilderDefinition.Identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier of the component definition to retrieve.</param>
        /// <returns>Returns component definition with given identifier, or null when not found.</returns>
        public FormComponentDefinition Get(string identifier)
        {
            return ComponentDefinitionStore<FormComponentDefinition>.Instance.Get(identifier);
        }


        /// <summary>
        /// Gets an enumeration of all registered form control definitions.
        /// </summary>
        public IEnumerable<FormComponentDefinition> GetAll()
        {
            return ComponentDefinitionStore<FormComponentDefinition>.Instance.GetAll();
        }
    }
}
