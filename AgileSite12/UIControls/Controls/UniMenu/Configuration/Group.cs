using System;

namespace CMS.UIControls.UniMenuConfig
{
    /// <summary>
    /// UniMenu group.
    /// </summary>
    public class Group
    {
        #region "Properties"

        /// <summary>
        /// Specifies the the caption of the group.
        /// Sample value: "Caption string"
        /// </summary>
        public string Caption
        {
            get;
            set;
        }


        /// <summary>
        /// The path of the custom user control to use as a content of the group (optional).
        /// Sample value: "~/SampleControl.ascx"
        /// </summary>
        public string ControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// The CSS class to be used for the group.
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// The CSS class to be used for the group separator
        /// </summary>
        public string SeparatorCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// The ID of the UI element to be used to dynamically populate the group with child UI elements (optional).
        /// </summary>
        public int UIElementID
        {
            get;
            set;
        }


        /// <summary>
        /// Custom user control to use as a content of the group.
        /// </summary>
        public CMSUserControl Control
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public Group()
        {
        }

        #endregion
    }
}