namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class providing audio/video parameters.
    /// </summary>
    public class AudioVideoParameters
    {
        #region "Variables"

        private bool mControls = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Site name for which the video is rendered
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Media source url.
        /// </summary>
        public string Url
        {
            get;
            set;
        }


        /// <summary>
        /// Media extension.
        /// </summary>
        public string Extension
        {
            get;
            set;
        }


        /// <summary>
        /// Player width.
        /// </summary>
        public int Width
        {
            get;
            set;
        }


        /// <summary>
        /// Player height.
        /// </summary>
        public int Height
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if media should be played automatically.
        /// </summary>
        public bool AutoPlay
        {
            get;
            set;
        }


        /// <summary>
        /// Loop media after playback ends.
        /// </summary>
        public bool Loop
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if controls should be displayed.
        /// </summary>
        public bool Controls
        {
            get
            {
                return mControls;
            }
            set
            {
                mControls = value;
            }
        }


        /// <summary>
        /// Client id of editor for inserting content.
        /// </summary>
        public string EditorClientID
        {
            get;
            set;
        }

        #endregion
    }
}