
namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class providing YouTube video parameters.
    /// </summary>
    public class YouTubeVideoParameters
    {
        #region "Variables"

        private string mUrl = null;
        private bool mFullScreen = false;
        private bool mAutoPlay = false;
        private bool mRelatedVideos = true;
        private int mWidth = 0;
        private int mHeight = 0;
        private string mEditorClientID = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// YouTube video URL.
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
        /// Indicates if full screen playback is allowed.
        /// </summary>
        public bool FullScreen
        {
            get
            {
                return mFullScreen;
            }
            set
            {
                mFullScreen = value;
            }
        }


        /// <summary>
        /// Indicates if video is played automatically.
        /// </summary>
        public bool AutoPlay
        {
            get
            {
                return mAutoPlay;
            }
            set
            {
                mAutoPlay = value;
            }
        }


        /// <summary>
        /// YouTube include related videos.
        /// </summary>
        public bool RelatedVideos
        {
            get
            {
                return mRelatedVideos;
            }
            set
            {
                mRelatedVideos = value;
            }
        }


        /// <summary>
        /// YouTube player width.
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
        /// YouTube player height.
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

        #endregion
    }
}