using System.ComponentModel;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Type of the icon displayed within the <see cref="TileIconModel"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum TileIconTypeEnum
    {
        /// <summary>
        /// Icon is defined in the CSS style sheet.
        /// </summary>
        CssClass,

        /// <summary>
        /// Icon is defined as image.
        /// </summary>
        Image,
    }
}