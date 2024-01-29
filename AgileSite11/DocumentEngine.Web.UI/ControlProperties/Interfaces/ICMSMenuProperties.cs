using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base CMS Menu controls properties interface definition.
    /// </summary>
    public interface ICMSMenuProperties : ICMSControlProperties
    {
        /// <summary>
        /// Indicates if apply document menu item properties.
        /// </summary>
        bool ApplyMenuDesign
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if highlighted images is not specified, use item image if exist.
        /// </summary>
        bool UseItemImagesForHighlightedItem
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if all items in the unfolded path should be displayed as highlighted.
        /// </summary>
        bool HighlightAllItemsInPath
        {
            get;
            set;
        }


        /// <summary>
        /// Contains a path to image that will be used on the right of every item that contains subitems.
        /// </summary>
        string SubmenuIndicator
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if alternating styles should be used for even and odd items in the same level of the menu.
        /// </summary>
        bool UseAlternatingStyles
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies prefix of standard CMSMenu CSS classes. You can also use several values separated with semicolon (;) for particular levels.
        /// </summary>    
        string CSSPrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if text can be wrapped or space is replaced with 'nbsp' entity.
        /// </summary>
        bool WordWrap
        {
            get;
            set;
        }


        /// <summary>
        /// Hides the control when no data is loaded. Default value is False.
        /// </summary>
        bool HideControlForZeroRows
        {
            get;
            set;
        }


        /// <summary>
        /// Text to be shown when the control is hidden by HideControlForZeroRows.
        /// </summary>        
        string ZeroRowsText
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to select, null or empty returns all columns.
        /// </summary>
        string Columns
        {
            get;
            set;
        }
    }
}