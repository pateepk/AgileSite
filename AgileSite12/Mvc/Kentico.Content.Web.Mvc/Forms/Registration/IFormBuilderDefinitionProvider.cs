using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provider for retrieval of registered object definitions for Form Builder.
    /// </summary>
    public interface IFormBuilderDefinitionProvider<out TDefinition> where TDefinition : IFormBuilderDefinition
    {
        /// <summary>
        /// Gets a Form Builder object definition by its <see cref="IFormBuilderDefinition.Identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier of the object definition to retrieve.</param>
        /// <returns>Returns object definition with given identifier, or null when not found.</returns>
        TDefinition Get(string identifier);


        /// <summary>
        /// Gets an enumeration of all registered Form Builder object definitions.
        /// </summary>
        IEnumerable<TDefinition> GetAll();
    }
}
