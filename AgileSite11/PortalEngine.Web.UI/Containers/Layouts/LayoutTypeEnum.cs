using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Zone layout type enumeration
    /// </summary>
    public enum ZoneLayoutTypeEnum : int
    {
        /// <summary>
        /// Standard layout, elements are rendered as they are
        /// </summary>
        Standard = 0,

        /// <summary>
        /// All elements float to left
        /// </summary>
        FloatLeft = 1,

        /// <summary>
        /// All elements float to right
        /// </summary>
        FloatRight = 2,

        /// <summary>
        /// Free layout - Element are positioned absolutely
        /// </summary>
        Free = 3
    }
}
