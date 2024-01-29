using System;

using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Search mode enumeration.
    /// </summary>
    public enum SearchModeEnum
    {
        /// <summary>
        /// Searches for expressions containing exact phrase.
        /// </summary>
        [EnumStringRepresentation("exactphrase")]
        ExactPhrase = 0,

        /// <summary>
        /// Searches for expressions containing any of specified words.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("anyword")]
        AnyWord = 1,

        /// <summary>
        /// Searches for expressions containing all specified words.
        /// </summary>
        [EnumStringRepresentation("allwords")]
        AllWords = 2,

        /// <summary>
        /// Searches for expressions containing any of the specified words or their synonyms.
        /// </summary>
        [EnumStringRepresentation("anywordorsynonyms")]
        AnyWordOrSynonyms = 3,
    }
}