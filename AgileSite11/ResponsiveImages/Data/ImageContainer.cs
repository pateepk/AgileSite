using System;
using System.IO;

using CMS.Core;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Represents a container which encapsulates image data that are passed into the image filters.
    /// </summary>
    public class ImageContainer
    {
        private readonly BinaryData mData;
        private readonly ImageMetadata mMetadata;


        /// <summary>
        /// Image metadata.
        /// </summary>
        public ImageMetadata Metadata
        {
            get
            {
                return mMetadata;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <param name="imageMetadata">Image metadata.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public ImageContainer(BinaryData data, ImageMetadata imageMetadata)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (imageMetadata == null)
            {
                throw new ArgumentNullException("imageMetadata");
            }

            mData = data.Data;
            mMetadata = imageMetadata;
        }


        /// <summary>
        /// Opens a read-only stream with the image's data.
        /// </summary>
        public Stream OpenReadStream()
        {
            return new MemoryStream(mData.Data, false);
        }
    }
}
