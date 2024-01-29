
namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class providing image parameters.
    /// </summary>
    public class ImageParameters
    {
        #region "Variables"

        private string mUrl = null;
        private bool mSizeToURL = false;
        private string mExtension = null;
        private string mAlt = null;
        private int mWidth = -1;
        private int mHeight = -1;
        private int mBorderWidth = -1;
        private string mBorderColor = null;
        private int mHSpace = -1;
        private int mVSpace = -1;
        private string mAlign = null;
        private string mId = null;
        private string mTooltip = null;
        private string mClass = null;
        private string mStyle = null;
        private string mLink = null;
        private string mTarget = null;
        private string mBehavior = null;
        private string mEditorClientID = null;
        private string mDir = null;
        private string mUseMap = null;
        private string mLang = null;
        private string mLongDesc = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Image source url.
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
        /// Gets or sets the value which determines whether to append size parameters to URL.
        /// </summary>
        public bool SizeToURL
        {
            get
            {
                return mSizeToURL;
            }
            set
            {
                mSizeToURL = value;
            }
        }


        /// <summary>
        /// Image extension.
        /// </summary>
        public string Extension
        {
            get
            {
                return mExtension;
            }
            set
            {
                mExtension = value;
            }
        }


        /// <summary>
        /// Image alternative text.
        /// </summary>
        public string Alt
        {
            get
            {
                return mAlt;
            }
            set
            {
                mAlt = value;
            }
        }


        /// <summary>
        /// Image width.
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
        /// Image height.
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
        /// Image border width.
        /// </summary>
        public int BorderWidth
        {
            get
            {
                return mBorderWidth;
            }
            set
            {
                mBorderWidth = value;
            }
        }


        /// <summary>
        /// Image border color.
        /// </summary>
        public string BorderColor
        {
            get
            {
                return mBorderColor;
            }
            set
            {
                mBorderColor = value;
            }
        }

        /// <summary>
        /// Image horizontal space.
        /// </summary>
        public int HSpace
        {
            get
            {
                return mHSpace;
            }
            set
            {
                mHSpace = value;
            }
        }


        /// <summary>
        /// Image vertical space.
        /// </summary>
        public int VSpace
        {
            get
            {
                return mVSpace;
            }
            set
            {
                mVSpace = value;
            }
        }


        /// <summary>
        /// Image align.
        /// </summary>
        public string Align
        {
            get
            {
                return mAlign;
            }
            set
            {
                mAlign = value;
            }
        }


        /// <summary>
        /// Image Id.
        /// </summary>
        public string Id
        {
            get
            {
                return mId;
            }
            set
            {
                mId = value;
            }
        }


        /// <summary>
        /// Image tooltip text.
        /// </summary>
        public string Tooltip
        {
            get
            {
                return mTooltip;
            }
            set
            {
                mTooltip = value;
            }
        }


        /// <summary>
        /// Image css class.
        /// </summary>
        public string Class
        {
            get
            {
                return mClass;
            }
            set
            {
                mClass = value;
            }
        }


        /// <summary>
        /// Image inline style.
        /// </summary>
        public string Style
        {
            get
            {
                return mStyle;
            }
            set
            {
                mStyle = value;
            }
        }


        /// <summary>
        /// Image link destination.
        /// </summary>
        public string Link
        {
            get
            {
                return mLink;
            }
            set
            {
                mLink = value;
            }
        }


        /// <summary>
        /// Image link target (_blank/_self/_parent/_top)
        /// </summary>
        public string Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                mTarget = value;
            }
        }


        /// <summary>
        /// Image behavior.
        /// </summary>
        public string Behavior
        {
            get
            {
                return mBehavior;
            }
            set
            {
                mBehavior = value;
            }
        }


        /// <summary>
        /// Client id of editor for inserting content.
        /// </summary>
        public string EditorClientID
        {
            get
            {
                return mEditorClientID;
            }
            set
            {
                mEditorClientID = value;
            }
        }


        /// <summary>
        /// Image direction definition (rtl, ltr).
        /// </summary>
        public string Dir
        {
            get
            {
                return mDir;
            }
            set
            {
                mDir = value;
            }
        }


        /// <summary>
        /// Image use map definition.
        /// </summary>
        public string UseMap
        {
            get
            {
                return mUseMap;
            }
            set
            {
                mUseMap = value;
            }
        }


        /// <summary>
        /// Image language definition.
        /// </summary>
        public string Lang
        {
            get
            {
                return mLang;
            }
            set
            {
                mLang = value;
            }
        }


        /// <summary>
        /// Image long description definition (url).
        /// </summary>
        public string LongDesc
        {
            get
            {
                return mLongDesc;
            }
            set
            {
                mLongDesc = value;
            }
        }

        #endregion
    }
}