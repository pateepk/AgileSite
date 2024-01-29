using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Class with information about clauses and keywords.
    /// </summary>
    public class SearchQueryClauses
    {
        #region "Protected variables"

        /// <summary>
        /// Collection of 'AND' clauses.
        /// </summary>
        protected List<object> andClauses = null;

        /// <summary>
        /// Collection of 'AND NOT' clauses.
        /// </summary>
        protected List<object> andNotClauses = null;

        /// <summary>
        /// Collection of 'OR' clauses.
        /// </summary>
        protected List<object> orClauses = null;

        /// <summary>
        /// Parent clause object.
        /// </summary>
        private SearchQueryClauses mParent = null;

        /// <summary>
        /// List of words which should be highlighted.
        /// </summary>
        private List<string> mHighlightedWords = null;

        /// <summary>
        /// Non-alpha numeric characters regex.
        /// </summary>
        private static Regex mNonAlphaNumericRegex = null;

        /// <summary>
        /// Exact phrase regular expression.
        /// </summary>
        private static Regex mExactRegex = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the regular expression for exact phrase.
        /// </summary>
        private static Regex ExactRegex
        {
            get
            {
                if (mExactRegex == null)
                {
                    mExactRegex = RegexHelper.GetRegex("^\".*?\"$");
                }
                return mExactRegex;
            }
        }


        /// <summary>
        /// Gets the non-alpha numeric characters regex.
        /// </summary>
        private static Regex NonAlphaNumericRegex
        {
            get
            {
                if (mNonAlphaNumericRegex == null)
                {
                    mNonAlphaNumericRegex = RegexHelper.GetRegex("\\W+");
                }

                return mNonAlphaNumericRegex;
            }
        }


        /// <summary>
        /// Gets the list of words which should be highlighted.
        /// </summary>
        public List<string> HighlightedWords
        {
            get
            {
                return mHighlightedWords;
            }
        }


        /// <summary>
        /// Gets or sets the parent of the current object.
        /// </summary>
        public SearchQueryClauses Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Add clause to current object.
        /// </summary>
        /// <param name="wordOrQueryToken">KeyWord or SearchQueryToken child</param>
        /// <param name="type">Clause type</param>
        public void AddClause(object wordOrQueryToken, ClauseTypeEnum type)
        {
            switch (type)
            {
                    // OR clause
                case ClauseTypeEnum.OR:
                    if (orClauses == null)
                    {
                        orClauses = new List<object>();
                    }
                    orClauses.Add(wordOrQueryToken);
                    break;

                    // AND clause
                case ClauseTypeEnum.AND:
                    if (andClauses == null)
                    {
                        andClauses = new List<object>();
                    }
                    andClauses.Add(wordOrQueryToken);
                    break;

                    // AND NOT clause
                case ClauseTypeEnum.AND_NOT:
                    if (andNotClauses == null)
                    {
                        andNotClauses = new List<object>();
                    }
                    andNotClauses.Add(wordOrQueryToken);
                    break;
            }
        }


        /// <summary>
        /// Returns Full text SQL query.
        /// </summary>
        public string GetQuery()
        {
            return GetQuery(true, true);
        }


        /// <summary>
        /// Returns Full text SQL query and fill collection of words which should be highlighted.
        /// </summary>
        /// <param name="generateQuery">Indicates whether query should be generated</param>
        /// <param name="causeType">If is true => positive cause (AND/OR), false => negative cause (NOT)</param>
        public string GetQuery(bool generateQuery, bool causeType)
        {
            return GetQuery(generateQuery, causeType, new List<string>());
        }


        /// <summary>
        /// Returns Full text SQL query and fill collection of words which should be highlighted.
        /// </summary>
        /// <param name="generateQuery">Indicates whether query should be generated</param>
        /// <param name="causeType">If is true => positive cause (AND/OR), false => negative cause (NOT)</param>
        /// <param name="highligtedWords">Collection of words which should be highlighted</param>
        public string GetQuery(bool generateQuery, bool causeType, List<string> highligtedWords)
        {
            string currentQuery = String.Empty;
            bool queryGenerated = false;


            #region "OR"

            if (orClauses != null)
            {
                // Loop throu all words
                foreach (object obj in orClauses)
                {
                    // Check whether current object is sub-clause
                    if (obj is SearchQueryClauses)
                    {
                        currentQuery = ValidationHelper.GetString(((SearchQueryClauses)obj).GetQuery(generateQuery, causeType, highligtedWords), String.Empty).Trim();
                        if (!String.IsNullOrEmpty(currentQuery))
                        {
                            queryGenerated = true;
                        }
                    }
                    // Current object is word
                    else if ((obj != null) && (obj.ToString().Trim() != String.Empty))
                    {
                        // Generate SQL query part
                        if (generateQuery)
                        {
                            currentQuery += ProcessKeyWord(obj) + " OR ";
                            queryGenerated = true;
                        }

                        // If it is positive type, add current word to the highlighted collection
                        if ((highligtedWords != null) && (causeType))
                        {
                            ProcessHighlightedWord(obj, highligtedWords);
                        }
                    }
                }

                // Remove final 'OR' and create parenthesis envelope
                if (queryGenerated)
                {
                    currentQuery = "(" + currentQuery.Substring(0, currentQuery.Length - 4) + ")";
                }
            }

            #endregion


            #region "AND"

            if (andClauses != null)
            {
                // Loop throu all words
                foreach (object obj in andClauses)
                {
                    if (queryGenerated)
                    {
                        currentQuery += " AND ";
                    }

                    // Check whether current object is sub-clause
                    if (obj is SearchQueryClauses)
                    {
                        string subQuery = ValidationHelper.GetString(((SearchQueryClauses)obj).GetQuery(generateQuery, causeType, highligtedWords), String.Empty).Trim();
                        if (!String.IsNullOrEmpty(subQuery))
                        {
                            currentQuery += "(" + subQuery + ")";
                        }
                        queryGenerated = true;
                    }
                    // Current object is word
                    else if ((obj != null) && (obj.ToString().Trim() != String.Empty))
                    {
                        // Generate SQL query part
                        if (generateQuery)
                        {
                            currentQuery += ProcessKeyWord(obj);
                            queryGenerated = true;
                        }

                        // If it is positive type, add current word to the highlighted collection
                        if ((highligtedWords != null) && (causeType))
                        {
                            ProcessHighlightedWord(obj, highligtedWords);
                        }
                    }

                    queryGenerated = true;
                }
            }

            #endregion


            #region "AND NOT"

            if (andNotClauses != null)
            {
                // Loop throu all words
                foreach (object obj in andNotClauses)
                {
                    if (queryGenerated)
                    {
                        currentQuery += " AND NOT ";
                    }
                    else
                    {
                        // Only negative clauses aren't valid for search syntax
                        return String.Empty;
                    }

                    // Check whether current object is sub-clause
                    if (obj is SearchQueryClauses)
                    {
                        string subQuery = ValidationHelper.GetString(((SearchQueryClauses)obj).GetQuery(generateQuery, !causeType, highligtedWords), String.Empty).Trim();
                        if (!String.IsNullOrEmpty(subQuery))
                        {
                            currentQuery += "(" + subQuery + ")";
                        }
                        queryGenerated = true;
                    }
                    // Current object is word
                    else if ((obj != null) && (obj.ToString().Trim() != String.Empty))
                    {
                        // Generate SQL query part
                        if (generateQuery)
                        {
                            currentQuery += ProcessKeyWord(obj);
                            queryGenerated = true;
                        }

                        // If it is positive type, add current word to the highlighted collection
                        if ((highligtedWords != null) && (!causeType))
                        {
                            ProcessHighlightedWord(obj, highligtedWords);
                        }
                    }

                    queryGenerated = true;
                }
            }

            #endregion


            // Add highlighted words only for the highest object
            if (Parent == null)
            {
                mHighlightedWords = highligtedWords;
                if (ValidationHelper.GetString(currentQuery, String.Empty).Trim() != String.Empty)
                {
                    return "'" + currentQuery + "'";
                }

                return String.Empty;
            }

            return currentQuery;
        }


        /// <summary>
        /// Returns string keyword without SQL Injection vulnerability.
        /// </summary>
        /// <param name="obj">Input object</param>
        private string ProcessKeyWord(object obj)
        {
            if (obj != null)
            {
                string result = obj.ToString().Trim();
                // Sets the flag which indicates whether current keyword is exact phrase
                bool isExact = ExactRegex.IsMatch(result);
                // Remove apostrophe
                result = result.Replace("'", String.Empty);
                result = result.Replace("\"", String.Empty);

                // Remove others non alpha numeric characters
                if (NonAlphaNumericRegex.IsMatch(result))
                {
                    result = NonAlphaNumericRegex.Replace(result, " ");
                    // Full text syntax requires double quotes if keyword contains white space
                    isExact = true;
                }

                // Check whether result is defined, otherwise set fake result
                if (String.IsNullOrEmpty(result))
                {
                    result = "wrdremovedunsearchable";
                }

                return "\"" + result + "\"";
            }

            return String.Empty;
        }


        /// <summary>
        /// Adds keyword to the collection of highlighted words (with and without diacritics)
        /// </summary>
        /// <param name="obj">Input object</param>]
        /// <param name="collection">Collection of highlighted words</param>
        private void ProcessHighlightedWord(object obj, List<string> collection)
        {
            if (obj != null)
            {
                string value = obj.ToString().Trim().TrimStart('"').TrimEnd('"');
                collection.Add(value);

                string invariantkeyword = TextHelper.RemoveDiacritics(value);
                if (!value.Equals(invariantkeyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    collection.Add(invariantkeyword);
                }
            }
        }

        #endregion
    }
}