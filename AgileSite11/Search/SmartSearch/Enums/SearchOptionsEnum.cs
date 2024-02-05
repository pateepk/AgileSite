using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Search task status enumeration.
    /// </summary>
    public enum SearchOptionsEnum
    {
        /// <summary>
        /// No special behaviour, all special character will be escaped.
        /// </summary>
        [EnumStringRepresentation("nonesearch")]
        NoneSearch = 0,

        /// <summary>
        /// Special character ":" will be escaped.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("basicsearch")]
        BasicSearch = 1,

        /// <summary>
        /// Search with all possibilities, no escaping.
        /// </summary>
        [EnumStringRepresentation("fullsearch")]
        FullSearch = 2,
    }
}