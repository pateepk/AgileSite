using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part management type enumeration.
    /// </summary>
    internal enum WebPartManagementEnum
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// No web part management.
        /// </summary>
        None = 0,

        /// <summary>
        /// All managements (both widgets and web parts).
        /// </summary>
        All = 1,

        /// <summary>
        /// Widgets only.
        /// </summary>
        Widgets = 2
    }
}
