using System.Collections.Generic;

using CMS.DancingGoat.Samples;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.ResponsiveImages;

[assembly: RegisterImageVariantDefinition(typeof(SmallSquareImageVariantDefinition))]

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Generates a small square image variant of Cafe images
    /// which are then used in the Cafe email widget.
    /// </summary>
    internal class SmallSquareImageVariantDefinition : ImageVariantDefinition
    {
        private const string IDENTIFIER = "DancingGoatSmallsquare";


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
                        .Type("DancingGoat.Cafe")
                        .Path("/Cafes")
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
                    new ResizeImageFilter(ImageHelper.AUTOSIZE, 105),
                    new CropImageFilter(105, 105, ImageHelper.ImageTrimAreaEnum.MiddleRight)
                };
            }
        }
    }
}