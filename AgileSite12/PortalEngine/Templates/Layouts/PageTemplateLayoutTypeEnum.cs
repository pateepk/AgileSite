using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Layout types enum
    /// </summary>
    public enum PageTemplateLayoutTypeEnum
    {
        /// <summary>
        /// Shared layout
        /// </summary>
        SharedLayout,
        
        /// <summary>
        /// Shared layout mapped
        /// </summary>
        SharedLayoutMapped,

        /// <summary>
        /// Custom page template layout
        /// </summary>
        PageTemplateLayout,

        /// <summary>
        /// Shared layout defined in device layout
        /// </summary>
        DeviceSharedLayout,

        /// <summary>
        /// Custom device layout
        /// </summary>
        DeviceLayout
    }
}
