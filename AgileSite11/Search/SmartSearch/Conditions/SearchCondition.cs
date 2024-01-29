namespace CMS.Search
{
    /// <summary>
    /// Defines search condition parameters
    /// </summary>
    /// <seealso cref="SearchSyntaxHelper.CombineSearchCondition(string, SearchCondition)"/>
    public class SearchCondition
    {
        /// <summary>
        /// Search condition that is added to any conditions specified in the search expression. You can use special characters (+ -) and field conditions (e.g. +documentnodeid:(int)255).
        /// </summary>
        public string ExtraConditions
        {
            get;
            set;
        }


        /// <summary>
        /// Search mode
        /// </summary>
        public SearchModeEnum SearchMode
        {
            get;
            set;
        }


        /// <summary>
        /// Search options
        /// </summary>
        public SearchOptionsEnum SearchOptions
        {
            get;
            set;
        }


        /// <summary>
        /// Search condition for documents
        /// </summary>
        public DocumentSearchCondition DocumentCondition
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the expression is transformed to support fuzzy search
        /// </summary>
        public bool FuzzySearch
        {
            get;
            set;
        }
        
        
        /// <param name="extraConditions">Search condition that is added to any conditions specified in the search expression. You can use special characters (+ -) and field conditions (e.g. +documentnodeid:(int)255).</param>
        /// <param name="searchMode">Search mode</param>
        /// <param name="searchOptions">Search options</param>
        /// <param name="documentCondition">Search condition for documents</param>
        /// <param name="fuzzySearch">If true, the expression is transformed to support fuzzy search</param>
        public SearchCondition(string extraConditions = null, SearchModeEnum searchMode = SearchModeEnum.AnyWord, SearchOptionsEnum searchOptions = SearchOptionsEnum.BasicSearch, DocumentSearchCondition documentCondition = null, bool fuzzySearch = false)
        {
            ExtraConditions = extraConditions;
            SearchMode = searchMode;
            SearchOptions = searchOptions;
            DocumentCondition = documentCondition;
            FuzzySearch = fuzzySearch;
        }
    }
}