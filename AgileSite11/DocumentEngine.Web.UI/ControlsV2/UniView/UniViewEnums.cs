using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Hierarchical display mode enumeration.
    /// </summary>
    public enum HierarchicalDisplayModeEnum
    {
        /// <summary>
        /// Inner mode.
        /// </summary>
        Inner = 0,

        /// <summary>
        /// Separate mode.
        /// </summary>
        Separate = 1
    }


    /// <summary>
    /// UniView item type enumeration. Contains item types of the templates.
    /// </summary>
    public enum UniViewItemType
    {
        /// <summary>
        /// All items.
        /// </summary>
        All = 0,

        /// <summary>
        /// Default item.
        /// </summary>
        Item = 1,

        /// <summary>
        /// Alternating item.
        /// </summary>
        AlternatingItem = 2,

        /// <summary>
        /// First item.
        /// </summary>
        FirstItem = 3,

        /// <summary>
        /// Last item.
        /// </summary>
        LastItem = 4,

        /// <summary>
        /// Header.
        /// </summary>
        Header = 5,

        /// <summary>
        /// Footer.
        /// </summary>
        Footer = 6,

        /// <summary>
        /// Separator.
        /// </summary>
        Separator = 7,

        /// <summary>
        /// Single item.
        /// </summary>
        SingleItem = 8,

        /// <summary>
        /// Current item (hierarchical only).
        /// </summary>
        CurrentItem = 9
    }
}