namespace CMS.UIControls
{
    /// <summary>
    /// Base class for help control.
    /// </summary>
    public class HelpControl : CMSUserControl
    {
        #region "Public properties"

        /// <summary>
        /// Documentation URL.
        /// </summary>
        public virtual string DocumentationUrl
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Help name (to identify the help).
        /// </summary>
        public virtual string HelpName
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Help topic.
        /// </summary>
        public virtual string TopicName
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Text.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Tooltip.
        /// </summary>
        public virtual string Tooltip
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Icon name.
        /// </summary>
        public virtual string IconName
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Icon CSS class.
        /// </summary>
        public virtual string IconCssClass
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Icon URL.
        /// </summary>
        public virtual string IconUrl
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Indicates whether the control is used in a dialog. If true, use the dark help icon by default
        /// </summary>
        public virtual bool IsDialog
        {
            get;
            set;
        }

        #endregion
    }
}