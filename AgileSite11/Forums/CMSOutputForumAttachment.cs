using System;

namespace CMS.Forums
{
    /// <summary>
    /// Summary description for CMSOutputFile.
    /// </summary>
    public class CMSOutputForumAttachment
    {
        #region "Variables"

        private DateTime mInstantiated = DateTime.Now;

        private string mRedirectTo = "";

        private ForumAttachmentInfo mForumAttachment;

        private int mWidth;

        private int mHeight;

        private int mMaxSideSize;

        private bool mResized;

        private byte[] mOutputData;

        private string mMimeType;

        private bool mDataLoaded;

        private string mPhysicalFile;

        #endregion


        #region "Properties"

        /// <summary>
        /// Forum attachment info without AttachmentBinary.
        /// </summary>
        public ForumAttachmentInfo ForumAttachment
        {
            get
            {
                return mForumAttachment;
            }
            set
            {
                mForumAttachment = value;
                mForumAttachment.AttachmentBinary = null;
            }
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
        /// Time when the file was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                DateTime result;

                // Get the value
                if (mForumAttachment != null)
                {
                    result = mForumAttachment.AttachmentLastModified;
                }
                else
                {
                    result = mInstantiated;
                }

                // Check if not in the future
                if (result > DateTime.Now)
                {
                    result = DateTime.Now;
                }

                return result;
            }
        }


        /// <summary>
        /// If set, file should be redirected to the file system.
        /// </summary>
        public string RedirectTo
        {
            get
            {
                return mRedirectTo;
            }
            set
            {
                mRedirectTo = value;
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
        /// Mime type.
        /// </summary>
        public string MimeType
        {
            get
            {
                if ((mMimeType == null) && (ForumAttachment != null))
                {
                    return ForumAttachment.AttachmentMimeType;
                }
                else
                {
                    return mMimeType;
                }
            }
            set
            {
                mMimeType = value;
            }
        }


        /// <summary>
        /// Physical file path to the data.
        /// </summary>
        public string PhysicalFile
        {
            get
            {
                return mPhysicalFile;
            }
            set
            {
                mPhysicalFile = value;
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
        /// Constructor.
        /// </summary>
        public CMSOutputForumAttachment()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fi">ForumAttachment info</param>
        /// <param name="data">Output file data</param>
        public CMSOutputForumAttachment(ForumAttachmentInfo fi, byte[] data)
        {
            mForumAttachment = fi;
            mOutputData = data;

            mDataLoaded = ((fi == null) && (data == null));
        }


        /// <summary>
        /// Ensures that the object contains the output data.
        /// </summary>
        /// <param name="defaultData">Default data which should be loaded if data required</param>
        /// <returns>Returns true if new data has been loaded</returns>
        public bool EnsureData(byte[] defaultData)
        {
            if (mDataLoaded)
            {
                return false;
            }

            if (defaultData != null)
            {
                OutputData = defaultData;
            }
            else
            {
                // Load the file data
                if (ForumAttachment != null)
                {
                    byte[] data = ForumAttachmentInfoProvider.GetAttachmentFile(ForumAttachment);
                    LoadData(data);
                }
                else
                {
                    OutputData = null;
                }
            }

            mDataLoaded = true;
            return true;
        }


        /// <summary>
        /// Loads the data to the object.
        /// </summary>
        /// <param name="data">New data</param>
        public void LoadData(byte[] data)
        {
            if (data != null)
            {
                if (ForumAttachment == null)
                {
                    throw new Exception("[CMSOutputForumAttachment.LoadData]: Cannot load data to the file object, the ForumAttachment information is missing.");
                }

                // Resize the image if necessary
                if (ForumAttachmentInfoProvider.CanResizeImage(ForumAttachment, Width, Height, MaxSideSize))
                {
                    ForumAttachment.AttachmentBinary = data;
                    data = ForumAttachmentInfoProvider.GetImageThumbnail(ForumAttachment, Width, Height, MaxSideSize);

                    // Clear the attachment binary to save the memory
                    ForumAttachment.AttachmentBinary = null;
                    Resized = true;
                }
            }

            mOutputData = data;
            mDataLoaded = true;
        }

        #endregion
    }
}