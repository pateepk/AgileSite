using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search
{
    using SearchResultsTable = SafeDictionary<string, DataRow>;
    using SearchDocumentsTable = SafeDictionary<string, ILuceneSearchDocument>;

    /// <summary>
    /// Search context.
    /// </summary>
    public class SearchContext : AbstractContext<SearchContext>
    {
        #region "Variables"

        private SearchResultsTable mCurrentSearchResults;

        private SearchDocumentsTable mCurrentSearchDocuments;

        private SearchParameters mCurrentParameters;

        private Exception mLastError;

        private Regex mHighlightRegEx;

        private List<string> mHighlights;

        private object[] mCurrentHighlight;

        private bool? mIsCrawler;

        #endregion


        #region "Public properties"


        /// <summary>
        /// Whether request is based on page crawler index.
        /// </summary>
        internal static bool? IsCrawler
        {
            get
            {
                return Current.mIsCrawler;
            }

            set
            {
                Current.mIsCrawler = value;
            }
        }


        /// <summary>
        /// Current highlight data
        /// </summary>
        internal static object[] CurrentHighlight
        {
            get
            {
                return Current.mCurrentHighlight;
            }
            set
            {
                Current.mCurrentHighlight = value;
            }
        }


        /// <summary>
        /// Current search parameters
        /// </summary>
        public static SearchParameters CurrentParameters
        {
            get
            {
                return Current.mCurrentParameters;
            }
            set
            {
                var c = Current;

                c.mCurrentParameters = value;
                c.mHighlightRegEx = null;
            }
        }


        /// <summary>
        /// Current search results
        /// </summary>
        public static SearchResultsTable CurrentSearchResults
        {
            get
            {
                return Current.mCurrentSearchResults;
            }
            set
            {
                Current.mCurrentSearchResults = value;
            }
        }


        /// <summary>
        /// Current search documents
        /// </summary>
        public static SearchDocumentsTable CurrentSearchDocuments
        {
            get
            {
                return Current.mCurrentSearchDocuments;
            }
            set
            {
                Current.mCurrentSearchDocuments = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current search is performed for all cultures or combine current culture with default.
        /// </summary>
        public static bool MoreCultures
        {
            get
            {
                var parameters = CurrentParameters;
                if (parameters == null)
                {
                    return false;
                }

                return parameters.SearchAllCultures;
            }
        }


        /// <summary>
        /// Gets the last search error.
        /// </summary>
        public static Exception LastError
        {
            get
            {
                return Current.mLastError;
            }
            set
            {
                Current.mLastError = value;
            }
        }


        /// <summary>
        /// Specifies the list of highlighted words
        /// </summary>
        public static List<string> Highlights
        {
            get
            {
                var c = Current;

                if (c.mHighlights == null)
                {
                    // Load highlighted words
                    string searchFor = c.mCurrentParameters.SearchFor;
                    if (!String.IsNullOrEmpty(searchFor))
                    {
                        // Get search clauses
                        var clauses = SearchManager.GetQueryClauses(searchFor);
                        if (clauses != null)
                        {
                            // Get collection of highlights
                            clauses.GetQuery(false, true);
                            c.mHighlights = clauses.HighlightedWords;
                        }
                    }
                }

                return c.mHighlights;
            }
            set
            {
                Current.mHighlights = value;
            }
        }


        /// <summary>
        /// Gets the regular expression specified for highlighting.
        /// </summary>
        public static Regex HighlightRegex
        {
            get
            {
                // Get list of regular expressions from request storage
                var c = Current;

                if ((c.mHighlightRegEx == null) && (c.mCurrentParameters != null))
                {
                    // Check whether collection with highlighted words exist
                    var highlights = Highlights;
                    if (highlights != null)
                    {
                        // Regex value
                        string regexq = String.Empty;

                        // Loop thru all words and create reg. expression
                        foreach (string word in highlights)
                        {
                            if (!String.IsNullOrEmpty(word))
                            {
                                if (word.EndsWithCSafe("*"))
                                {
                                    regexq += "(^|\\W)(?<selected>" + Regex.Escape(word).Replace("\\*", "\\w*").Replace("\\?", ".{1}") + ")|";
                                }
                                else
                                {
                                    regexq += "(^|\\W)(?<selected>" + Regex.Escape(word).Replace("\\*", "\\w*").Replace("\\?", ".{1}") + ")(?=[\\W]|$)|";
                                }
                            }
                        }

                        // Remove final
                        regexq = regexq.TrimEnd('|');

                        // Create temporary Reg. expression
                        Regex tmpRegex = RegexHelper.GetRegex(regexq, true);

                        // Set current highlights
                        if (!String.IsNullOrEmpty(regexq))
                        {
                            c.mHighlightRegEx = tmpRegex;
                        }
                        else
                        {
                            c.mHighlightRegEx = null;
                        }
                    }
                }

                // Return regex collection
                return c.mHighlightRegEx;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns column value for current search result item.
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="columnName">Column name</param>
        public static object GetSearchValue(string id, string columnName)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var c = Current;

                // Check whether id and data row collection exists
                var resultRows = c.mCurrentSearchResults;
                if (resultRows != null)
                {
                    // Check whether data row exists and contains required column
                    var dr = resultRows[id];
                    if ((dr != null) && (dr.Table.Columns.Contains(columnName)))
                    {
                        // Return column value
                        return dr[columnName];
                    }
                }

                // Get the current search document
                var docs = c.mCurrentSearchDocuments;
                if (docs != null)
                {
                    var doc = docs[id];
                    if (doc != null)
                    {
                        return doc.Get(columnName.ToLowerCSafe());
                    }
                }
            }

            // Return nothing by default
            return null;
        }


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            // Share this object from the main thread
            return this;
        }

        #endregion
    }
}