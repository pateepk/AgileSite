using System;

using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Indicates if all content or only its part should be searched.
    /// </summary>
    public enum SearchScopeEnum
    {
        /// <summary>
        /// All content search.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("searchallcontent")]
        SearchAllContent = 0,

        /// <summary>
        /// Search current section.
        /// </summary>
        [EnumStringRepresentation("searchcurrentsection")]
        SearchCurrentSection = 1,
    }
}