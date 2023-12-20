using System.Collections.Generic;

using CMS.DancingGoat.Samples;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.ResponsiveImages;

[assembly: RegisterImageVariantDefinition(typeof(SmallSizeImageVariantDefinition))]

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample image variant definition for small-size device.
    /// </summary>
    internal class SmallSizeImageVariantDefinition : ImageVariantDefinition
    {
        private const string IDENTIFIER = "DancingGoatSmall";


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
                    new ResizeImageFilter(ImageHelper.AUTOSIZE, 200),
                    new CropImageFilter(184, ImageHelper.AUTOSIZE, ImageHelper.ImageTrimAreaEnum.MiddleRight)
                };
            }
        }
    }
}