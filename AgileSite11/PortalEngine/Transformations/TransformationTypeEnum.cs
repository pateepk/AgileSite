using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Transformation type enumeration.
    /// </summary>
    public enum TransformationTypeEnum
    {
        /// <summary>
        /// ASCX transformation (for user controls).
        /// </summary>
        [EnumStringRepresentation("ascx")]
        Ascx,

        /// <summary>
        /// XSL transformation.
        /// </summary>
        [EnumStringRepresentation("xslt")]
        Xslt,

        /// <summary>
        /// General text transformation (with resolved macros).
        /// </summary>
        [EnumStringRepresentation("text")]
        Text,

        /// <summary>
        /// Html transformation (with resolved macros and WYSIWYG editing).
        /// </summary>
        [EnumStringRepresentation("html")]
        Html,

        /// <summary>
        /// jQuery transformation (with resolved macros)
        /// </summary>
        [EnumStringRepresentation("jquery")]
        jQuery
    }
}