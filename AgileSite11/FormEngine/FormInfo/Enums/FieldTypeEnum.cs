using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Field types enum.
    /// </summary>
    public enum FieldTypeEnum
    {
        /// <summary>
        /// Standard field type.
        /// </summary>
        [EnumStringRepresentation("standard")]
        Standard,


        /// <summary>
        /// Primary key field type.
        /// </summary>
        [EnumStringRepresentation("primary")]
        Primary,


        /// <summary>
        /// Document field type.
        /// </summary>
        [EnumStringRepresentation("document")]
        Document,


        /// <summary>
        /// Dummy field type.
        /// </summary>
        [EnumStringRepresentation("dummy")]
        Dummy,


        /// <summary>
        /// Extra field type.
        /// </summary>
        [EnumStringRepresentation("extra")]
        Extra,
    }
}