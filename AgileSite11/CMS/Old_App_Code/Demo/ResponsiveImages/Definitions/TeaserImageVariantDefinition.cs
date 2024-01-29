using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.ResponsiveImages;

using Samples.ResponsiveImages;

[assembly: RegisterImageVariantDefinition(typeof(TeaserImageVariantDefinition))]

namespace Samples.ResponsiveImages
{
    /// <summary>
    /// Generates a teaser article image which is then used in the Article email widget.
    /// </summary>
    internal class TeaserImageVariantDefinition : ImageVariantDefinition
    {
        /// <summary>
        /// Definition identifier.
        /// </summary>
        public override string Identifier => "teaser";


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
                    new ResizeImageFilter(240, ImageHelper.AUTOSIZE),
                    new CropImageFilter(240, 130, ImageHelper.ImageTrimAreaEnum.MiddleCenter)
                };
            }
        }
    }
}