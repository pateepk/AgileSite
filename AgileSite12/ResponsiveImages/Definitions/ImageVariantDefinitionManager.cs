using System;
using System.Collections.Generic;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Provides methods for image variant definition management.
    /// </summary>
    public static class ImageVariantDefinitionManager
    {
        private static IImageVariantDefinitionProvider mProvider;


        /// <summary>
        /// Provider object.
        /// </summary>
        private static IImageVariantDefinitionProvider ProviderObject
        {
            get
            {
                return mProvider ?? (mProvider = new ImageVariantDefinitionProvider());
            }
        }


        /// <summary>
        /// Sets the provider object.
        /// </summary>
        /// <param name="provider">Image variant definition provider.</param>
        internal static void SetProvider(IImageVariantDefinitionProvider provider)
        {
            mProvider = provider;
        }


        /// <summary>
        /// Registers an image variant definition.
        /// </summary>
        /// <param name="definition">Definition object which implements <see cref="IImageVariantDefinition"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the definition doesn't implement the <see cref="IImageVariantDefinition"/> interface,
        ///     or doesn't provide a parameterless constructor.
        /// </exception>
        public static void RegisterDefinition(IImageVariantDefinition definition)
        {
            ProviderObject.RegisterDefinition(definition);
        }


        /// <summary>
        /// Registers an image variant definition.
        /// </summary>
        /// <param name="type">Definition type which implements <see cref="IImageVariantDefinition"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the definition type doesn't implement the <see cref="IImageVariantDefinition"/> interface,
        ///     is abstract or doesn't provide a parameterless constructor.
        /// </exception>
        internal static void RegisterDefinition(Type type)
        {
            ProviderObject.RegisterDefinition(type);
        }


        /// <summary>
        /// Returns an image variant definition by identifier.
        /// </summary>
        /// <param name="identifier">Definition identifier.</param>
        /// <returns>Image variant definition or <c>null</c> when definition with the given name does not exists.</returns>
        public static IImageVariantDefinition GetDefinition(string identifier)
        {
            return ProviderObject.GetDefinition(identifier);
        }


        /// <summary>
        /// Returns all registered image variant definitions.
        /// </summary>
        public static IEnumerable<IImageVariantDefinition> GetDefinitions()
        {
            return ProviderObject.GetDefinitions();
        }
    }
}
