namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Enumeration that determines the syntax highlighting editor mode and its capabilities.
    /// </summary>
    public enum EditorModeEnum : int
    {
        /// <summary>
        /// No highlighting or advanced editing capabilities; supports static bookmarks.
        /// </summary>
        Basic = 0,


        /// <summary>
        /// Full code editor mode with syntax highlighting and advanced editing capabilities.
        /// </summary>
        Advanced = 1
    }
}