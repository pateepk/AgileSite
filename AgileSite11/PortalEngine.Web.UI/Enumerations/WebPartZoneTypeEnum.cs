using System;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part zone type enumeration which defines position of the web par zone used in the UI pages
    /// </summary>
    public enum WebPartZoneTypeEnum
    {
        /// <summary>
        /// The content zone.
        /// </summary>
        Content = 0,

        /// <summary>
        /// The header zone, uses a fixed positioning and is placed at the top of the page.
        /// </summary>
        Header = 1,

        /// <summary>
        /// The footer zone, uses a fixed positioning and is placed at the bottom of the page.
        /// </summary>
        Footer = 2,

        /// <summary>
        /// The dialog footer zone, uses a fixed positioning and is placed at the bottom of the page. This zone is displayed only in dialogs.
        /// </summary>
        DialogFooter = 3
    }
}
