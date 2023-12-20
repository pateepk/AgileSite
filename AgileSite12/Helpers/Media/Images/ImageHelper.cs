using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

using CMS.Core;
using CMS.IO;
using CMS.Base;

using SystemIO = System.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Class providing methods for image processing.
    /// </summary>
    public class ImageHelper
    {
        #region "Variables and constants"

        /// <summary>
        /// Used when resizing image. (Old dimensions: x = 10, y = 20; Resizing: x = AUTOSIZE; y = 10, New dimensions: x = 5, y = 10).
        /// </summary>
        public const int AUTOSIZE = -1;

        private Image mImageData;

        private int mImageWidth = -1;
        private int mImageHeight = -1;

        private static bool? mUseGDIForResize;
        private static int? mDefaultQuality;
        private static CompositingMode mDefaultCompositingMode = CompositingMode.SourceCopy;
        private static CompositingQuality mDefaultCompositingQuality = CompositingQuality.HighQuality;
        private static SmoothingMode mDefaultSmoothingMode = SmoothingMode.HighQuality;
        private static InterpolationMode mDefaultInterpolationMode = InterpolationMode.HighQualityBicubic;
        private static PixelOffsetMode mDefaultPixelOffsetMode = PixelOffsetMode.HighQuality;
        private static WrapMode mDefaultWrapMode = WrapMode.TileFlipXY;
        private static bool mJPEGEncoderLoaded;
        private static ImageCodecInfo mJPEGEncoder;
        private static bool? mUseFixedEnsureImageDimensions;
        private static HashSet<string> mImageExtensions;


        /// <summary>
        /// Enumeration used to specify angle for image rotation.
        /// </summary>
        public enum ImageRotationEnum
        {
            /// <summary>
            /// Rotate 90 degrees clockwise.
            /// </summary>
            Rotate90 = 0,

            /// <summary>
            /// Rotate 180 degrees clockwise.
            /// </summary>
            Rotate180 = 1,

            /// <summary>
            /// Rotate 270 degrees clockwise.
            /// </summary>
            Rotate270 = 2
        };


        /// <summary>
        /// Enumeration used to specify flipping axis.
        /// </summary>
        public enum ImageFlipEnum
        {
            /// <summary>
            /// Flip vertically.
            /// </summary>
            FlipVertical = 0,

            /// <summary>
            /// Flip horizontally.
            /// </summary>
            FlipHorizontal = 1
        };


        /// <summary>
        /// Enumeration used to specify trimming area.
        /// </summary>
        public enum ImageTrimAreaEnum
        {
            /// <summary>
            /// Trim area around top left corner.
            /// </summary>
            TopLeft = 0,

            /// <summary>
            /// Trim area around top center.
            /// </summary>
            TopCenter = 1,

            /// <summary>
            /// Trim area around top right corner.
            /// </summary>
            TopRight = 2,

            /// <summary>
            /// Trim area around middle left.
            /// </summary>
            MiddleLeft = 3,

            /// <summary>
            /// Trim area around middle center.
            /// </summary>
            MiddleCenter = 4,

            /// <summary>
            /// Trim area around middle right.
            /// </summary>
            MiddleRight = 5,

            /// <summary>
            /// Trim area around bottom left corner.
            /// </summary>
            BottomLeft = 6,

            /// <summary>
            /// Trim area around bottom center.
            /// </summary>
            BottomCenter = 7,

            /// <summary>
            /// Trim area around bottom right corner.
            /// </summary>
            BottomRight = 8
        }


        /// <summary>
        /// Enumeration of the image positioning within another image or object.
        /// </summary>
        public enum ImagePositionEnum
        {
            /// <summary>
            /// Top left corner.
            /// </summary>
            TopLeft = 0,

            /// <summary>
            /// Top center.
            /// </summary>
            TopCenter = 1,

            /// <summary>
            /// Top right corner.
            /// </summary>
            TopRight = 2,

            /// <summary>
            /// Middle left.
            /// </summary>
            MiddleLeft = 3,

            /// <summary>
            /// Middle center.
            /// </summary>
            MiddleCenter = 4,

            /// <summary>
            /// Middle right.
            /// </summary>
            MiddleRight = 5,

            /// <summary>
            /// Bottom left corner.
            /// </summary>
            BottomLeft = 6,

            /// <summary>
            /// Bottom center.
            /// </summary>
            BottomCenter = 7,

            /// <summary>
            /// Bottom right corner.
            /// </summary>
            BottomRight = 8,

            /// <summary>
            /// Cover full size of the image.
            /// </summary>
            FullSize = 9,

            /// <summary>
            /// Locate to the center, maximize the size (keeps aspect ratio).
            /// </summary>
            CenterMaxSize = 10
        }


        /// <summary>
        /// Gets the enumeration of the given position string.
        /// </summary>
        public static ImagePositionEnum GetPositionEnum(string position)
        {
            if (position == null)
            {
                return ImagePositionEnum.BottomRight;
            }

            switch (position.ToLowerInvariant())
            {
                case "topleft":
                    return ImagePositionEnum.TopLeft;

                case "topcenter":
                    return ImagePositionEnum.TopCenter;

                case "topright":
                    return ImagePositionEnum.TopRight;

                case "middleleft":
                    return ImagePositionEnum.MiddleLeft;

                case "middlecenter":
                    return ImagePositionEnum.MiddleCenter;

                case "middleright":
                    return ImagePositionEnum.MiddleRight;

                case "bottomleft":
                    return ImagePositionEnum.BottomLeft;

                case "bottomcenter":
                    return ImagePositionEnum.BottomCenter;

                case "bottomright":
                    return ImagePositionEnum.BottomRight;

                case "fullsize":
                    return ImagePositionEnum.FullSize;

                case "centermaxsize":
                    return ImagePositionEnum.CenterMaxSize;

                default:
                    return ImagePositionEnum.BottomRight;
            }
        }


        /// <summary>
        /// Available image types.
        /// </summary>
        public enum ImageTypeEnum
        {
            /// <summary>
            /// Image type is not specified.
            /// </summary>
            None = 0,

            /// <summary>
            /// Image is attachment.
            /// </summary>
            Attachment = 1,

            /// <summary>
            /// Image is metafile.
            /// </summary>
            Metafile = 2,

            /// <summary>
            /// Image is mediafile.
            /// </summary>
            MediaFile = 3,

            /// <summary>
            /// Image is physical file.
            /// </summary>
            PhysicalFile = 4
        }


        /// <summary>
        /// Supported image types for conversion.
        /// </summary>
        public enum SupportedTypesEnum
        {
            /// <summary>
            /// Image type is BMP.
            /// </summary>
            bmp = 0,

            /// <summary>
            /// Image type is GIF.
            /// </summary>
            gif = 1,

            /// <summary>
            /// Image type is JPG/JPEG/PJPEG
            /// </summary>
            jpg = 2,

            /// <summary>
            /// Image type is PNG.
            /// </summary>
            png = 3,
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns image data.
        /// </summary>
        public Image ImageData
        {
            get
            {
                if ((mImageData == null) && (SourceData != null))
                {
                    LoadImage(SourceData);
                }

                return mImageData;
            }
            set
            {
                mImageData = value;

                if (mImageData != null)
                {
                    mImageWidth = mImageData.Width;
                    mImageHeight = mImageData.Height;
                }
            }
        }


        /// <summary>
        /// Source data for the image.
        /// </summary>
        public byte[] SourceData
        {
            get;
            private set;
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public int ImageWidth
        {
            get
            {
                if (mImageWidth < 0)
                {
                    mImageWidth = (ImageData != null ? ImageData.Width : 0);
                }

                return mImageWidth;
            }
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public int ImageHeight
        {
            get
            {
                if (mImageHeight < 0)
                {
                    mImageHeight = (ImageData != null ? ImageData.Height : 0);
                }

                return mImageHeight;
            }
        }


        /// <summary>
        /// Last processing error.
        /// </summary>
        public string LastError
        {
            get;
            private set;
        }


        /// <summary>
        /// JPEG image encoder.
        /// </summary>
        public static ImageCodecInfo JPEGEncoder
        {
            get
            {
                if ((mJPEGEncoder == null) && !mJPEGEncoderLoaded)
                {
                    // Get JPEG encoder
                    mJPEGEncoder = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase));

                    mJPEGEncoderLoaded = true;
                }

                return mJPEGEncoder;
            }
            set
            {
                mJPEGEncoder = value;
            }
        }


        /// <summary>
        /// Default quality to be used when an image is being re-sized. (Default quality is set to 85% by default).
        /// </summary>
        public static int DefaultQuality
        {
            get
            {
                // Get default quality from web.config file settings
                if (mDefaultQuality == null)
                {
                    mDefaultQuality = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSImageResizingQuality"], 85);
                }
                return mDefaultQuality.Value;
            }
            set
            {
                mDefaultQuality = value;
            }
        }


        /// <summary>
        /// Default composition mode is set to SourceCopy.
        /// </summary>
        public static CompositingMode DefaultCompositingMode
        {
            get
            {
                return mDefaultCompositingMode;
            }
            set
            {
                mDefaultCompositingMode = value;
            }
        }


        /// <summary>
        /// Default composing quality is set to HighQuality.
        /// </summary>
        public static CompositingQuality DefaultCompositingQuality
        {
            get
            {
                return mDefaultCompositingQuality;
            }
            set
            {
                mDefaultCompositingQuality = value;
            }
        }


        /// <summary>
        /// Default mode is set to HighQualityBicubic.
        /// </summary>
        public static InterpolationMode DefaultInterpolationMode
        {
            get
            {
                return mDefaultInterpolationMode;
            }
            set
            {
                mDefaultInterpolationMode = value;
            }
        }


        /// <summary>
        /// Default smoothing mode is set to HighQuality.
        /// </summary>
        public static SmoothingMode DefaultSmoothingMode
        {
            get
            {
                return mDefaultSmoothingMode;
            }
            set
            {
                mDefaultSmoothingMode = value;
            }
        }


        /// <summary>
        /// Default offset mode is set to HighQuality.
        /// </summary>
        public static PixelOffsetMode DefaultPixelOffsetMode
        {
            get
            {
                return mDefaultPixelOffsetMode;
            }
            set
            {
                mDefaultPixelOffsetMode = value;
            }
        }


        /// <summary>
        /// Default mode is set to TileFlipXY.
        /// </summary>
        public static WrapMode DefaultWrapMode
        {
            get
            {
                return mDefaultWrapMode;
            }
            set
            {
                mDefaultWrapMode = value;
            }
        }


        /// <summary>
        /// Indicates if GDI resizing will be used for resizing indexed images.
        /// </summary>
        public static bool UseGDIForResize
        {
            get
            {
                if (mUseGDIForResize == null)
                {
                    mUseGDIForResize = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseGDIResizingForImages"], false);
                }
                return (bool)mUseGDIForResize;
            }
            set
            {
                mUseGDIForResize = value;
            }
        }


        /// <summary>
        /// Gets value that indicates if use of new image resizing is enabled.
        /// </summary>
        public static bool UseFixedEnsureImageDimensions
        {
            get
            {
                if (mUseFixedEnsureImageDimensions == null)
                {
                    mUseFixedEnsureImageDimensions = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseFixedEnsureImageDimensions"], false);
                }
                return mUseFixedEnsureImageDimensions.Value;
            }
            set
            {
                mUseFixedEnsureImageDimensions = value;
            }
        }


        /// <summary>
        /// Image extensions supported by the system
        /// </summary>
        public static HashSet<string> ImageExtensions
        {
            get
            {
                if (mImageExtensions == null)
                {
                    // Get extensions from settings
                    string settingsImageExtensions = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSImageExtensions"], "");

                    // Use settings
                    if (!String.IsNullOrEmpty(settingsImageExtensions))
                    {
                        mImageExtensions = new HashSet<string>(settingsImageExtensions.Split(';').Select(e => e.Trim('.')), StringComparer.OrdinalIgnoreCase);
                    }
                    // Use predefined values
                    else
                    {
                        mImageExtensions = new HashSet<string>(new[]
                        {
                            "bmp",
                            "gif",
                            "ico",
                            "png",
                            "wmf",
                            "jpg",
                            "jpeg",
                            "tiff",
                            "tif"
                         }, StringComparer.OrdinalIgnoreCase);
                    }
                }

                return mImageExtensions;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ImageHelper()
        {
        }


        /// <summary>
        /// Constructor with image data to load.
        /// </summary>
        /// <param name="imageData">Image data</param>
        public ImageHelper(byte[] imageData)
        {
            SourceData = imageData;
        }


        /// <summary>
        /// Constructor with image data to load.
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public ImageHelper(byte[] imageData, int width, int height)
            : this(imageData)
        {
            // Sizes
            if (imageData != null)
            {
                if (width > 0)
                {
                    mImageWidth = width;
                }
                if (height > 0)
                {
                    mImageHeight = height;
                }
            }
        }


        /// <summary>
        /// Load image data.
        /// </summary>
        /// <param name="img">Image data to load</param>
        public bool LoadImage(byte[] img)
        {
            if (img == null)
            {
                return true;
            }

            try
            {
                SourceData = img;

                using (var ms = new SystemIO.MemoryStream(img))
                {
                    ImageData = new Bitmap(ms);
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Gets string name of image format.
        /// </summary>
        /// <returns>Converted format</returns>
        public static ImageFormat StringToImageFormat(string currentFormat)
        {
            if (currentFormat == null)
            {
                return null;
            }

            switch (currentFormat.ToLowerInvariant())
            {
                case "bmp":
                    return ImageFormat.Bmp;

                case "gif":
                    return ImageFormat.Gif;

                case "jpg":
                    return ImageFormat.Jpeg;

                case "png":
                    return ImageFormat.Png;

                default:
                    return null;
            }
        }


        /// <summary>
        /// Gets image format.
        /// </summary>
        /// <returns>Converted format</returns>
        public string ImageFormatToString()
        {
            if (ImageData != null)
            {
                var format = ImageData.RawFormat;
                if (format.Equals(ImageFormat.Bmp))
                {
                    return "bmp";
                }
                if (format.Equals(ImageFormat.Gif))
                {
                    return "gif";
                }
                if (format.Equals(ImageFormat.Jpeg))
                {
                    return "jpg";
                }
                if (format.Equals(ImageFormat.Png))
                {
                    return "png";
                }
            }

            return null;
        }


        /// <summary>
        /// Returns resized image as the data array. For quality is used DefaultQuality.
        /// </summary>
        /// <param name="width">Width of the resized image</param>
        /// <param name="height">Height of the resized image</param>
        public byte[] GetResizedImageData(int width, int height)
        {
            return GetResizedImageData(width, height, DefaultQuality);
        }


        /// <summary>
        /// Returns resized image as the data array.
        /// </summary>
        /// <param name="width">Width of the resized image</param>
        /// <param name="height">Height of the resized image</param>
        /// <param name="quality">Thumbnail quality</param>
        public byte[] GetResizedImageData(int width, int height, int quality)
        {
            using (var str = new SystemIO.MemoryStream())
            {
                if (ImageData != null)
                {
                    // Try to process with JPEG encoder
                    if (ImageData.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        // Setup the quality
                        using (EncoderParameters encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                            // Save to the stream
                            Image thumbImage = GetResizedImage(width, height);
                            if (thumbImage != null)
                            {
                                thumbImage.Save(str, JPEGEncoder, encoderParameters);
                            }
                        }
                    }
                    else
                    {
                        // Default way to save if not processed by the encoder
                        Image thumbImage = GetResizedImage(width, height);
                        if (thumbImage != null)
                        {
                            thumbImage.Save(str, ImageData.RawFormat);
                        }
                    }
                }

                // Return the thumbnail data
                byte[] thumbnail = str.ToArray();

                return thumbnail;
            }
        }


        /// <summary>
        /// Returns resized image.
        /// </summary>
        /// <param name="width">Width of the resized image</param>
        /// <param name="height">Height of the resized image</param>
        public Image GetResizedImage(int width, int height)
        {
            // Width has auto size
            if ((width == AUTOSIZE) && (height != AUTOSIZE))
            {
                double ratio = (double)height / (double)ImageHeight;
                width = (int)(ImageWidth * ratio);
            }
            // Height has auto size
            else if ((width != AUTOSIZE) && (height == AUTOSIZE))
            {
                double ratio = (double)width / (double)ImageWidth;
                height = (int)(ImageHeight * ratio);
            }
            // 'Resize' to the current dimensions
            else if ((width == AUTOSIZE) && (height == AUTOSIZE))
            {
                height = ImageHeight;
                width = ImageWidth;
            }

            return ResizeImage(width, height);
        }


        /// <summary>
        /// Returns resized image according to max side size.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>        
        public Image GetResizedImage(int maxSideSize)
        {
            int width = ImageData.Width;
            int height = ImageData.Height;

            // resize to max side size
            if ((maxSideSize < ImageData.Width) || (maxSideSize < ImageData.Height))
            {
                double ratio;

                // Check larger side in image and estimate resize ratio               
                if (ImageData.Width > ImageData.Height)
                {
                    width = maxSideSize;
                    ratio = Convert.ToDouble(width) / Convert.ToDouble(ImageData.Width);
                    height = Convert.ToInt32(ImageData.Height * ratio);
                }
                else
                {
                    height = maxSideSize;
                    ratio = Convert.ToDouble(height) / Convert.ToDouble(ImageData.Height);
                    width = Convert.ToInt32(ImageData.Width * ratio);
                }
            }

            return ResizeImage(width, height);
        }


        /// <summary>
        /// Returns resized image. Input dimensions are none zero.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        private Image ResizeImage(int width, int height)
        {
            if (ImageData == null)
            {
                return null;
            }

            Image resizedImage = null;
            try
            {
                bool dontUsePixelFormat = false;

                // Check pixel format to avoid following exception
                // "A Graphics object cannot be created from an image that has an indexed pixel format."
                switch (ImageData.PixelFormat)
                {
                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Undefined:
                    case PixelFormat.Format16bppArgb1555:
                    case PixelFormat.Format16bppGrayScale:
                        dontUsePixelFormat = true;
                        break;
                }

                // Handle special CMYK images - bad pixel format (Win7 bug)
                if (ImageData.PixelFormat.ToString() == "8207")
                {
                    dontUsePixelFormat = true;
                }

                // Resize GIF
                if (ImageData.RawFormat.Equals(ImageFormat.Gif) || dontUsePixelFormat)
                {
                    using (Bitmap bmp = new Bitmap(ImageData, new Size(width, height)))
                    {
                        using (Image indexedImage = CreateIndexedImage(bmp))
                        {
                            var str = new SystemIO.MemoryStream();
                            indexedImage.Save(str, ImageFormat.Gif);
                            resizedImage = new Bitmap(str);
                        }
                    }
                }
                // Resize all other formats
                else
                {
                    // create the output bitmap
                    resizedImage = new Bitmap(width, height, ImageData.PixelFormat);
                    SetDpi(resizedImage, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                    // create a Graphics object to draw into it
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        // set Graphics objects properties
                        g.CompositingMode = DefaultCompositingMode;
                        g.CompositingQuality = DefaultCompositingQuality;
                        g.SmoothingMode = DefaultSmoothingMode;
                        g.InterpolationMode = DefaultInterpolationMode;
                        g.PixelOffsetMode = DefaultPixelOffsetMode;

                        using (ImageAttributes attr = new ImageAttributes())
                        {
                            attr.SetWrapMode(DefaultWrapMode);
                            g.DrawImage(ImageData, new Rectangle(0, 0, width, height), 0, 0, ImageData.Width, ImageData.Height, GraphicsUnit.Pixel, attr);
                        }
                    }
                }

                return resizedImage;
            }
            catch
            {
                if (resizedImage != null)
                {
                    resizedImage.Dispose();
                }
                throw;
            }
        }


        private void SetDpi(Image resizedImage, float xDpi, float yDpi)
        {
            ((Bitmap)resizedImage).SetResolution(xDpi, yDpi);
        }


        /// <summary>
        /// Returns new image dimensions (int[2]: 0 - width, 1 - height) .
        /// </summary>
        public int[] EnsureImageDimensions(int width, int height, int maxSideSize)
        {
            return EnsureImageDimensions(width, height, maxSideSize, ImageWidth, ImageHeight);
        }


        /// <summary>
        /// Converts Image to byte array.
        /// </summary>
        /// <param name="img">Image object</param>
        public byte[] ImageToBytes(Image img)
        {
            ImageFormat format = ImageFormat.Png;
            if (ImageData != null)
            {
                format = ImageData.RawFormat;
            }

            return GetBytes(img, format);
        }


        /// <summary>
        /// Converts Image to byte array.
        /// </summary>
        /// <param name="img">Image object</param>
        /// <param name="quality">Quality for JPG images</param>
        public byte[] ImageToBytes(Image img, int quality)
        {
            if (img == null)
            {
                return null;
            }

            // Get Bitmap from Image
            using (Bitmap newBitmap = new Bitmap(img))
            {
                using (var str = new SystemIO.MemoryStream())
                { 
                    // Process JPG image type
                    if (ImageData.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        using (EncoderParameters encoderParams = new EncoderParameters(1))
                        {
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                            newBitmap.Save(str, JPEGEncoder, encoderParams);
                        }
                    }
                    // Proces other image types
                    else
                    {
                        newBitmap.Save(str, ImageData.RawFormat);
                    }

                    return str.ToArray();
                }
            }
        }


        /// <summary>
        /// Transforms image to grayscale.
        /// </summary>
        public Image GetGrayscaledImage()
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0))
            {
                return null;
            }

            // Create a blank bitmap of the same size as original
            Bitmap newBitmap = null;
            try
            {
                newBitmap = new Bitmap(ImageWidth, ImageHeight);

                SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                // Get a graphics object from the new image
                using (Graphics graphics = Graphics.FromImage(newBitmap))
                {
                    // Create the grayscale ColorMatrix
                    ColorMatrix colorMatrix = new ColorMatrix(
                        new []
                        {
                            new float[] { .3f, .3f, .3f, 0, 0 },
                            new float[] { .59f, .59f, .59f, 0, 0 },
                            new float[] { .11f, .11f, .11f, 0, 0 },
                            new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 0, 0, 0, 0, 1 }
                        });

                    // Create image attributes
                    using (ImageAttributes attributes = new ImageAttributes())
                    {

                        // Set the color matrix attribute
                        attributes.SetColorMatrix(colorMatrix);

                        // Draw the original image on the new image with grayscale color matrix
                        graphics.DrawImage(ImageData,
                            new Rectangle(0, 0, ImageWidth, ImageHeight),
                            0, 0, ImageWidth, ImageHeight,
                            GraphicsUnit.Pixel, attributes);
                    }
                }

                return newBitmap;
            }
            catch
            {
                if (newBitmap != null)
                {
                    newBitmap.Dispose();
                }
                throw;
            }
        }


        /// <summary>
        /// Transforms image to grayscale.
        /// </summary>
        public byte[] GetGrayscaledImageData()
        {
            // Get image transformed to grayscale and convert it to byte array
            return ImageToBytes(GetGrayscaledImage());
        }


        /// <summary>
        /// Rotates image by specified angle.
        /// </summary>
        /// <param name="rotation">Rotation angle enumeration</param>
        private Image RotateImage(ImageRotationEnum rotation)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0))
            {
                return null;
            }

            // Create bitmap image from original image
            using (var newBitmap = new Bitmap(ImageData))
            {
                SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                // Rotate image by specified angle
                switch (rotation)
                {
                    case ImageRotationEnum.Rotate90:
                    {
                        newBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    }
                    case ImageRotationEnum.Rotate180:
                    {
                        newBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    }
                    case ImageRotationEnum.Rotate270:
                    {
                        newBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    }
                }

                return GetImageInDefaultImageFormat(newBitmap);
            }
        }


        /// <summary>
        /// Rotates image by specified angle.
        /// </summary>
        /// <param name="rotation">Rotation angle enumeration</param>
        public Image GetRotatedImage(ImageRotationEnum rotation)
        {
            return RotateImage(rotation);
        }


        /// <summary>
        /// Rotates image by specified angle.
        /// </summary>
        /// <param name="rotation">Rotation angle enumeration</param>
        public byte[] GetRotatedImageData(ImageRotationEnum rotation)
        {
            // Get rotated image and convert it to byte array
            return ImageToBytes(RotateImage(rotation));
        }


        /// <summary>
        /// Gets the image based on the file name. The method's return value is cached for 60 minutes.
        /// </summary>
        /// <param name="filename">File name</param>
        /// <remarks>
        /// As the return value is cached, the returned <see cref="Image"/> is a shared resource. Performing
        /// any operation which modifies the image state has inherently side effects.
        /// </remarks>
        public Image GetImage(string filename)
        {
            Image img = null;

            // Save the image to the cache for later use
            using (var cs = new CachedSection<Image>(ref img, 60, true, null, "image", filename))
            {
                if (cs.LoadData)
                {
                    // Open the bitmap file
                    using (FileStream fs = FileStream.New(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        img = new Bitmap(fs);
                    }

                    // Cache the data
                    cs.Data = img;

                    // Create cache dependency
                    cs.CacheDependency = CacheHelper.GetFileCacheDependency(filename);
                }
            }

            return img;
        }


        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <param name="filename">File name of the watermark image</param>
        /// <param name="position">Watermark position</param>
        public byte[] GetImageWithWatermarkData(string filename, ImagePositionEnum position)
        {
            // Draw the watermark
            Image mark = GetImage(filename);
            using (Image markedImage = DrawImage(mark, position))
            {
                return ImageToBytes(markedImage);
            }
        }


        /// <summary>
        /// Draws the given image into the current one.
        /// </summary>
        /// <param name="image">Image to draw</param>
        /// <param name="position">Position of the image in the current image</param>
        internal Image DrawImage(Image image, ImagePositionEnum position)
        {
            if (image == null)
            {
                return null;
            }

            lock (image)
            {
                switch (position)
                {
                    // Top
                    case ImagePositionEnum.TopLeft:
                        return DrawImage(image, 0, 0, image.Width, image.Height);

                    case ImagePositionEnum.TopCenter:
                        return DrawImage(image, (ImageWidth - image.Width) / 2, 0, image.Width, image.Height);

                    case ImagePositionEnum.TopRight:
                        return DrawImage(image, (ImageWidth - image.Width), 0, image.Width, image.Height);

                    // Middle
                    case ImagePositionEnum.MiddleLeft:
                        return DrawImage(image, 0, (ImageHeight - image.Height) / 2, image.Width, image.Height);

                    case ImagePositionEnum.MiddleCenter:
                        return DrawImage(image, (ImageWidth - image.Width) / 2, (ImageHeight - image.Height) / 2, image.Width, image.Height);

                    case ImagePositionEnum.MiddleRight:
                        return DrawImage(image, (ImageWidth - image.Width), (ImageHeight - image.Height) / 2, image.Width, image.Height);

                    // Bottom
                    case ImagePositionEnum.BottomLeft:
                        return DrawImage(image, 0, ImageHeight - image.Height, image.Width, image.Height);

                    case ImagePositionEnum.BottomCenter:
                        return DrawImage(image, (ImageWidth - image.Width) / 2, ImageHeight - image.Height, image.Width, image.Height);

                    case ImagePositionEnum.BottomRight:
                        return DrawImage(image, (ImageWidth - image.Width), ImageHeight - image.Height, image.Width, image.Height);

                    // Special

                    case ImagePositionEnum.FullSize:
                        return DrawImage(image, 0, 0, ImageWidth, ImageHeight);

                    case ImagePositionEnum.CenterMaxSize:
                    {
                        if (((float)ImageWidth / image.Width) > ((float)ImageHeight / image.Height))
                        {
                            // Width is smaller
                            int w = (int)((float)ImageHeight / image.Height * image.Width);

                            return DrawImage(image, (ImageWidth - w) / 2, 0, w, ImageHeight);
                        }
                        else
                        {
                            // Height is smaller
                            int h = (ImageWidth / image.Width * image.Height);

                            return DrawImage(image, 0, (ImageHeight - h) / 2, ImageWidth, h);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Draws the given image into the current one.
        /// </summary>
        /// <param name="image">Image to write</param>
        /// <param name="x">X coordinate of the image in the target</param>
        /// <param name="y">Y coordinate of the image in the target</param>
        /// <param name="height">Height of the image in the target</param>
        /// <param name="width">Width of the image in the target</param>
        private Image DrawImage(Image image, int x, int y, int width, int height)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0))
            {
                return null;
            }

            // Ensure the sizes to fit to the image
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x + width > ImageWidth)
            {
                float aspect = (ImageWidth - x) / (float)width;

                width = (int)(width * aspect);
                height = (int)(height * aspect);
            }
            if (y + height > ImageHeight)
            {
                float aspect = (ImageHeight - y) / (float)height;

                width = (int)(width * aspect);
                height = (int)(height * aspect);
            }

            // Create bitmap image from original image
            Bitmap newBitmap = null;
            try
            {
                newBitmap = new Bitmap(ImageWidth, ImageHeight);

                // Get a graphics object from the new image
                using (Graphics graphics = Graphics.FromImage(newBitmap))
                {
                    SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                    // Draw original image
                    graphics.DrawImage(ImageData, 0, 0, ImageWidth, ImageHeight);

                    // Draw the image on top of it
                    graphics.DrawImage(image, x, y, width, height);

                    return newBitmap;
                }
            }
            catch
            {
                if (newBitmap != null)
                {
                    newBitmap.Dispose();
                }
                throw;
            }
        }


        /// <summary>
        /// Flips image.
        /// </summary>
        /// <param name="flip">Flip image horizontally or vertically</param>
        private Image FlipImage(ImageFlipEnum flip)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0))
            {
                return null;
            }

            // Create bitmap image from original image
            using (var newBitmap = new Bitmap(ImageData))
            {
                SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                // Flip image by specified axis
                switch (flip)
                {
                    default:
                        newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;

                    case ImageFlipEnum.FlipHorizontal:
                        newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                }

                return GetImageInDefaultImageFormat(newBitmap);
            }
        }


        /// <summary>
        /// Flips image.
        /// </summary>
        /// <param name="flip">Flip image horizontally or vertically</param>
        public Image GetFlippedImage(ImageFlipEnum flip)
        {
            return FlipImage(flip);
        }


        /// <summary>
        /// Flips image.
        /// </summary>
        /// <param name="flip">Flip image horizontally or vertically</param>
        public byte[] GetFlippedImageData(ImageFlipEnum flip)
        {
            // Get flipped image and convert it to byte array
            return ImageToBytes(FlipImage(flip));
        }


        /// <summary>
        /// Transforms image to different format.
        /// </summary>
        /// <param name="format">New image format</param>
        /// <param name="quality">Image quality applied when converting to JPG</param>
        public Image GetConvertedImage(ImageFormat format, int quality)
        {
            // Get converted image in memory stream
            var str = GetConvertedImageStream(format, quality);
            if (str != null)
            {
                Bitmap newBitmap = null;
                try
                {
                    newBitmap = new Bitmap(str);
                    SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                    return newBitmap;
                }
                catch
                {
                    if (newBitmap != null)
                    {
                        newBitmap.Dispose();
                    }
                    throw;
                }
            }

            return null;
        }


        /// <summary>
        /// Transforms image to different format.
        /// </summary>
        /// <param name="format">New image format</param>
        /// <param name="quality">Image quality applied when converting to JPG</param>/// 
        public byte[] GetConvertedImageData(ImageFormat format, int quality)
        {
            // Get converted image in memory stream
            using (var str = GetConvertedImageStream(format, quality))
            {
                if (str != null)
                {
                    return str.ToArray();
                }
            }

            return null;
        }


        /// <summary>
        /// Transforms image to different format.
        /// </summary>
        /// <param name="format">New image format</param>
        /// <param name="quality">Image quality applied when converting to JPG</param>
        /// <returns>MemoryStream</returns>
        private SystemIO.MemoryStream GetConvertedImageStream(ImageFormat format, int quality)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0))
            {
                return null;
            }

            // Create bitmap image from original image
            using (Bitmap newBitmap = new Bitmap(ImageData))
            {
                newBitmap.SetResolution(ImageData.HorizontalResolution, ImageData.VerticalResolution);
                SystemIO.MemoryStream str = null;
                try
                {
                    str = new SystemIO.MemoryStream();

                    // Try to process with JPEG encoder
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        // Setup the quality
                        using (EncoderParameters encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                            newBitmap.Save(str, JPEGEncoder, encoderParameters);
                        }
                    }
                    // Create indexed bitmat for GIF format
                    else if (format.Equals(ImageFormat.Gif))
                    {
                        using (Bitmap bmp = CreateIndexedImage(newBitmap))
                        {
                            bmp.Save(str, format);
                        }
                    }
                    // Otherwise convert image without quality setting
                    else
                    {
                        newBitmap.Save(str, format);
                    }

                    return str;
                }
                catch
                {
                    if (str != null)
                    {
                        str.Dispose();
                    }
                    throw;
                }
            }
        }


        /// <summary>
        /// Trim image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="area">Enumeration to specify the area to keep</param>
        private Image TrimImage(int width, int height, ImageTrimAreaEnum area)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0) || (width <= 0) || (height <= 0) || (width > ImageWidth) || (height > ImageHeight))
            {
                return null;
            }

            // Create cropping area
            Rectangle cropArea = new Rectangle();

            // Set cropping area
            switch (area)
            {
                case ImageTrimAreaEnum.TopLeft:
                    cropArea = new Rectangle(0, 0, width, height);
                    break;

                case ImageTrimAreaEnum.TopCenter:
                    cropArea = new Rectangle(((ImageWidth - width) / 2), 0, width, height);
                    break;

                case ImageTrimAreaEnum.TopRight:
                    cropArea = new Rectangle((ImageWidth - width), 0, width, height);
                    break;

                case ImageTrimAreaEnum.MiddleLeft:
                    cropArea = new Rectangle(0, ((ImageHeight - height) / 2), width, height);
                    break;

                case ImageTrimAreaEnum.MiddleCenter:
                    cropArea = new Rectangle(((ImageWidth - width) / 2), ((ImageHeight - height) / 2), width, height);
                    break;

                case ImageTrimAreaEnum.MiddleRight:
                    cropArea = new Rectangle((ImageWidth - width), ((ImageHeight - height) / 2), width, height);
                    break;

                case ImageTrimAreaEnum.BottomLeft:
                    cropArea = new Rectangle(0, (ImageHeight - height), width, height);
                    break;

                case ImageTrimAreaEnum.BottomCenter:
                    cropArea = new Rectangle(((ImageWidth - width) / 2), (ImageHeight - height), width, height);
                    break;

                case ImageTrimAreaEnum.BottomRight:
                    cropArea = new Rectangle((ImageWidth - width), (ImageHeight - height), width, height);
                    break;
            }

            return TrimImageCore(cropArea);
        }

        
        /// <summary>
        /// Trim image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="xPos">X coordinates of trimmed area</param>
        /// <param name="yPos">Y coordinates of trimmed area</param>
        private Image TrimImage(int width, int height, int xPos, int yPos)
        {
            if ((ImageWidth <= 0) || (ImageHeight <= 0) || (width <= 0) || (height <= 0) || (width > ImageWidth) || (height > ImageHeight))
            {
                return null;
            }

            Rectangle cropArea = new Rectangle(xPos, yPos, width, height);

            return TrimImageCore(cropArea);
        }


        private Image TrimImageCore(Rectangle cropArea)
        {
            // Trim GIF
            if (ImageData.RawFormat.Equals(ImageFormat.Gif))
            {
                return TrimGifImage(cropArea);
            }
            // Trim all other formats
            using (Bitmap newBitmap = new Bitmap(ImageData))
            {
                SetDpi(newBitmap, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                using (Bitmap cloneBitmap = newBitmap.Clone(cropArea, ImageData.PixelFormat))
                {
                    return GetImageInDefaultImageFormat(cloneBitmap);
                }
            }
        }


        /// <summary>
        /// Trims GIF image according to <paramref name="cropArea"/>.
        /// </summary>
        private Image TrimGifImage(Rectangle cropArea)
        {
            using (Bitmap pngBitmap = ConvertToPngBitmap(ImageData))
            {
                // Create trimmed image
                using (Image resized = pngBitmap.Clone(cropArea, PixelFormat.Format32bppArgb))
                {
                    Image gifImage = null;
                    try
                    {
                        gifImage = ConvertToGifImage(resized);
                        SetDpi(gifImage, ImageData.HorizontalResolution, ImageData.VerticalResolution);

                        return gifImage;
                    }
                    catch
                    {
                        if (gifImage != null)
                        {
                            gifImage.Dispose();
                        }
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Converts <paramref name="image"/> to PNG <see cref="Bitmap"/>.
        /// </summary>
        private static Bitmap ConvertToPngBitmap(Image image)
        {
            using (var originalBitmap = new Bitmap(image))
            {
                using (var memoryStream = new SystemIO.MemoryStream())
                {
                    originalBitmap.Save(memoryStream, ImageFormat.Png);

                    return new Bitmap(memoryStream);
                }
            }
        }


        /// <summary>
        /// Converts <paramref name="image"/> to GIF <see cref="Image"/>.
        /// </summary>
        private static Image ConvertToGifImage(Image image)
        {
            using (var memoryStream = new SystemIO.MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Gif);

                return new Bitmap(memoryStream);
            }
        }


        /// <summary>
        /// Trim image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="area">Enumeration to specify the area to keep</param>
        public Image GetTrimmedImage(int width, int height, ImageTrimAreaEnum area)
        {
            return TrimImage(width, height, area);
        }


        /// <summary>
        /// Trim image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="xPos">X coordinates of trimmed area</param>
        /// <param name="yPos">Y coordinates of trimmed area</param>
        public Image GetTrimmedImage(int width, int height, int xPos, int yPos)
        {
            return TrimImage(width, height, xPos, yPos);
        }


        /// <summary>
        /// Trim image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="area">Enumeration to specify the area to keep</param>
        public byte[] GetTrimmedImageData(int width, int height, ImageTrimAreaEnum area)
        {
            // Get trimmed image and convert it to byte array
            return ImageToBytes(TrimImage(width, height, area));
        }


        /// <summary>
        /// Crop image by specified area.
        /// </summary>
        /// <param name="width">New width of the image</param>
        /// <param name="height">New height of the image</param>
        /// <param name="xPos">X coordinates of trimmed area</param>
        /// <param name="yPos">Y coordinates of trimmed area</param>
        public byte[] GetTrimmedImageData(int width, int height, int xPos, int yPos)
        {
            // Get trimmed image and convert it to byte array
            return ImageToBytes(TrimImage(width, height, xPos, yPos));
        }


        /// <summary>
        /// Returns given image <paramref name="rawImage"/> in original image format.
        /// </summary>
        /// <param name="rawImage">Raw data of modified image</param>
        private Image GetImageInDefaultImageFormat(Bitmap rawImage)
        {
            using (var stream = new SystemIO.MemoryStream())
            {
                // Return image in the original image format
                rawImage.Save(stream, ImageData.RawFormat);

                return new Bitmap(stream);
            }
        }


        /// <summary>
        /// Indicates if current image can be resized to the specified dimensions.
        /// </summary>
        /// <param name="width">Required width</param>
        /// <param name="height">Required height</param>
        /// <param name="maxSideSize">Required max side size</param>        
        public bool CanResizeImage(int width, int height, int maxSideSize)
        {
            // Resize only when bigger than required 
            if (maxSideSize > 0)
            {
                if ((maxSideSize < ImageWidth) || (maxSideSize < ImageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (ImageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (ImageHeight > height))
                {
                    return true;
                }
            }

            return false;
        }

#endregion


#region "Static methods"

        /// <summary>
        /// Gets the URL for the given QR code
        /// </summary>
        public static string GetQRCodeUrl(string code, int size)
        {
            return GetQRCodeUrl(code, size, null, 0, null, 0, null, null);
        }


        /// <summary>
        /// Gets the URL for the given QR code
        /// </summary>
        /// <param name="code">Code to generate by the QR code</param>
        /// <param name="size">Image size, image is rendered with recommended resolution for the QR code</param>
        /// <param name="encoding">Encoding, possible options (B - Byte, AN - Alphanumeric, N - Numeric)</param>
        /// <param name="version">QR code version (by default supported 1 to 10), additional data templates may be put into ~/App_Data/CMS_Modules/QRCode/Resources.zip</param>
        /// <param name="correction">Correction type, possible options (L, M, Q, H)</param>
        /// <param name="maxSideSize">Maximum size of the code in pixels, the code will be resized if larger than this size</param>
        /// <param name="fgColor">Foreground color</param>
        /// <param name="bgColor">Background color</param>
        public static string GetQRCodeUrl(string code, int size, string encoding, int version, string correction, int maxSideSize, string fgColor, string bgColor)
        {
            // Build the query
            string query = QueryHelper.BuildQuery(
                "qrcode", code,
                "e", encoding,
                "s", (size > 0 ? size.ToString() : null),
                "v", (version > 0 ? version.ToString() : null),
                "c", correction,
                "fc", fgColor,
                "bc", bgColor,
                "maxsidesize", (maxSideSize > 0 ? maxSideSize.ToString() : null)
            );

            string hash = QueryHelper.GetHash(query, true);

            string result = String.Format("~/CMSPages/GetResource.ashx{0}&hash={1}", query, hash);

            return result;
        }


        /// <summary>
        /// Gets the binary data for the given image
        /// </summary>
        /// <param name="img">Image</param>
        /// <param name="format">Format of the output</param>
        public static byte[] GetBytes(Image img, ImageFormat format)
        {
            if (img == null)
            {
                return null;
            }

            // Get Bitmap from Image
            using (Bitmap newBitmap = new Bitmap(img))
            {
                newBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                using (var str = new SystemIO.MemoryStream())
                {
                    // Process JPG image type
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        using (EncoderParameters encoderParams = new EncoderParameters(1))
                        {
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, DefaultQuality);

                            newBitmap.Save(str, JPEGEncoder, encoderParams);
                        }
                    }
                    // Process other image types
                    else
                    {
                        newBitmap.Save(str, format);
                    }

                    return str.ToArray();
                }
            }
        }


        /// <summary>
        /// Checks if image format is supported by Image Editor.
        /// </summary>
        /// <param name="extension">Format of given image</param>
        /// <returns>True if image format is supported by Image Editor</returns>
        public static bool IsSupportedByImageEditor(string extension)
        {
            bool isSupported = false;

            if (extension != null)
            {
                switch (extension.ToLowerInvariant().TrimStart('.'))
                {
                    case "bmp":
                    case "gif":
                    case "jpg":
                    case "jpeg":
                    case "png":

                        isSupported = true;
                        break;
                }
            }

            return isSupported;
        }


        /// <summary>
        /// Computes new image dimensions according to the specified parameters, and returns it.
        /// </summary>
        /// <remarks>
        /// This method prevents image enlargement.
        /// Both image width and image height must be positive integers, otherwise specified image dimensions are returned.
        /// Specified image dimensions are also returned when neither width, height or maxSideSize is a positive integer.
        /// </remarks>
        /// <param name="width">Required image width (optional)</param>
        /// <param name="height">Required image height (optional)</param>
        /// <param name="maxSideSize">Required max image size (overrides both required width and height, optional)</param>
        /// <param name="imageWidth">Image width (required)</param>
        /// <param name="imageHeight">Image height (required)</param>
        /// <returns>An array with computed width ([0]) and height ([1])</returns>
        public static int[] EnsureImageDimensions(int width, int height, int maxSideSize, int imageWidth, int imageHeight)
        {
            // Suppose that image dimensions do not change
            int newWidth = imageWidth;
            int newHeight = imageHeight;

            // There is some work to do when both image dimensions are specified
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                return new[] { newWidth, newHeight };
            }

            // Max side size overrides required dimensions and keeps aspect ratio
            if (maxSideSize > 0)
            {
                if (imageWidth > imageHeight)
                {
                    newWidth = maxSideSize;
                    // Multiplication first for better numeric stability
                    newHeight = (int)Math.Round((double)maxSideSize * imageHeight / imageWidth, MidpointRounding.AwayFromZero);
                }
                else
                {
                    newHeight = maxSideSize;
                    // Multiplication first for better numeric stability
                    newWidth = (int)Math.Round((double)maxSideSize * imageWidth / imageHeight, MidpointRounding.AwayFromZero);
                }
            }
            else
            {
                // There is some work to do when at least one required dimension is specified
                if (width > 0 || height > 0)
                {
                    // Both required dimensions are specified
                    if (width > 0 && height > 0)
                    {
                        // Keep aspect ratio
                        if (UseFixedEnsureImageDimensions)
                        {
                            double ratio = Math.Min((double)width / imageWidth, (double)height / imageHeight);
                            newWidth = (int)Math.Round(imageWidth * ratio, MidpointRounding.AwayFromZero);
                            newHeight = (int)Math.Round(imageHeight * ratio, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            newWidth = Math.Min(width, imageWidth);
                            newHeight = Math.Min(height, imageHeight);
                        }
                    }
                    else
                    {
                        // Calculate the missing required dimension
                        double ratio = (double)imageWidth / imageHeight;
                        if (width <= 0)
                        {
                            width = (int)Math.Round(height * ratio, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            height = (int)Math.Round(width / ratio, MidpointRounding.AwayFromZero);
                        }
                        newWidth = width;
                        newHeight = height;
                    }
                }
            }
            // Prevent image enlargement
            if (newWidth > imageWidth || newHeight > imageHeight)
            {
                newWidth = imageWidth;
                newHeight = imageHeight;
            }

            return new[] { newWidth, newHeight };
        }


        /// <summary>
        /// Returns image thumbnail file name.
        /// </summary>
        /// <param name="guid">File GUID</param>        
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public static string GetImageThumbnailFileName(string guid, int width, int height)
        {
            return guid + "_" + width + "_" + height;
        }


        /// <summary>
        /// Determines whether it is an image extension or not. Accepts extensions in both formats ".gif", and "gif"
        /// </summary>
        /// <param name="extension">File extension to check</param>
        public static bool IsImage(string extension)
        {
            if (extension == null)
            {
                return false;
            }

            return ImageExtensions.Contains(extension.TrimStart('.'));
        }


        /// <summary>
        /// Determines whether image is an image depending on mimetype.
        /// </summary>
        /// <param name="mimetype">Mime type of image</param>
        /// <returns>True if mimetype starts with 'image/'</returns>
        public static bool IsMimeImage(string mimetype)
        {
            return mimetype.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Determines whether it is a html/htm extension or not.
        /// </summary>
        /// <param name="extension">File extension to check</param>
        public static bool IsHtml(string extension)
        {
            if (extension == null)
            {
                return false;
            }

            switch (extension.ToLowerInvariant().TrimStart('.'))
            {
                case "html":
                case "htm":
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Gets dimensions from settings into width, height and max side size parameters.
        /// </summary>
        /// <param name="settings">Form field info settings</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">Required width</param>
        /// <param name="height">Required height</param>
        /// <param name="maxSideSize">Required max side size</param>
        public static void GetAutoResizeDimensions(Hashtable settings, string siteName, out int width, out int height, out int maxSideSize)
        {
            // Set default dimensions
            width = 0;
            height = 0;
            maxSideSize = 0;

            if (settings != null)
            {
                string autoresize = ValidationHelper.GetString(settings["autoresize"], "").ToLowerInvariant();

                // Set custom settings
                if (autoresize == "custom")
                {
                    width = ValidationHelper.GetInteger(settings["autoresize_width"], 0);
                    height = ValidationHelper.GetInteger(settings["autoresize_height"], 0);
                    maxSideSize = ValidationHelper.GetInteger(settings["autoresize_maxsidesize"], 0);
                }
                else if (autoresize == "noresize")
                {
                    // No resize is required
                }
                else
                {
                    // Set site settings
                    width = GetAutoResizeToWidth(siteName);
                    height = GetAutoResizeToHeight(siteName);
                    maxSideSize = GetAutoResizeToMaxSideSize(siteName);
                }
            }
        }


        /// <summary>
        /// Unifies file extensions.
        /// </summary>
        /// <param name="extension">Extension of the file (e.x. 'jpg')</param>
        /// <returns>Correct extension of file.</returns>
        public static string UnifyFileExtension(string extension)
        {
            extension = extension.Trim('.').ToLowerInvariant();
            switch (extension)
            {
                case "jpg":
                    return "jpeg";

                case "tif":
                    return "tiff";

                case "docx":
                    return "doc";

                case "xlsx":
                    return "xls";

                case "pptx":
                    return "ppt";

                default:
                    return extension;
            }
        }


        /// <summary>
        /// Returns image resize enumeration for given string.
        /// </summary>
        /// <param name="resizeString">Enumeration string</param>
        public static ImageResizeEnum GetResizeEnum(string resizeString)
        {
            try
            {
                //return (ImageResizeEnum)Enum.Parse(typeof(ImageResizeEnum), resizeString, true);
                return (ImageResizeEnum)Enum.Parse(typeof(ImageResizeEnum), resizeString, true);
            }
            catch (Exception)
            {
                return ImageResizeEnum.Auto;
            }
        }


        /// <summary>
        /// Returns WIDTH the images should be automatically resized to. Value is returned from the site settings.
        /// </summary>
        public static int GetAutoResizeToWidth(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSAutoResizeImageWidth"].ToInteger(0);
        }


        /// <summary>
        /// Returns HEIGHT the images should be automatically resized to. Value is returned from the site settings.
        /// </summary>
        public static int GetAutoResizeToHeight(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSAutoResizeImageHeight"].ToInteger(0);
        }


        /// <summary>
        /// Returns MAX SIDE SIZE the images should be automatically resized to. Value is returned from the site settings.
        /// </summary>
        public static int GetAutoResizeToMaxSideSize(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSAutoResizeImageMaxSideSize"].ToInteger(0);
        }


        /// <summary>
        /// If posted file is image, resizes it to the required dimensions if possible and returns resized data. If it is not image, original data are returned.
        /// </summary>
        /// <param name="fileBinary">Posted file data</param>
        /// <param name="fileExtension">Extension of the file</param>
        /// <param name="width">Image required width</param>
        /// <param name="height">Image required height</param>
        /// <param name="maxSideSize">Image required max side size</param>        
        public static byte[] GetResizedImageData(byte[] fileBinary, string fileExtension, int width, int height, int maxSideSize)
        {
            // Resize image if required
            if ((fileBinary != null) && IsImage(fileExtension))
            {
                ImageHelper ih = new ImageHelper(fileBinary);
                if (ih.CanResizeImage(width, height, maxSideSize))
                {
                    int[] dimensions = EnsureImageDimensions(width, height, maxSideSize, ih.ImageWidth, ih.ImageHeight);
                    fileBinary = ih.GetResizedImageData(dimensions[0], dimensions[1], DefaultQuality);
                }

                return fileBinary;
            }

            return null;
        }


        /// <summary>
        /// Creates an indexed image from a bitmap with a given palette.
        /// </summary>
        /// <param name="src">The source image</param>
        /// <returns>An indexed image.</returns>
        private static Bitmap CreateIndexedImage(Bitmap src)
        {
            // If GDI resize is used
            if (UseGDIForResize)
            {
                // Return input bitmap
                return src;
            }

            // Create new Octree Quantizer for 256 colors of 8bpp color
            OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
            // Quantize picture
            return quantizer.Quantize(src);
        }

#endregion


#region "Class Quantizer"

        /// <summary>
        /// Summary description for Class1.
        /// </summary>
        public abstract class Quantizer
        {
            /// <summary>
            /// Flag used to indicate whether a single pass or two passes are needed for quantization.
            /// </summary>
            private bool mSinglePass = true;

            private int mPixelSize;


            /// <summary>
            /// Construct the quantizer.
            /// </summary>
            /// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
            /// <remarks>
            /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
            /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
            /// and then 'QuantizeImage'.
            /// </remarks>
            public Quantizer(bool singlePass)
            {
                mSinglePass = singlePass;
                mPixelSize = Marshal.SizeOf(typeof(Color32));
            }


            /// <summary>
            /// Quantize an image and return the resulting output bitmap.
            /// </summary>
            /// <param name="source">The image to quantize</param>
            /// <returns>A quantized version of the image</returns>
            public Bitmap Quantize(Image source)
            {
                // Get the size of the source image
                int height = source.Height;
                int width = source.Width;

                // And construct a rectangle from these dimensions
                Rectangle bounds = new Rectangle(0, 0, width, height);

                // First off take a 32bpp copy of the image
                using (Bitmap copy = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    // And construct an 8bpp version
                    Bitmap output = null;
                    try
                    {
                        output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                        // Now lock the bitmap into memory
                        using (Graphics g = Graphics.FromImage(copy))
                        {
                            g.PageUnit = GraphicsUnit.Pixel;

                            // Draw the source image onto the copy bitmap,
                            // which will effect a widening as appropriate.
                            g.DrawImage(source, bounds);
                        }

                        // Define a pointer to the bitmap data
                        BitmapData sourceData = null;

                        try
                        {
                            // Get the source image bits and lock into memory
                            sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                            // Call the FirstPass function if not a single pass algorithm.
                            // For something like an octree quantizer, this will run through
                            // all image pixels, build a data structure, and create a palette.
                            if (!mSinglePass)
                            {
                                FirstPass(sourceData, width, height);
                            }

                            // Then set the color palette on the output bitmap. I'm passing in the current palette 
                            // as there's no way to construct a new, empty palette.
                            output.Palette = GetPalette(output.Palette);


                            // Then call the second pass which actually does the conversion
                            SecondPass(sourceData, output, width, height, bounds);
                        }
                        finally
                        {
                            // Ensure that the bits are unlocked
                            copy.UnlockBits(sourceData);
                        }

                        // Last but not least, return the output bitmap
                        return output;
                    }
                    catch
                    {
                        if (output != null)
                        {
                            output.Dispose();
                        }
                        throw;
                    }
                }
            }


            /// <summary>
            /// Execute the first pass through the pixels in the image.
            /// </summary>
            /// <param name="sourceData">The source data</param>
            /// <param name="width">The width in pixels of the image</param>
            /// <param name="height">The height in pixels of the image</param>
            protected virtual void FirstPass(BitmapData sourceData, int width, int height)
            {
                // Define the source data pointers. The source row is a byte to
                // keep addition of the stride value easier (as this is in bytes)              
                IntPtr pSourceRow = sourceData.Scan0;

                // Loop through each row
                for (int row = 0; row < height; row++)
                {
                    // Set the source pixel to the first pixel in this row
                    IntPtr pSourcePixel = pSourceRow;

                    // And loop through each column
                    for (int col = 0; col < width; col++)
                    {
                        InitialQuantizePixel(new Color32(pSourcePixel));
                        pSourcePixel = (IntPtr)((Int64)pSourcePixel + mPixelSize);
                    } // Now I have the pixel, call the FirstPassQuantize function...

                    // Add the stride to the source row
                    pSourceRow = (IntPtr)((Int64)pSourceRow + sourceData.Stride);
                }
            }


            /// <summary>
            /// Execute a second pass through the bitmap.
            /// </summary>
            /// <param name="sourceData">The source bitmap, locked into memory</param>
            /// <param name="output">The output bitmap</param>
            /// <param name="width">The width in pixels of the image</param>
            /// <param name="height">The height in pixels of the image</param>
            /// <param name="bounds">The bounding rectangle</param>
            protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
            {
                BitmapData outputData = null;

                try
                {
                    // Lock the output bitmap into memory
                    outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

                    // Define the source data pointers. The source row is a byte to
                    // keep addition of the stride value easier (as this is in bytes)
                    IntPtr pSourceRow = sourceData.Scan0;
                    IntPtr pSourcePixel = pSourceRow;
                    IntPtr pPreviousPixel = pSourcePixel;

                    // Now define the destination data pointers
                    IntPtr pDestinationRow = outputData.Scan0;
                    IntPtr pDestinationPixel = pDestinationRow;

                    // And convert the first pixel, so that I have values going into the loop

                    byte pixelValue = QuantizePixel(new Color32(pSourcePixel));

                    // Assign the value of the first pixel
                    Marshal.WriteByte(pDestinationPixel, pixelValue);

                    // Loop through each row
                    for (int row = 0; row < height; row++)
                    {
                        // Set the source pixel to the first pixel in this row
                        pSourcePixel = pSourceRow;

                        // And set the destination pixel pointer to the first pixel in the row
                        pDestinationPixel = pDestinationRow;

                        // Loop through each pixel on this scan line
                        for (int col = 0; col < width; col++)
                        {
                            // Check if this is the same as the last pixel. If so use that value
                            // rather than calculating it again. This is an inexpensive optimisation.
                            if (Marshal.ReadInt32(pPreviousPixel) != Marshal.ReadInt32(pSourcePixel))
                            {
                                // Quantize the pixel
                                pixelValue = QuantizePixel(new Color32(pSourcePixel));

                                // And setup the previous pointer
                                pPreviousPixel = pSourcePixel;
                            }

                            // And set the pixel in the output
                            Marshal.WriteByte(pDestinationPixel, pixelValue);

                            pSourcePixel = (IntPtr)((long)pSourcePixel + mPixelSize);
                            pDestinationPixel = (IntPtr)((long)pDestinationPixel + 1);
                        }

                        // Add the stride to the source row
                        pSourceRow = (IntPtr)((long)pSourceRow + sourceData.Stride);

                        // And to the destination row
                        pDestinationRow = (IntPtr)((long)pDestinationRow + outputData.Stride);
                    }
                }
                finally
                {
                    // Ensure that I unlock the output bits
                    output.UnlockBits(outputData);
                }
            }


            /// <summary>
            /// Override this to process the pixel in the first pass of the algorithm.
            /// </summary>
            /// <param name="pixel">The pixel to quantize</param>
            /// <remarks>
            /// This function need only be overridden if your quantize algorithm needs two passes,
            /// such as an Octree quantizer.
            /// </remarks>
            protected virtual void InitialQuantizePixel(Color32 pixel)
            {
            }


            /// <summary>
            /// Override this to process the pixel in the second pass of the algorithm.
            /// </summary>
            /// <param name="pixel">The pixel to quantize</param>
            /// <returns>The quantized value</returns>
            protected abstract byte QuantizePixel(Color32 pixel);


            /// <summary>
            /// Retrieve the palette for the quantized image.
            /// </summary>
            /// <param name="original">Any old palette, this is overrwritten</param>
            /// <returns>The new color palette</returns>
            protected abstract ColorPalette GetPalette(ColorPalette original);


            /// <summary>
            /// Struct that defines a 32 bpp colour.
            /// </summary>
            /// <remarks>
            /// This struct is used to read data from a 32 bits per pixel image
            /// in memory, and is ordered in this manner as this is the way that
            /// the data is layed out in memory
            /// </remarks>
            [StructLayout(LayoutKind.Explicit)]
            public struct Color32
            {
                /// <summary>
                /// Constructor of Color32.
                /// </summary>
                /// <param name="pSourcePixel">Source pixel pointer</param>
                public Color32(IntPtr pSourcePixel)
                {
                    this = (Color32)Marshal.PtrToStructure(pSourcePixel, typeof(Color32));
                }


                /// <summary>
                /// Holds the blue component of the colour.
                /// </summary>
                [FieldOffset(0)]
                public byte Blue;

                /// <summary>
                /// Holds the green component of the colour.
                /// </summary>
                [FieldOffset(1)]
                public byte Green;

                /// <summary>
                /// Holds the red component of the colour.
                /// </summary>
                [FieldOffset(2)]
                public byte Red;

                /// <summary>
                /// Holds the alpha component of the colour.
                /// </summary>
                [FieldOffset(3)]
                public byte Alpha;

                /// <summary>
                /// Permits the color32 to be treated as an int32.
                /// </summary>
                [FieldOffset(0)]
                public int ARGB;

                /// <summary>
                /// Returns the color for this Color32 object.
                /// </summary>
                public Color Color
                {
                    get
                    {
                        return Color.FromArgb(Alpha, Red, Green, Blue);
                    }
                }
            }
        }

#endregion


#region "Class OctreeQuantizer"

        /// <summary>
        /// Quantize using an Octree.
        /// </summary>
        public class OctreeQuantizer : Quantizer
        {
            /// <summary>
            /// Stores the tree.
            /// </summary>
            private Octree mOctree;


            /// <summary>
            /// Maximum allowed color depth.
            /// </summary>
            private int mMaxColors;


            /// <summary>
            /// Construct the octree quantizer.
            /// </summary>
            /// <remarks>
            /// The Octree quantizer is a two pass algorithm. The initial pass sets up the octree,
            /// the second pass quantizes a color based on the nodes in the tree
            /// </remarks>
            /// <param name="maxColors">The maximum number of colors to return</param>
            /// <param name="maxColorBits">The number of significant bits</param>
            public OctreeQuantizer(int maxColors, int maxColorBits)
                : base(false)
            {
                if (maxColors > 255)
                {
                    throw new ArgumentOutOfRangeException("maxColors", maxColors, "The number of colors should be less than 256");
                }

                if ((maxColorBits < 1) | (maxColorBits > 8))
                {
                    throw new ArgumentOutOfRangeException("maxColorBits", maxColorBits, "This should be between 1 and 8");
                }

                // Construct the octree
                mOctree = new Octree(maxColorBits);
                mMaxColors = maxColors;
            }


            /// <summary>
            /// Process the pixel in the first pass of the algorithm.
            /// </summary>
            /// <param name="pixel">The pixel to quantize</param>
            /// <remarks>
            /// This function need only be overridden if your quantize algorithm needs two passes,
            /// such as an Octree quantizer.
            /// </remarks>
            protected override void InitialQuantizePixel(Color32 pixel)
            {
                // Add the color to the octree
                mOctree.AddColor(pixel);
            }


            /// <summary>
            /// Override this to process the pixel in the second pass of the algorithm.
            /// </summary>
            /// <param name="pixel">The pixel to quantize</param>
            /// <returns>The quantized value</returns>
            protected override byte QuantizePixel(Color32 pixel)
            {
                // The color at [_maxColors] is set to transparent
                byte paletteIndex = (byte)mMaxColors;

                // Get the palette index if this non-transparent
                if (pixel.Alpha > 0)
                {
                    paletteIndex = (byte)mOctree.GetPaletteIndex(pixel);
                }

                return paletteIndex;
            }


            /// <summary>
            /// Retrieve the palette for the quantized image.
            /// </summary>
            /// <param name="original">Any old palette, this is overrwritten</param>
            /// <returns>The new color palette</returns>
            protected override ColorPalette GetPalette(ColorPalette original)
            {
                // First off convert the octree to _maxColors colors
                ArrayList palette = mOctree.Palletize(mMaxColors - 1);

                // Then convert the palette based on those colors
                for (int index = 0; index < palette.Count; index++)
                {
                    original.Entries[index] = (Color)palette[index];
                }

                // Add the transparent color
                original.Entries[mMaxColors] = Color.FromArgb(0, 0, 0, 0);

                return original;
            }


#region "Class Octree"

            /// <summary>
            /// Class which does the actual quantization.
            /// </summary>
            private class Octree
            {
                /// <summary>
                /// Mask used when getting the appropriate pixels for a given node.
                /// </summary>
                private static int[] mask = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };


                /// <summary>
                /// The root of the octree.
                /// </summary>
                private OctreeNode mRoot;


                /// <summary>
                /// Number of leaves in the tree.
                /// </summary>
                private int mLeafCount;


                /// <summary>
                /// Array of reducible nodes.
                /// </summary>
                private OctreeNode[] mReducibleNodes;


                /// <summary>
                /// Maximum number of significant bits in the image.
                /// </summary>
                private int mMaxColorBits;


                /// <summary>
                /// Store the last node quantized.
                /// </summary>
                private OctreeNode mPreviousNode;


                /// <summary>
                /// Cache the previous color quantized.
                /// </summary>
                private int mPreviousColor;


                /// <summary>
                /// Get/Set the number of leaves in the tree
                /// </summary>
                public int Leaves
                {
                    get
                    {
                        return mLeafCount;
                    }
                    set
                    {
                        mLeafCount = value;
                    }
                }


                /// <summary>
                /// Returns the array of reducible nodes.
                /// </summary>
                protected OctreeNode[] ReducibleNodes
                {
                    get
                    {
                        return mReducibleNodes;
                    }
                }


                /// <summary>
                /// Construct the octree.
                /// </summary>
                /// <param name="maxColorBits">The maximum number of significant bits in the image</param>
                public Octree(int maxColorBits)
                {
                    mMaxColorBits = maxColorBits;
                    mLeafCount = 0;
                    mReducibleNodes = new OctreeNode[9];
                    mRoot = new OctreeNode(0, mMaxColorBits, this);
                    mPreviousColor = 0;
                    mPreviousNode = null;
                }


                /// <summary>
                /// Add a given color value to the octree.
                /// </summary>
                /// <param name="pixel">Pixel</param>
                public void AddColor(Color32 pixel)
                {
                    // Check if this request is for the same color as the last
                    if (mPreviousColor == pixel.ARGB)
                    {
                        // If so, check if I have a previous node setup. This will only ocurr if the first color in the image
                        // happens to be black, with an alpha component of zero.
                        if (null == mPreviousNode)
                        {
                            mPreviousColor = pixel.ARGB;
                            mRoot.AddColor(pixel, mMaxColorBits, 0, this);
                        }
                        else
                        {
                            // Just update the previous node
                            mPreviousNode.Increment(pixel);
                        }
                    }
                    else
                    {
                        mPreviousColor = pixel.ARGB;
                        mRoot.AddColor(pixel, mMaxColorBits, 0, this);
                    }
                }


                /// <summary>
                /// Reduce the depth of the tree.
                /// </summary>
                public void Reduce()
                {
                    int index;

                    // Find the deepest level containing at least one reducible node
                    for (index = mMaxColorBits - 1; (index > 0) && (null == mReducibleNodes[index]); index--)
                    {
                        ;
                    }

                    // Reduce the node most recently added to the list at level 'index'
                    OctreeNode node = mReducibleNodes[index];
                    mReducibleNodes[index] = node.NextReducible;

                    // Decrement the leaf count after reducing the node
                    mLeafCount -= node.Reduce();


                    // And just in case I've reduced the last color to be added, and the next color to
                    // be added is the same, invalidate the previousNode...
                    mPreviousNode = null;
                }


                /// <summary>
                /// Keep track of the previous node that was quantized.
                /// </summary>
                /// <param name="node">The node last quantized</param>
                protected void TrackPrevious(OctreeNode node)
                {
                    mPreviousNode = node;
                }


                /// <summary>
                /// Convert the nodes in the octree to a palette with a maximum of colorCount colors.
                /// </summary>
                /// <param name="colorCount">The maximum number of colors</param>
                /// <returns>An arraylist with the palettized colors</returns>
                public ArrayList Palletize(int colorCount)
                {
                    while (Leaves > colorCount)
                    {
                        Reduce();
                    }

                    // Now palettize the nodes
                    ArrayList palette = new ArrayList(Leaves);
                    int paletteIndex = 0;
                    mRoot.ConstructPalette(palette, ref paletteIndex);

                    // And return the palette
                    return palette;
                }


                /// <summary>
                /// Gets the palette index for the passed color.
                /// </summary>
                /// <param name="pixel">Pixel</param>
                public int GetPaletteIndex(Color32 pixel)
                {
                    return mRoot.GetPaletteIndex(pixel, 0);
                }


#region "Class OctreeNode"

                /// <summary>
                /// Class which encapsulates each node in the tree.
                /// </summary>
                protected class OctreeNode
                {
                    /// <summary>
                    /// Flag indicating that this is a leaf node.
                    /// </summary>
                    private bool mLeaf;

                    /// <summary>
                    /// Number of pixels in this node.
                    /// </summary>
                    private int mPixelCount;

                    /// <summary>
                    /// Red component.
                    /// </summary>
                    private int mRed;

                    /// <summary>
                    /// Green Component.
                    /// </summary>
                    private int mGreen;

                    /// <summary>
                    /// Blue component.
                    /// </summary>
                    private int mBlue;

                    /// <summary>
                    /// Pointers to any child nodes.
                    /// </summary>
                    private OctreeNode[] mChildren;

                    /// <summary>
                    /// Pointer to next reducible node.
                    /// </summary>
                    private OctreeNode mNextReducible;

                    /// <summary>
                    /// The index of this node in the palette.
                    /// </summary>
                    private int mPaletteIndex;


                    /// <summary>
                    /// Get/Set the next reducible node
                    /// </summary>
                    public OctreeNode NextReducible
                    {
                        get
                        {
                            return mNextReducible;
                        }
                        set
                        {
                            mNextReducible = value;
                        }
                    }


                    /// <summary>
                    /// Returns the child nodes.
                    /// </summary>
                    public OctreeNode[] Children
                    {
                        get
                        {
                            return mChildren;
                        }
                    }


                    /// <summary>
                    /// Construct the node.
                    /// </summary>
                    /// <param name="level">The level in the tree = 0 - 7</param>
                    /// <param name="colorBits">The number of significant color bits in the image</param>
                    /// <param name="octree">The tree to which this node belongs</param>
                    public OctreeNode(int level, int colorBits, Octree octree)
                    {
                        // Construct the new node
                        mLeaf = (level == colorBits);

                        mRed = mGreen = mBlue = 0;
                        mPixelCount = 0;

                        // If a leaf, increment the leaf count
                        if (mLeaf)
                        {
                            octree.Leaves++;
                            mNextReducible = null;
                            mChildren = null;
                        }
                        else
                        {
                            // Otherwise add this to the reducible nodes
                            mNextReducible = octree.ReducibleNodes[level];
                            octree.ReducibleNodes[level] = this;
                            mChildren = new OctreeNode[8];
                        }
                    }


                    /// <summary>
                    /// Add a color into the tree.
                    /// </summary>
                    /// <param name="pixel">The color</param>
                    /// <param name="colorBits">The number of significant color bits</param>
                    /// <param name="level">The level in the tree</param>
                    /// <param name="octree">The tree to which this node belongs</param>
                    public void AddColor(Color32 pixel, int colorBits, int level, Octree octree)
                    {
                        // Update the color information if this is a leaf
                        if (mLeaf)
                        {
                            Increment(pixel);
                            // Setup the previous node
                            octree.TrackPrevious(this);
                        }
                        else
                        {
                            // Go to the next level down in the tree
                            int shift = 7 - level;
                            int index = ((pixel.Red & mask[level]) >> (shift - 2)) |
                                        ((pixel.Green & mask[level]) >> (shift - 1)) |
                                        ((pixel.Blue & mask[level]) >> (shift));

                            OctreeNode child = mChildren[index];

                            if (null == child)
                            {
                                // Create a new child node & store in the array
                                child = new OctreeNode(level + 1, colorBits, octree);
                                mChildren[index] = child;
                            }

                            // Add the color to the child node
                            child.AddColor(pixel, colorBits, level + 1, octree);
                        }
                    }


                    /// <summary>
                    /// Reduce this node by removing all of its children.
                    /// </summary>
                    /// <returns>The number of leaves removed</returns>
                    public int Reduce()
                    {
                        mRed = mGreen = mBlue = 0;
                        int children = 0;

                        // Loop through all children and add their information to this node
                        for (int index = 0; index < 8; index++)
                        {
                            if (null != mChildren[index])
                            {
                                mRed += mChildren[index].mRed;
                                mGreen += mChildren[index].mGreen;
                                mBlue += mChildren[index].mBlue;
                                mPixelCount += mChildren[index].mPixelCount;
                                ++children;
                                mChildren[index] = null;
                            }
                        }

                        // Now change this to a leaf node
                        mLeaf = true;

                        // Return the number of nodes to decrement the leaf count by
                        return (children - 1);
                    }


                    /// <summary>
                    /// Traverse the tree, building up the color palette.
                    /// </summary>
                    /// <param name="palette">The palette</param>
                    /// <param name="paletteIndex">The current palette index</param>
                    public void ConstructPalette(ArrayList palette, ref int paletteIndex)
                    {
                        if (mLeaf)
                        {
                            // Consume the next palette index
                            mPaletteIndex = paletteIndex++;

                            // And set the color of the palette entry
                            palette.Add(Color.FromArgb(mRed / mPixelCount, mGreen / mPixelCount, mBlue / mPixelCount));
                        }
                        else
                        {
                            // Loop through children looking for leaves
                            for (int index = 0; index < 8; index++)
                            {
                                if (null != mChildren[index])
                                {
                                    mChildren[index].ConstructPalette(palette, ref paletteIndex);
                                }
                            }
                        }
                    }


                    /// <summary>
                    /// Returns the palette index for the passed color.
                    /// </summary>
                    public int GetPaletteIndex(Color32 pixel, int level)
                    {
                        int paletteIndex = mPaletteIndex;

                        if (!mLeaf)
                        {
                            int shift = 7 - level;
                            int index = ((pixel.Red & mask[level]) >> (shift - 2)) |
                                        ((pixel.Green & mask[level]) >> (shift - 1)) |
                                        ((pixel.Blue & mask[level]) >> (shift));

                            if (null != mChildren[index])
                            {
                                paletteIndex = mChildren[index].GetPaletteIndex(pixel, level + 1);
                            }
                            else
                            {
                                throw new Exception("Didn't expect this!");
                            }
                        }

                        return paletteIndex;
                    }


                    /// <summary>
                    /// Increment the pixel count and add to the color information.
                    /// </summary>
                    public void Increment(Color32 pixel)
                    {
                        mPixelCount++;
                        mRed += pixel.Red;
                        mGreen += pixel.Green;
                        mBlue += pixel.Blue;
                    }
                }

#endregion
            }

#endregion
        }

#endregion
    }
}