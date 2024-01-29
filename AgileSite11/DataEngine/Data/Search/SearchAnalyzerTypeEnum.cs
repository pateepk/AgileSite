using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Search analyzer type enum.
    /// </summary>
    public enum SearchAnalyzerTypeEnum
    {
        /// <summary>
        /// SimpleAnalyzer - divides text at non-letters, lowercase.
        /// </summary>
        [EnumStringRepresentation("simple")]
        SimpleAnalyzer = 0,

        /// <summary>
        /// StopAnalyzer - divides text at non-letters, use of stop words.
        /// </summary>
        [EnumStringRepresentation("stop")]
        StopAnalyzer = 1,

        /// <summary>
        /// WhiteSpaceAnalyzer - divides text at whitespace, lowercase.
        /// </summary>
        [EnumStringRepresentation("whitespace")]
        WhiteSpaceAnalyzer = 2,

        /// <summary>
        /// StandardAnalyzer - grammar based, lowecase, good for most European-language documents.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("standard")]
        StandardAnalyzer = 3,

        /// <summary>
        /// "Tokenizes" the entire stream as a single token. This is useful for data like zip codes, ids, and some product names.
        /// </summary>
        [EnumStringRepresentation("keyword")]
        KeywordAnalyzer = 4,

        /// <summary>
        /// Custom analyzer.
        /// </summary>
        [EnumStringRepresentation("custom")]
        CustomAnalyzer = 5,

        /// <summary>
        /// Subset analyzer - whitespace, lowercase, searched text is processed as *word*.
        /// </summary>
        [EnumStringRepresentation("subset")]
        SubsetAnalyzer = 6,

        /// <summary>
        /// Starts with analyzer - whitespace, lowercase, searched text is processed as word*.
        /// </summary>
        [EnumStringRepresentation("startswith")]
        StartsWithanalyzer = 7,

        /// <summary>
        /// Simple analyzer with stemmer involved.
        /// </summary>
        [EnumStringRepresentation("simplewithstemming")]
        SimpleWithStemmingAnalyzer = 8,

        /// <summary>
        /// Stop word analyzer with stemmer involved.
        /// </summary>
        [EnumStringRepresentation("stopwithstemming")]
        StopWithStemmingAnalyzer = 9,
        
        /// <summary>
        /// Whitespace analyzer with stemmer involved.
        /// </summary>
        [EnumStringRepresentation("whitespacewithstemming")]
        WhitespaceWithStemmingAnalyzer = 10
    }
}
