using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Field layout enumeration.
    /// </summary>
    public enum FieldLayoutEnum : int
    {
        /// <summary>
        /// Default layout.
        /// </summary>
        [EnumStringRepresentation("default")]
        Default = -1,

        /// <summary>
        /// Standard layout - No additional controls.
        /// </summary>
        [EnumStringRepresentation("inline")]
        Inline = 0,

        /// <summary>
        /// Two columns layout - label | editing + error.
        /// </summary>
        [EnumStringRepresentation("twocolumns")]
        TwoColumns,

        /// <summary>
        /// Three columns layout - label | editing | error. The error label in the third column is created automatically.
        /// </summary>
        [EnumStringRepresentation("threecolumns")]
        ThreeColumns
    }
}
