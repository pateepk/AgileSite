using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Class representing UI element control.
    /// </summary>
    public interface IUIElementControl
    {
        /// <summary>
        /// Resource name
        /// </summary>
        String ResourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Element name
        /// </summary>
        String ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether control displays breadcrumbs
        /// </summary>
        bool DisplayBreadCrumbs
        {
            get;
        }
    }
}
