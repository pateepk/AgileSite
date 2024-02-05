using System;
using System.Linq;

using CMS.Helpers;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Extension methods for <see cref="IImageVariantDefinition"/>.
    /// </summary>
    public static class ImageVariantDefinitionExtensions
    {
        /// <summary>
        /// Generates an image variant by applying all variant definition filters.
        /// </summary>
        /// <param name="imageVariantDefinition">Image variant definition.</param>
        /// <param name="imageContainer">Source image data.</param>
        /// <param name="context">Image processing context.</param>
        /// <returns>
        /// New instance of <see cref="ImageContainer" /> with transformed image data or <c>null</c> when the definition is not applicable for the provided image.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        /// <exception cref="ImageFilterException">Thrown when an error occurs during a filter application.</exception>
        public static ImageContainer GenerateVariant(this IImageVariantDefinition imageVariantDefinition, ImageContainer imageContainer, IVariantContext context)
        {
            if (imageVariantDefinition == null)
            {
                throw new ArgumentNullException("imageVariantDefinition");
            }

            if (imageContainer == null)
            {
                throw new ArgumentNullException("imageContainer");
            }

            if (!imageVariantDefinition.IsApplicable(imageContainer.Metadata, context))
            {
                return null;
            }

            foreach (var filter in imageVariantDefinition.Filters)
            {
                var resultImageVariant = filter.ApplyFilter(imageContainer);

                // Check if the filter got applied, if not, proceed with the previous image
                if (resultImageVariant != null)
                {
                    imageContainer = resultImageVariant;
                }
            }

            return imageContainer;
        }


        /// <summary>
        /// Indicates whether the definition is applicable for the given image metadata.
        /// </summary>
        /// <param name="imageVariantDefinition">Image variant definition.</param>
        /// <param name="metadata">Image metadata for which the condition should be evaluated.</param>
        /// <param name="context">Context in which the variant is generated.</param>
        public static bool IsApplicable(this IImageVariantDefinition imageVariantDefinition, ImageMetadata metadata, IVariantContext context)
        {
            // Check for image by default
            if (!ImageHelper.IsMimeImage(metadata.MimeType))
            {
                return false;
            }

            // If context scopes defined, any of them can enable variant generation
            var contextScopes = imageVariantDefinition.ContextScopes;
            if (contextScopes != null)
            {
                return contextScopes.Any(scope => scope.CheckContext(context));  
            }

            return true;
        }
    }
}
