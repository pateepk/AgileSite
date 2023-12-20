using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Universal button control
    /// </summary>
    public abstract class UniButton : CMSUserControl
    {
        #region "Public properties

        /// <summary>
        /// Gets or sets link URL.
        /// </summary>
        public string LinkUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets link text.
        /// </summary>
        public string LinkText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets link CSS class.
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Show as button
        /// </summary>
        public bool ShowAsButton
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets link target.
        /// </summary>
        public string LinkTarget
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets image URL.
        /// </summary>
        public string ImageUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets image alternate text.
        /// </summary>
        public string ImageAltText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets image CSS class.
        /// </summary>
        public string ImageCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Raise event
        /// </summary>
        public string LinkEvent
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string to use on the button
        /// </summary>
        public string ResourceString 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Javascript executed when client clicks on the button
        /// </summary>
        public string OnClientClick
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies whether the control is enabled or not
        /// </summary>
        public virtual bool Enabled 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Style which should be used when the button is displayed.
        /// </summary>
        public virtual ButtonStyle ButtonStyle
        {
            get;
            set;
        }

        #endregion
    }
}
