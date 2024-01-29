using System.Collections.Generic;

using CMS.Base.Web.UI;

namespace CMS.UIControls.UniMenuConfig
{
    /// <summary>
    /// Sub-navigation item
    /// </summary>
    public class SubItem : NavigationItem
    {
        #region "Fields"

        private readonly Dictionary<string, string> mHtmlAttributes = new Dictionary<string, string>();

        #endregion


        #region "Properties"

        /// <summary>
        /// The image path.
        /// </summary>
        public string ImagePath
        {
            get;
            set;
        }


        /// <summary>
        /// The image alternate text.
        /// </summary>
        public string ImageAltText
        {
            get;
            set;
        }


        /// <summary>
        /// The right image icon class.
        /// </summary>
        public string RightImageIconClass
        {
            get;
            set;
        }

        /// <summary>
        /// The right image path.
        /// </summary>
        public string RightImagePath
        {
            get;
            set;
        }


        /// <summary>
        /// The right image alternate text.
        /// </summary>
        public string RightImageAltText
        {
            get;
            set;
        }


        /// <summary>
        /// Menu control path.
        /// If path is defined other properties are ignored.
        /// </summary>
        public string ControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of custom HTML attributes.
        /// </summary>
        public virtual Dictionary<string, string> HtmlAttributes
        {
            get
            {
                return mHtmlAttributes;
            }
        }

        #endregion
    }
}
