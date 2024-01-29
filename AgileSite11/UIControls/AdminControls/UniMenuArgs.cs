using System;
using System.Web.UI.WebControls;

using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// UniMenu event arguments
    /// </summary>
    public class UniMenuArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the UI element.
        /// </summary>
        public UIElementInfo UIElement
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the target URL.
        /// </summary>
        public string TargetUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the button control that will be rendered.
        /// </summary>
        public Panel ButtonControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the image control that will be rendered.
        /// </summary>
        public Image ImageControl
        {
            get;
            set;
        }
    }
}