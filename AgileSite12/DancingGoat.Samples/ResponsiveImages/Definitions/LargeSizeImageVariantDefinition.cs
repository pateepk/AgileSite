using System.Collections.Generic;

using CMS.DancingGoat.Samples;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.ResponsiveImages;

[assembly: RegisterImageVariantDefinition(typeof(LargeSizeImageVariantDefinition))]

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample image variant definition for large-size device.
    /// </summary>
    internal class LargeSizeImageVariantDefinition : ImageVariantDefinition
    {
        private const string IDENTIFIER = "DancingGoatLarge";


        /// <summary>
        /// Definition identifier.
        /// </summary>
        public override string Identifier
        {
            get
            {
                return IDENTIFIER;
            }
        }


        /// <summary>
        /// Returns context scopes to restrict variant application.
        /// </summary>
        public override IEnumerable<IVariantContextScope> ContextScopes
        {
            get
            {
                return new[] {
                    new AttachmentVariantContextScope()
                        .OnSite("DancingGoat")
                        .Type("DancingGoat.Article")
                        .Path("/Articles")
                };
            }
        }


        /// <summary>
        /// Collection of filters used for variant generation.
        /// </summary>
        public override IEnumerable<IImageFilter> Filters
        {
            get
            {
                return new IImageFilter[]
                {
                    new ResizeImageFilter(500, ImageHelper.AUTOSIZE)
                };
            }
        }
    }
}