using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Constants for DataQuery operators
    /// </summary>
    public enum QueryOperator
    {
        /// <summary>
        /// Equals "="
        /// </summary>
        [EnumStringRepresentation("=")]
        Equals,

        /// <summary>
        /// Not equals "&lt;&gt;"
        /// </summary>
        [EnumStringRepresentation("<>")]
        NotEquals,

        /// <summary>
        /// Like "LIKE"
        /// </summary>
        [EnumStringRepresentation("LIKE")]
        Like,

        /// <summary>
        /// Not line "NOT LIKE"
        /// </summary>
        [EnumStringRepresentation("NOT LIKE")]
        NotLike,

        /// <summary>
        /// Greater than "&gt;"
        /// </summary>
        [EnumStringRepresentation(">")]
        GreaterThan,

        /// <summary>
        /// Larger than "&gt;"
        /// </summary>
        [EnumStringRepresentation(">")]
        LargerThan = GreaterThan,

        /// <summary>
        /// Less than "&lt;"
        /// </summary>
        [EnumStringRepresentation("<")]
        LessThan,

        /// <summary>
        /// Greater or equals "&gt;="
        /// </summary>
        [EnumStringRepresentation(">=")]
        GreaterOrEquals,

        /// <summary>
        /// Larger or equals "&gt;="
        /// </summary>
        [EnumStringRepresentation(">=")]
        LargerOrEquals = GreaterOrEquals,

        /// <summary>
        /// Less or equals "&lt;="
        /// </summary>
        [EnumStringRepresentation("<=")]
        LessOrEquals
    }
}
