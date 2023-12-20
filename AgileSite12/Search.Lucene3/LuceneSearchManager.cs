using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Search;
using CMS.Search.Lucene3;

using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

using Directory = Lucene.Net.Store.Directory;
using Query = Lucene.Net.Search.Query;
using Version = Lucene.Net.Util.Version;

using IOExceptions = System.IO;

[assembly: RegisterImplementation(typeof(ISearchManager), typeof(LuceneSearchManager), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Helper classes to provide search indexes
    /// </summary>
    internal class LuceneSearchManager : ISearchManager
    {
        #region "Variables"

        /// <summary>
        /// Sort prefix regular expression.
        /// </summary>
        private static Regex mSortPrefixRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the sort field type syntax regular expression.
        /// Result contains type, name and order groups.
        /// </summary>
        public static Regex SortPrefixRegex
        {
            get
            {
                return mSortPrefixRegex ?? (mSortPrefixRegex = RegexHelper.GetRegex("(?<type>\\(.*?\\))*\\s*(?<name>[^\\s]*)\\s*(?<order>.*)"));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates Lucene document.
        /// </summary>
        public ILuceneSearchDocument CreateDocument()
        {
            return new LuceneSearchDocument();
        }


        /// <summary>
        /// Creates the index writer for the given path and Analyzer
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="path">Index path</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="create">If true, the writer is created</param>
        public virtual IIndexWriter CreateIndexWriter(string path, ISearchAnalyzer analyzer, bool create)
        {
            IIndexWriter iw;
            var a = (LuceneSearchAnalyzer)analyzer;

            // Get index writer for current analyzer
            try
            {
                iw = new LuceneIndexWriter(new SearchDirectory(path), a.Analyzer, create);
            }
            // If file not found exception was thrown, forcibly create index
            catch (IOExceptions.FileNotFoundException)
            {
                iw = new LuceneIndexWriter(new SearchDirectory(path), a.Analyzer, true);
            }

            return iw;
        }


        /// <summary>
        /// Creates the index searcher
        /// </summary>
        /// <param name="path">Index path</param>
        public virtual IIndexSearcher CreateIndexSearcher(string path)
        {
            var dir = CMS.IO.DirectoryInfo.New(path);
            if ((!dir.Exists) || (dir.GetFiles().Length == 0))
            {
                return null;
            }

            // Searcher object
            IIndexSearcher searcher;

            try
            {
                searcher = new LuceneIndexSearcher(new SearchDirectory(path));
            }
            catch
            {
                return null;
            }

            return searcher;
        }


        /// <summary>
        /// Returns current object analyzer.
        /// </summary>
        /// <param name="sii">Search index info</param>
        /// <param name="isSearch">Indicates whether analyzer should be used for search or indexing</param>
        public virtual ISearchAnalyzer CreateAnalyzer(SearchIndexInfo sii, bool isSearch)
        {
            return new LuceneSearchAnalyzer(sii, isSearch);
        }


        /// <summary>
        /// Returns analyzer that can be used on searching over multiple indexes
        /// </summary>
        /// <param name="indexes">Search index infos</param>
        public virtual ISearchAnalyzer CreateAnalyzer(params SearchIndexInfo[] indexes)
        {
            if (indexes.Length == 0)
            {
                throw new ArgumentException("[LuceneSearchManager.CreateAnalyzer]: Indexes array must contain at least one index.", "indexes");
            }

            return new LuceneSearchAnalyzer(indexes);
        }


        /// <summary>
        /// Creates the defined search filter
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="match">Match value</param>
        /// <param name="condition">Filter condition</param>
        public ISearchFilter CreateFilter(string fieldName, string match, Func<string, string, bool> condition)
        {
            return new LuceneSearchFilter(fieldName, match, condition);
        }


        /// <summary>
        /// With dependence on current sort condition returns Sort object, supports ##SCORE## macro.
        /// </summary>
        /// <param name="sortCondition">Sort condition</param>
        public static Sort GetSort(string sortCondition)
        {
            // Check whether condition is defined
            if (!String.IsNullOrEmpty(sortCondition))
            {
                // Collection of sort fields
                List<SortField> sortFields = new List<SortField>();
                // Split fields by delimiter
                string[] fields = sortCondition.Split(',');

                // Loop thru all fields and get field name and order
                foreach (string field in fields)
                {
                    if (!String.IsNullOrEmpty(field))
                    {
                        // Get current order match
                        Match mtch = SortPrefixRegex.Match(field.Trim().ToLowerCSafe());

                        // Sort field type
                        string type = mtch.Groups["type"].Value;
                        // Sort field name
                        string name = mtch.Groups["name"].Value;
                        // Indicates whether current sort order is reversible (DESC)
                        bool reverseOrder = (mtch.Groups["order"].Value == "desc");

                        // Special case for score ordering
                        if (name == "##score##")
                        {
                            sortFields.Add(SortField.FIELD_SCORE);
                        }
                        // Order by existing document field
                        else
                        {
                            // Current sort field type
                            int sortType = SortField.STRING;

                            // Switch by specified sot field type
                            switch (type)
                            {
                                // Date field is saved in string representation
                                case "(date)":
                                case "(string)":
                                case "(int)":
                                case "(float)":
                                case "(double)":
                                    sortType = SortField.STRING;
                                    break;

                                // Custom field
                                case "(custom)":
                                    sortType = SortField.CUSTOM;
                                    break;

                                // Score field
                                case "(score)":
                                    sortType = SortField.SCORE;
                                    break;
                            }

                            // Create sort field for specific field and field type
                            SortField srtField = new SortField(name, sortType, reverseOrder);
                            sortFields.Add(srtField);
                        }
                    }
                }

                // Create sort object if exists at lest one search field
                if (sortFields.Count > 0)
                {
                    Sort srt = new Sort(sortFields.ToArray());
                    return srt;
                }
            }

            return null;
        }


        /// <summary>
        /// Adds the attachment results to the search results
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="results">Search results</param>
        /// <exception cref="SearchException">Thrown when an error during search occurs.</exception>
        public void AddResults(SearchParameters parameters, SearchResults results)
        {
            List<Searchable> searchers = new List<Searchable>();

            // Loop thru all indexes and create searchables
            foreach (SearchIndexInfo index in results.Indexes)
            {
                // Add index searcher to the searcher collection if exists
                var searcher = (LuceneIndexSearcher)index.Provider.GetSearcher(true);
                if (searcher != null)
                {
                    //When sort condition is used, this method must be called. Otherwise search results will not contain score.
                    searcher.SetDefaultFieldSortScoring(true, true);

                    searchers.Add(searcher);
                }
            }

            Analyzer analyzer = ((LuceneSearchAnalyzer)SearchManager.CreateAnalyzer(results.Indexes.ToArray())).Analyzer;

            if (searchers.Count > 0)
            {
                // Create lucene objects
                TopDocs hits = null;

                Searchable[] searchables = searchers.ToArray();
                MultiSearcher ins = new MultiSearcher(searchables);

                QueryParser parser = new QueryParser(Version.LUCENE_21, SearchFieldsConstants.CONTENT, analyzer);
                parser.SetDateResolution(DateTools.Resolution.SECOND);

                try
                {
                    // Get the query
                    string searchFor = TextHelper.RemoveDiacritics(parameters.SearchFor);
                    var q = parser.Parse(searchFor);

                    bool searchOnlyWhenContentPresent = SearchHelper.SearchOnlyWhenContentPresent && parameters.BlockFieldOnlySearch;

                    if ((q != null) && (!searchOnlyWhenContentPresent || q.ToString().ToLowerCSafe().Contains(SearchFieldsConstants.CONTENT)))
                    {
                        LuceneSearchFilter apf = null;
                        var sort = GetSort(parameters.SearchSort);

                        // Search
                        if (!(String.IsNullOrEmpty(parameters.Path) || (CMSString.Compare(parameters.Path, "/%", true) == 0)))
                        {
                            // Add filter for the alias path
                            string path = parameters.Path.ToLowerCSafe().TrimEnd('%');
                            apf = new LuceneSearchFilter("nodealiaspath", path, (value, match) => value.StartsWithCSafe(match));
                        }

                        if (sort != null)
                        {
                            hits = ins.Search(q, apf, SearchHelper.MaxResults, sort);
                        }
                        else
                        {
                            hits = ins.Search(q, apf, SearchHelper.MaxResults);
                        }
                    }
                }
                // TooManyClauses, ParseException, TokenManagerError
                catch (Exception ex)
                {
                    throw new SearchException("An error has occurred during search. See the inner exception for details.", ex);
                }

                // Get number of results
                if (hits != null)
                {
                    parameters.NumberOfResults = hits.TotalHits;
                    parameters.MaxScore = hits.MaxScore;
                }

                var lucHits = new LuceneSearchHits(ins, hits);
                int lastPosition = SearchHelper.FilterResults(lucHits, results, parameters);

                // Set real number of results
                if (hits != null)
                {
                    parameters.NumberOfResults = hits.TotalHits - (lastPosition - results.Results.Count);
                }
            }
        }


        /// <summary>
        /// Forcibly unlock current index.
        /// </summary>
        /// <param name="path">Index path to unlock</param>
        public void Unlock(string path)
        {
            try
            {
                Directory dir = FSDirectory.Open(path);
                if ((dir != null) && IndexWriter.IsLocked(dir))
                {
                    IndexWriter.Unlock(dir);
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("SmartSearch", "Unlock", ex);
            }
        }


        /// <summary>
        /// Returns SearchQueryClauses objects with query keywords and clauses.
        /// </summary>
        /// <param name="q">Query to process</param>
        /// <param name="type">Clause type</param>
        /// <param name="clauses">Current clauses collection</param>
        /// <param name="field">Filter for filed name</param>
        private static bool ProcessQuery(Query q, ClauseTypeEnum type, SearchQueryClauses clauses, string field)
        {
            // Indicates whether current round add some clause
            bool added = false;

            // Switch by query type
            // Boolean query
            if (q is BooleanQuery)
            {
                // Indicates whether current local add some close

                // Get boolean query
                BooleanQuery bq = q as BooleanQuery;
                // Loop thru all clauses
                foreach (BooleanClause bc in bq.Clauses)
                {
                    // Get parent type


                    #region "Clause type"

                    // Use negation if field is prohibited
                    ClauseTypeEnum curType;

                    if (bc.IsProhibited)
                    {
                        curType = (type == ClauseTypeEnum.AND_NOT) ? ClauseTypeEnum.AND : ClauseTypeEnum.AND_NOT;
                    }
                    // Otherwise use regular option
                    else if (bc.IsRequired)
                    {
                        curType = ClauseTypeEnum.AND;
                    }
                    else
                    {
                        curType = ClauseTypeEnum.OR;
                    }

                    #endregion


                    // Get clause
                    SearchQueryClauses currClauses = clauses;

                    // Indicates whether sub query is boolean query
                    bool subQueryIsBooleanQuery = false;

                    // Get sub query
                    Query subQuery = bc.Query;

                    // Check whether sub query is boolean query
                    if (subQuery is BooleanQuery)
                    {
                        // Create child query clauses
                        SearchQueryClauses child = new SearchQueryClauses();
                        // Set parent clauses
                        child.Parent = clauses;
                        // Set child clause as current clause
                        currClauses = child;
                        // Set sub query flag
                        subQueryIsBooleanQuery = true;
                    }

                    // Process sub query
                    bool currAdded = ProcessQuery(subQuery, curType, currClauses, field);

                    // If current query is boolean query and contains at least one sub-clause add this clause
                    // object to the clause collection
                    if ((currAdded) && (subQueryIsBooleanQuery))
                    {
                        clauses.AddClause(currClauses, curType);
                    }

                    // Combine added flag
                    added |= currAdded;
                }
            }
            // Term query
            else if (q is TermQuery)
            {
                // Get query object
                TermQuery tq = q as TermQuery;
                // Get current term
                Term trm = tq.Term;
                // Check if current field is required
                if (trm.Field == field)
                {
                    // Add to the clause collection
                    clauses.AddClause(trm.Text, type);
                    // Set flag that the clause was added
                    added = true;
                }
            }
            // Phrase query
            else if (q is PhraseQuery)
            {
                // Get phrase query
                PhraseQuery pq = q as PhraseQuery;
                string phrase = String.Empty;

                // Get phrase terms
                Term[] terms = pq.GetTerms();
                // Loop thru all terms
                foreach (Term trm in terms)
                {
                    // Check if current field is required
                    if (trm.Field == field)
                    {
                        // Add to the phrase words
                        phrase += trm.Text + " ";
                    }
                }

                // Trim phrase words
                phrase = phrase.Trim();

                // Check whether exists at least one phrase word
                if (!String.IsNullOrEmpty(phrase))
                {
                    // Add to the clause collection with phrase quotes
                    clauses.AddClause("\"" + phrase + "\"", type);
                    // Set flag that the clause was added
                    added = true;
                }
            }
            // Wildcard query
            else if (q is WildcardQuery)
            {
                // Get wildcard query
                WildcardQuery wq = q as WildcardQuery;
                // Get current term
                Term trm = wq.Term;

                // Check if current field is required
                if (trm.Field == field)
                {
                    // Add to the clause collection
                    clauses.AddClause(trm.Text, type);
                    // Set flag that the clause was added
                    added = true;
                }
            }
            // Prefix query
            else if (q is PrefixQuery)
            {
                // Get prefix query
                PrefixQuery pq = q as PrefixQuery;
                // Get current term
                Term trm = pq.Prefix;

                // Check if current field is required
                if (trm.Field == field)
                {
                    // Add to the clause collection
                    clauses.AddClause(trm.Text + "*", type);
                    // Set flag that the clause was added
                    added = true;
                }
            }
            // Fuzzy query
            else if (q is FuzzyQuery)
            {
                // Get fuzy query
                FuzzyQuery fq = q as FuzzyQuery;
                // Get current term
                Term trm = fq.Term;

                // Check if current field is required
                if (trm.Field == field)
                {
                    // Add to the clause collection
                    clauses.AddClause(trm.Text, type);
                    // Set flag that the clause was added
                    added = true;
                }
            }
            else
            {
                // All other query types are not required
            }

            // Return flag if current call add some clause
            return added;
        }


        /// <summary>
        /// Returns SQL Fulltext query.
        /// </summary>
        /// <param name="searchFor">Search query</param>
        public SearchQueryClauses GetQueryClauses(string searchFor)
        {
            return GetQueryClauses(searchFor, SearchFieldsConstants.CONTENT);
        }


        /// <summary>
        /// Returns SQl Fulltext query.
        /// </summary>
        /// <param name="luceneQuery">Lucene query</param>
        /// <param name="field">Field name</param>
        private static SearchQueryClauses GetQueryClauses(string luceneQuery, string field)
        {
            // Check whether all parameters are present
            if (String.IsNullOrEmpty(luceneQuery) || String.IsNullOrEmpty(field))
            {
                return null;
            }

            // Get only required fields in supported format
            QueryParser qp = new QueryParser(Version.LUCENE_21, field, new WhitespaceAnalyzer());

            // Get parsed query
            Query query = qp.Parse(luceneQuery);

            // Create clauses field
            SearchQueryClauses clauses = new SearchQueryClauses();

            // Return Search query clauses object
            ProcessQuery(query, ClauseTypeEnum.OR, clauses, field);

            // Return clauses
            return clauses;
        }

        #endregion
    }
}