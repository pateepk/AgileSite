using System.Collections.Generic;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Variant definition which is generated using the collection of <see cref="IImageFilter"/>.
    /// </summary>
    public abstract class ImageVariantDefinition : IImageVariantDefinition
    {
        /// <summary>
        /// Definition identifier.
        /// </summary>
        public abstract string Identifier
        {
            get;
        }


        /// <summary>
        /// Returns context scopes to restrict variant application.
        /// </summary>
        public virtual IEnumerable<IVariantContextScope> ContextScopes
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Collection of filters used for variant generation.
        /// </summary>
        public abstract IEnumerable<IImageFilter> Filters
        {
            get;
        }
    }
}
