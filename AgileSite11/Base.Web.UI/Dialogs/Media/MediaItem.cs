using System;

using CMS.Base.Web.UI;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class containing information about the selected item which supposed to be used as media or link.
    /// </summary>
    public class MediaItem
    {
        #region "Private variables"

        private int mNodeId = 0;
        private string mAliasPath = "";
        private string mUrl = "";
        private string mPermanentUrl = "";
        private string mName = "";
        private int mWidth = 0;
        private int mHeight = 0;
        private string mExtension = "";
        private long mSize = 0;
        private int mHistoryId = 0;
        private Nullable<MediaTypeEnum> mMediaType = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// AliasPath of the selected node.
        /// </summary>
        public string AliasPath
        {
            get
            {
                return mAliasPath;
            }
            set
            {
                mAliasPath = value;
            }
        }


        /// <summary>
        /// ID of the selected node.
        /// </summary>
        public int NodeID
        {
            get
            {
                return mNodeId;
            }
            set
            {
                mNodeId = value;
            }
        }


        /// <summary>
        /// URL of the selected item.
        /// </summary>
        public string Url
        {
            get
            {
                return mUrl;
            }
            set
            {
                mUrl = value;
            }
        }


        /// <summary>
        /// URL of the selected item.
        /// </summary>
        public string PermanentUrl
        {
            get
            {
                return mPermanentUrl;
            }
            set
            {
                mPermanentUrl = value;
            }
        }


        /// <summary>
        /// Name of the selected item.
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// ID of the item object history.
        /// </summary>
        public int HistoryID
        {
            get
            {
                return mHistoryId;
            }
            set
            {
                mHistoryId = value;
            }
        }


        /// <summary>
        /// Width of the selected item (set only for images).
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
        /// Height of the selected item (set only for images).
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
        /// File extension of the selected item.
        /// </summary>
        public string Extension
        {
            get
            {
                if (mExtension != null)
                {
                    return mExtension.ToLowerCSafe();
                }
                return mExtension;
            }
            set
            {
                mExtension = value;
                mMediaType = null;
            }
        }


        /// <summary>
        /// Size of the selected item.
        /// </summary>
        public long Size
        {
            get
            {
                return mSize;
            }
            set
            {
                mSize = value;
            }
        }


        /// <summary>
        /// Type of the media determined according to the extension.
        /// </summary>
        public MediaTypeEnum MediaType
        {
            get
            {
                if (mMediaType == null)
                {
                    mMediaType = MediaHelper.GetMediaType(Extension);
                }
                return mMediaType.Value;
            }
        }

        #endregion
    }
}