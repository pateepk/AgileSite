using System;
using System.Collections.Generic;

namespace CMS.ResponsiveImages
{
    internal interface IImageVariantDefinitionProvider
    {
        /// <summary>
        /// Registers an image variant definition.
        /// </summary>
        /// <param name="type">Definition type which implements <see cref="IImageVariantDefinition"/>.</param>
        void RegisterDefinition(Type type);


        /// <summary>
        /// Registers an image variant definition.
        /// </summary>
        /// <param name="definition">Definition object which implements <see cref="IImageVariantDefinition"/>.</param>
        void RegisterDefinition(IImageVariantDefinition definition);


        /// <summary>
        /// Returns an image variant definition by identifier.
        /// </summary>
        /// <param name="identifier">Definition identifier.</param>
        IImageVariantDefinition GetDefinition(string identifier);


        /// <summary>
        /// Returns all registered image variant definitions.
        /// </summary>
        IEnumerable<IImageVariantDefinition> GetDefinitions();
    }
}