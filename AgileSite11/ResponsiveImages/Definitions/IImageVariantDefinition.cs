using System.Collections.Generic;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Interface for the image variant definition.
    /// </summary>
    public interface IImageVariantDefinition
    {
        /// <summary>
        /// Definition identifier.
        /// </summary>
        string Identifier { get; }


        /// <summary>
        /// Collection of filters used for variant generation.
        /// </summary>
        IEnumerable<IImageFilter> Filters { get; }


        /// <summary>
        /// Returns context scopes to restrict variant application.
        /// </summary>
        IEnumerable<IVariantContextScope> ContextScopes { get; }
    }
}