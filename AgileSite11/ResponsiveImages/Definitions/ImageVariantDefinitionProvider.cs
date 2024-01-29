using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Provides image variant definition management implementation for <see cref="ImageVariantDefinitionManager"/>.
    /// </summary>
    internal class ImageVariantDefinitionProvider : IImageVariantDefinitionProvider
    {
        private static readonly Lazy<StringSafeDictionary<IImageVariantDefinition>> mImageVariantDefinitions = new Lazy<StringSafeDictionary<IImageVariantDefinition>>(() => new StringSafeDictionary<IImageVariantDefinition>());


        /// <summary>
        /// Collection of registered image variant definition instances.
        /// </summary>
        private static StringSafeDictionary<IImageVariantDefinition> ImageVariantDefinitions
        {
            get
            {
                return mImageVariantDefinitions.Value;
            }
        }


        /// <summary>
        /// Registers an image variant definition.
        /// </summary>
        /// <param name="definition">Definition object which implements <see cref="IImageVariantDefinition"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the definition doesn't provide a parameterless constructor.
        /// </exception>
        public void RegisterDefinition(IImageVariantDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }
            
            var identifier = definition.Identifier;
            if (!ValidationHelper.IsIdentifier(identifier))
            {
                EventLogProvider.LogEvent(EventType.ERROR, "ImageVariantDefinitionProvider", "RegisterDefinition", "Definition identifier needs to be in a valid identifier format.");
            }
            else if (ImageVariantDefinitions.ContainsKey(identifier))
            {
                EventLogProvider.LogEvent(EventType.ERROR, "ImageVariantDefinitionProvider", "RegisterDefinition", "Definition with the same identifier '" + identifier + "' has already been registered.");
            }
            else
            {
                ImageVariantDefinitions[identifier] = definition;
            }
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
        public void RegisterDefinition(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!typeof(IImageVariantDefinition).IsAssignableFrom(type))
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "must implement the IImageVariantDefinition interface.", type.FullName), "type");
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "cannot be abstract.", type.FullName), "type");
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "must include a parameterless constructor.", type.FullName), "type");
            }

            var imageVariantDefinition = (IImageVariantDefinition)Activator.CreateInstance(type);

            RegisterDefinition(imageVariantDefinition);
        }


        /// <summary>
        /// Returns an image variant definition by identifier.
        /// </summary>
        /// <param name="identifier">Definition identifier</param>
        public IImageVariantDefinition GetDefinition(string identifier)
        {
            return ImageVariantDefinitions[identifier];
        }


        /// <summary>
        /// Returns all registered image variant definitions.
        /// </summary>
        public IEnumerable<IImageVariantDefinition> GetDefinitions()
        {
            return ImageVariantDefinitions.TypedValues.OrderBy(i => i.Identifier, StringComparer.InvariantCulture);
        }
    }
}
