using System;

using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page template type enumeration.
    /// </summary>
    public enum PageTemplateTypeEnum
    {
        /// <summary>
        /// Unknown type.
        /// </summary>   
        [EnumDefaultValue()]
        [EnumStringRepresentation("unknown")]
        Unknown = 0,

        /// <summary>
        /// Portal page template.
        /// </summary>       
        [EnumStringRepresentation("portal")]
        Portal = 1,

        /// <summary>
        /// ASPX page template.
        /// </summary>
        [EnumStringRepresentation("aspx")]
        Aspx = 2,

        /// <summary>
        /// ASPX page template with portal enabled.
        /// </summary>
        [EnumStringRepresentation("aspxportal")]
        AspxPortal = 3,

        /// <summary>
        /// Dashboard template.
        /// </summary>
        [EnumStringRepresentation("dashboard")]
        Dashboard = 4,

        /// <summary>
        /// MVC template
        /// </summary>
        [EnumStringRepresentation("mvc")]
        MVC = 5,

        /// <summary>
        /// UI template
        /// </summary>
        [EnumStringRepresentation("ui")]
        UI = 6,
    }
}