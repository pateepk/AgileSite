namespace CMS.Search
{
    
    /// <summary>
    /// Helper methods for search condition syntax
    /// </summary>
    public class SearchSyntaxHelper : CMS.Core.StaticWrapper<ISearchSyntaxHelper>
    {
        #region "Methods"

        /// <summary>
        /// Returns true if the given search condition is empty
        /// </summary>
        /// <param name="condition">Search condition to check</param>
        public static bool IsEmptyCondition(string condition)
        {
            return Implementation.IsEmptyCondition(condition);
        }


        /// <summary>
        /// Adds the given search condition to the existing condition
        /// </summary>
        /// <param name="original">Original condition</param>
        /// <param name="add">Condition to add</param>
        public static string AddSearchCondition(string original, string add)
        {
            return Implementation.AddSearchCondition(original, add);
        }


        /// <summary>
        /// Gets the range expression
        /// </summary>
        /// <param name="from">From value</param>
        /// <param name="to">To value. If not specified, the range covers only the from value.</param>
        public static string GetRange(object from, object to = null)
        {
            return Implementation.GetRange(from, to);
        }


        /// <summary>
        /// Gets the search condition for the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="val">Field value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        public static string GetFieldCondition(string fieldName, object val, System.Nullable<System.Boolean> valueMatch = true)
        {
            return Implementation.GetFieldCondition(fieldName, val, valueMatch);
        }


        /// <summary>
        /// Returns condition for search filter row
        /// </summary>
        /// <param name="row">Filter row</param>
        /// <param name="value">Filter row value</param>
        public static string GetFilterCondition(string row, string value)
        {
            return Implementation.GetFilterCondition(row, value);
        }


        /// <summary>
        /// Gets the exact phrase condition from the given phrase
        /// </summary>
        /// <param name="phrase">Phrase to convert</param>
        public static string GetExactPhraseCondition(string phrase)
        {
            return Implementation.GetExactPhraseCondition(phrase);
        }


        /// <summary>
        /// Gets a required condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        public static string GetRequiredCondition(string condition)
        {
            return Implementation.GetRequiredCondition(condition);
        }


        /// <summary>
        /// Gets a not (except) condition from the given condition
        /// </summary>
        /// <param name="condition">Condition to convert to required</param>
        public static string GetNotCondition(string condition)
        {
            return Implementation.GetNotCondition(condition);
        }


        /// <summary>
        /// Groups the expressions
        /// </summary>
        /// <param name="expressions">Inner group expressions</param>
        public static string GetGroup(string[] expressions)
        {
            return Implementation.GetGroup(expressions);
        }


        /// <summary>
        /// Expands given search expression with synonyms. If the data base of synonyms for given language is not found, searchExpression is returned without any modifications.
        /// </summary>
        /// <param name="searchExpression">Search expression which should be expanded with synonyms</param>
        /// <param name="culture">Language code of the search expression (if null, en-us is used)</param>
        public static string ExpandWithSynonyms(string searchExpression, string culture)
        {
            return Implementation.ExpandWithSynonyms(searchExpression, culture);
        }


        /// <summary>
        /// Adds ~ signs to each term to force fuzzy search.
        /// </summary>
        /// <param name="searchExpression">Search expression to transform</param>
        public static string TransformToFuzzySearch(string searchExpression)
        {
            return Implementation.TransformToFuzzySearch(searchExpression);
        }


        /// <summary>
        /// Escapes the key words to be searched
        /// </summary>
        /// <param name="keywords">Keywords</param>
        public static string EscapeKeyWords(string keywords)
        {
            return Implementation.EscapeKeyWords(keywords);
        }


        /// <summary>
        /// Gets the field condition for a range of values
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        /// <param name="valueMatch">Defines if the condition is required. True means required, false means all except, null means default operator (typically optional)</param>
        public static string GetFieldCondition(string fieldName, object from, object to, System.Nullable<System.Boolean> valueMatch = true)
        {
            return Implementation.GetFieldCondition(fieldName, from, to, valueMatch);
        }


        /// <summary>
        /// Returns string with combined search index conditions.
        /// </summary>
        /// <param name="keyWords">Search keywords, separated by space</param>
        /// <param name="searchCondition">Search condition</param>
        public static string CombineSearchCondition(string keyWords, CMS.Search.SearchCondition searchCondition)
        {
            return Implementation.CombineSearchCondition(keyWords, searchCondition);
        }


        /// <summary>
        /// Returns modified keywords string based on searchOptions.
        /// </summary>
        /// <param name="keywords">String with keyword</param>
        /// <param name="searchOptions">Search options specifies encoding</param>
        public static string ProcessSearchKeywords(string keywords, CMS.Search.SearchOptionsEnum searchOptions)
        {
            return Implementation.ProcessSearchKeywords(keywords, searchOptions);
        }

        #endregion
    }
}
