using System;

using CMS.Core;
using CMS.IO;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Abstract output file with the base functionality.
    /// </summary>
    public abstract class AbstractOutputFile
    {
        #region "Variables"

        /// <summary>
        /// Initialization time
        /// </summary>
        protected DateTime mInstantiated = DateTime.Now;

        /// <summary>
        /// Width
        /// </summary>
        protected int mWidth = 0;
        
        /// <summary>
        /// Height
        /// </summary>
        protected int mHeight = 0;

        /// <summary>
        /// Max side size
        /// </summary>
        protected int mMaxSideSize = 0;

        /// <summary>
        /// Binary output data
        /// </summary>
        protected byte[] mOutputData = null;
        
        /// <summary>
        /// Indicates whether data were loaded
        /// </summary>
        protected bool mDataLoaded = false;

        /// <summary>
        /// Indicates whether image was resized
        /// </summary>
        protected bool mResized = false;

        /// <summary>
        /// Indicates whether watermark should be used
        /// </summary>
        protected bool? mUseWatermark = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the watermark is allowed to be used.
        /// </summary>
        public abstract bool UseWatermark
        {
            get;
            set;
        }


        /// <summary>
        /// If set, watermark image is applied to the image. Name of the watermark image from ~/App_Themes/{theme}/Images/Watermarks
        /// </summary>
        public string Watermark
        {
            get;
            set;
        }


        /// <summary>
        /// Watermark position.
        /// </summary>
        public ImageHelper.ImagePositionEnum WatermarkPosition
        {
            get;
            set;
        }


        /// <summary>
        /// Output file data.
        /// </summary>
        public byte[] OutputData
        {
            get
            {
                return mOutputData;
            }
            set
            {
                mOutputData = value;
                mDataLoaded = true;
            }
        }


        /// <summary>
        /// Requested output width.
        /// </summary>
        public int Width
        {
            get
            {
                return mWidth;
            }
            set
            {
                mWidth = value;
            }
        }


        /// <summary>
        /// Requested output Height.
        /// </summary>
        public int Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }


        /// <summary>
        /// Requested output MaxSideSize.
        /// </summary>
        public int MaxSideSize
        {
            get
            {
                return mMaxSideSize;
            }
            set
            {
                mMaxSideSize = value;
            }
        }


        /// <summary>
        /// Returns true if the data is loaded to the object.
        /// </summary>
        public bool DataLoaded
        {
            get
            {
                return mDataLoaded;
            }
        }


        /// <summary>
        /// If true, the file is resized version of the file.
        /// </summary>
        public bool Resized
        {
            get
            {
                return mResized;
            }
            set
            {
                mResized = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies watermark on the given image data.
        /// </summary>
        /// <param name="data">Image data</param>
        public void ApplyWatermark(ref byte[] data)
        {
            // Prepare the path
            string path = Watermark;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!path.StartsWithCSafe("~/"))
            {
                path = AdministrationUrlHelper.GetImagePath(null, "Watermarks/" + path, true);
            }

            path = CMSHttpContext.Current.Server.MapPath(path);

            // Only apply if the file exists
            if (!File.Exists(path))
            {
                return;
            }

            var helper = new ImageHelper(data);
            data = helper.GetImageWithWatermarkData(path, WatermarkPosition);
        }


        /// <summary>
        /// Checks whether the watermark can be used
        /// </summary>
        protected bool CheckUseWatermark(string siteName, int imageWidth, int imageHeight)
        {
            // Check if the watermark can be used for current image
            bool watermark = !String.IsNullOrEmpty(Watermark);
            if (!watermark)
            {
                return false;
            }

            // Ensure the resized dimensions to check the watermark
            int[] newDims = ImageHelper.EnsureImageDimensions(Width, Height, MaxSideSize, imageWidth, imageHeight);
            imageWidth = newDims[0];
            imageHeight = newDims[1];

            // Check minimal width
            int minWidth = CoreServices.Settings[siteName + ".CMSMinWatermarkImageWidth"].ToInteger(0);
            if (!SatisfiesWatermarkDimensions(minWidth, imageWidth))
            {
                return false;
            }
            
            // Check minimal height
            int minHeight = CoreServices.Settings[siteName + ".CMSMinWatermarkImageHeight"].ToInteger(0);
            if (!SatisfiesWatermarkDimensions(minHeight, imageHeight))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the minimal
        /// </summary>
        /// <param name="minSize">Minimum size of the image</param>
        /// <param name="originalSize">Original size of the image</param>
        protected bool SatisfiesWatermarkDimensions(int minSize, int originalSize)
        {
            return ((minSize <= 0) || (originalSize >= minSize));
        }

        #endregion
    }
}