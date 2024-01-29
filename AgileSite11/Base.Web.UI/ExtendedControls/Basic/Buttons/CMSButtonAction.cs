namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Multi button action.
    /// </summary>
    public class CMSButtonAction
    {
        #region "Variables"

        private bool mEnabled = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Action name. Can be used to identify the action.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Action text
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Script executed when action is clicked
        /// </summary>
        public string OnClientClick
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if action is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Tooltip text
        /// </summary>
        public string ToolTip
        {
            get;
            set;
        }

        #endregion
    }
}