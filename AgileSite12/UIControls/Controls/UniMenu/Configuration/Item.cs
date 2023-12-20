using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;

using SubMenuItem = CMS.UIControls.UniMenuConfig.SubItem;

namespace CMS.UIControls.UniMenuConfig
{
    /// <summary>
    /// UniMenu item.
    /// </summary>
    public class Item : NavigationItem
    {
        #region "Variables"

        ImageAlign mImageAlign = default(ImageAlign);
        List<SubMenuItem> mSubItems;
        private bool mIsSelectable = true;
        private readonly IDictionary<string, string> mAttributes = new Dictionary<string, string>();

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
        /// The icon class name.
        /// </summary>
        public string IconClass
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
        /// The image align (default: NotSet).
        /// </summary>
        public ImageAlign ImageAlign
        {
            get
            {
                return mImageAlign;
            }
            set
            {
                mImageAlign = value;
            }
        }


        /// <summary>
        /// Minimal width of middle part of the button (in pixels).
        /// </summary>
        public int MinimalWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Template for dragging item representation.
        /// </summary>
        public string DraggableTemplateHandler
        {
            get;
            set;
        }


        /// <summary>
        /// Group of draggable items.
        /// </summary>
        public string DraggableScope
        {
            get;
            set;
        }


        /// <summary>
        /// List of menu item sub items.
        /// </summary>
        public List<SubMenuItem> SubItems
        {
            get
            {
                return mSubItems ?? (mSubItems = new List<SubMenuItem>());
            }
        }


        /// <summary>
        /// Allows toggle for button.
        /// </summary>
        public bool AllowToggle 
        {
            get; 
            set; 
        }


        /// <summary>
        /// Whether the button is toggled by default.
        /// </summary>
        public bool IsToggled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the button is not selectable.
        /// </summary>
        public bool IsSelectable
        {
            get
            {
                return mIsSelectable;
            }
            set
            {
                mIsSelectable = value;
            }
        }


        /// <summary>
        /// Collection of arbitrary attributes (for rendering only) that do not correspond to properties on the control.
        /// </summary>
        public IDictionary<string, string> Attributes
        {
            get
            {
                return mAttributes;
            }
        }

        #endregion
    }
}