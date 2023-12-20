using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Represents search result retrieved by the search service.
    /// </summary>
    public sealed class SearchResult
    {
        private SearchParameters mParameters;

        private Regex mHighlightRegEx;

        private List<string> mHighlights;


        /// <summary>
        /// Collection of search results for the requested page number.
        /// </summary>
        public List<SearchResultItem> Items
        {
            get;
            set;
        } = new List<SearchResultItem>();


        /// <summary>
        /// Gets the total number of search results which can be retrieved from the search index (across all pages).
        /// </summary>
        public int TotalNumberOfResults
        {
            get;
            set;
        }


        /// <summary>
        /// Search parameters this search result is based upon.
        /// </summary>
        public SearchParameters Parameters
        {
            get
            {
                return mParameters;
            }
            set
            {
                mParameters = value;
                mHighlightRegEx = null;
            }
        }


        /// <summary>
        /// Gets a value that indicates whether this search was performed for all cultures or current culture combined with default culture.
        /// </summary>
        public bool MoreCultures
        {
            get
            {
                var parameters = Parameters;
                if (parameters == null)
                {
                    return false;
                }

                return parameters.SearchAllCultures;
            }
        }


        /// <summary>
        /// Specifies the list of highlighted words
        /// </summary>
        public List<string> Highlights
        {
            get
            {
                if (mHighlights == null)
                {
                    // Load highlighted words
                    string searchFor = mParameters.SearchFor;
                    if (!String.IsNullOrEmpty(searchFor))
                    {
                        // Get search clauses
                        var clauses = SearchManager.GetQueryClauses(searchFor);
                        if (clauses != null)
                        {
                            // Get collection of highlights
                            clauses.GetQuery(false, true);
                            mHighlights = clauses.HighlightedWords;
                        }
                    }
                }

                return mHighlights;
            }
            set
            {
                mHighlights = value;
            }
        }


        /// <summary>
        /// Gets the regular expression specified for highlighting.
        /// </summary>
        public Regex HighlightRegex
        {
            get
            {
                if ((mHighlightRegEx == null) && (mParameters != null))
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
                                if (word.EndsWith("*", StringComparison.Ordinal))
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
                            mHighlightRegEx = tmpRegex;
                        }
                        else
                        {
                            mHighlightRegEx = null;
                        }
                    }
                }

                // Return regex collection
                return mHighlightRegEx;
            }
        }


        /// <summary>
        /// Search error associated with this results, if any occurred during the search operation.
        /// Results of a failed search are not valid.
        /// </summary>
        public Exception LastError
        {
            get;
            set;
        }
    }
}
