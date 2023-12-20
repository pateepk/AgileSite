namespace CMS.Search
{
    
    /// <summary>
    /// Helper methods for search condition syntax
    /// </summary>
    public interface ISearchSyntaxHelper
    {
        #region "Methods"

        /// <summary>
        /// Returns true if the given search condition is empty
        /// </summary>
        /// <param name="condition">Search condition to check</param>
        bool IsEmptyCondition(string condition);


        /// <summary>
        /// Adds the given search condition to the existing condition
        /// </summary>
        /// <param name="original">Original condition</param>
        /// <param name="add">Condition to add</param>
        string AddSearchCondition(string original, string add);


        /// <summary>
        /// Gets the range expression
        /// </summary>
        /// <param name="from">From value</param>
        /// <param name="to">To value. If not specified, the range covers only the from value.</param>
        string GetRange(object from, object to = null);


        /// <summary>
        /// Gets the search condition for the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="val">Field value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        string GetFieldCondition(string fieldName, object val, System.Nullable<System.Boolean> valueMatch = true);


        /// <summary>
        /// Returns condition for search filter row
        /// </summary>
        /// <param name="row">Filter row</param>
        /// <param name="value">Filter row value</param>
        string GetFilterCondition(string row, string value);


        /// <summary>
        /// Gets the exact phrase condition from the given phrase
        /// </summary>
        /// <param name="phrase">Phrase to convert</param>
        string GetExactPhraseCondition(string phrase);


        /// <summary>
        /// Gets a required condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        string GetRequiredCondition(string condition);


        /// <summary>
        /// Gets a not (except) condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        string GetNotCondition(string condition);


        /// <summary>
        /// Groups the expressions
        /// </summary>
        /// <param name="expressions">Inner group expressions</param>
        string GetGroup(string[] expressions);


        /// <summary>
        /// Expands given search expression with synonyms. If the data base of synonyms for given language is not found, searchExpression is returned without any modifications.
        /// </summary>
        /// <param name="searchExpression">Search expression which should be expanded with synonyms</param>
        /// <param name="culture">Language code of the search expression (if null, en-us is used)</param>
        string ExpandWithSynonyms(string searchExpression, string culture);


        /// <summary>
        /// Adds ~ signs to each term to force fuzzy search.
        /// </summary>
        /// <param name="searchExpression">Search expression to transform</param>
        string TransformToFuzzySearch(string searchExpression);


        /// <summary>
        /// Escapes the key words to be searched
        /// </summary>
        /// <param name="keywords">Keywords</param>
        string EscapeKeyWords(string keywords);


        /// <summary>
        /// Gets the field condition for a range of values
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        string GetFieldCondition(string fieldName, object from, object to, System.Nullable<System.Boolean> valueMatch = true);


        /// <summary>
        /// Returns string with combined search index conditions.
        /// </summary>
        /// <param name="keyWords">Search keywords, separated by space</param>
        /// <param name="searchCondition">Search condition</param>
        string CombineSearchCondition(string keyWords, CMS.Search.SearchCondition searchCondition);


        /// <summary>
        /// Returns modified keywords string based on searchOptions.
        /// </summary>
        /// <param name="keywords">String with keyword</param>
        /// <param name="searchOptions">Search options specifies encoding</param>
        string ProcessSearchKeywords(string keywords, CMS.Search.SearchOptionsEnum searchOptions);

        #endregion
    }
}
