using CMS.Helpers;

namespace CMS.Modules
{
    /// <summary>
    /// UI element type 
    /// </summary>
    public enum UIElementTypeEnum
    {
        /// <summary>
        /// URL
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("Url")]
        Url,

        /// <summary>
        /// Page template
        /// </summary>
        [EnumStringRepresentation("PageTemplate")]
        PageTemplate,

        /// <summary>
        /// User control
        /// </summary>
        [EnumStringRepresentation("UserControl")]
        UserControl,

        /// <summary>
        /// Javascript
        /// </summary>
        [EnumStringRepresentation("Javascript")]
        Javascript
    }
}
