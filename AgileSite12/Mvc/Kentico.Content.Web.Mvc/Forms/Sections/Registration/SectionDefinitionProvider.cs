using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves list of registered section definitions for Form Builder.
    /// </summary>
    public sealed class SectionDefinitionProvider : ISectionDefinitionProvider
    {
        /// <summary>
        /// Gets a section definition by its <see cref="IFormBuilderDefinition.Identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier of the section definition to retrieve.</param>
        /// <returns>Returns section definition with given identifier, or null when not found.</returns>
        public SectionDefinition Get(string identifier)
        {
            return ComponentDefinitionStore<SectionDefinition>.Instance.Get(identifier);
        }


        /// <summary>
        /// Gets an enumeration of all registered section definitions.
        /// </summary>
        public IEnumerable<SectionDefinition> GetAll()
        {
            return ComponentDefinitionStore<SectionDefinition>.Instance.GetAll();
        }
    }
}
