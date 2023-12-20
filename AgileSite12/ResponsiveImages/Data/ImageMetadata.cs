using System;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Represents image metadata.
    /// </summary>
    public class ImageMetadata
    {
        /// <summary>
        /// Image width.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }


        /// <summary>
        /// Image MIME type.
        /// </summary>
        public string MimeType
        {
            get;
            private set;
        }


        /// <summary>
        /// Image file extension.
        /// </summary>
        public string Extension
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an instace of the <see cref="ImageMetadata"/> class.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="mimeType">Image MIME type.</param>
        /// <param name="extension">Image file extension.</param>
        public ImageMetadata(int width, int height, string mimeType, string extension)
        {
            Width = width;
            Height = height;
            MimeType = mimeType;
            Extension = extension;
        }
    }
}
