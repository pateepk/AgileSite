using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered object definitions for Form Builder.
    /// </summary>
    public sealed class FormBuilderDefinitionProvider<TDefinition> : IFormBuilderDefinitionProvider<TDefinition>
        where TDefinition : ComponentDefinitionBase, IFormBuilderDefinition
    {
        /// <summary>
        /// Gets a Form Builder object definition by its <see cref="IFormBuilderDefinition.Identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier of the object definition to retrieve.</param>
        /// <returns>Returns object definition with given identifier, or null when not found.</returns>
        public TDefinition Get(string identifier)
        {
            return ComponentDefinitionStore<TDefinition>.Instance.Get(identifier);
        }


        /// <summary>
        /// Gets an enumeration of all registered Form Builder object definitions.
        /// </summary>
        public IEnumerable<TDefinition> GetAll()
        {
            return ComponentDefinitionStore<TDefinition>.Instance.GetAll();
        }
    }
}
