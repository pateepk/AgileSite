using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

using Version = Lucene.Net.Util.Version;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Lucene index searcher
    /// </summary>
    internal class LuceneIndexSearcher : IndexSearcher, IIndexSearcher
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Index directory</param>
        public LuceneIndexSearcher(Directory directory)
            : base(directory, false)
        {
        }


        /// <summary>
        /// Returns true if the index is optimized
        /// </summary>
        public bool IsOptimized()
        {
            var reader = IndexReader;
            if ((reader != null) && reader.IsOptimized())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Searches the given query
        /// </summary>
        /// <param name="query">Query to search</param>
        /// <param name="analyzer">Search analyzer</param>
        /// <param name="filter">Search filter</param>
        public ISearchHits Search(string query, ISearchAnalyzer analyzer, ISearchFilter filter = null)
        {
            var a = (LuceneSearchAnalyzer)analyzer;

            // Get parser
            QueryParser parser = new QueryParser(Version.LUCENE_21, SearchFieldsConstants.CONTENT, a.Analyzer);

            // Search for documents only in specified site and type
            Query q = parser.Parse(query);

            var f = (Filter)filter;

            var hits = Search(q, f, MaxDoc);
            
            return new LuceneSearchHits(this, hits);
        }


        /// <summary>
        /// Deletes the items with matching field name and value
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="value">Value</param>
        public void Delete(string name, string value)
        {
            // Check whether searcher and reader exists
            if (IndexReader != null)
            {
                // Create field term
                Term term = new Term(name, value);

                // Delete all appropriate documents
                IndexReader.DeleteDocuments(term);
                IndexReader.Commit();
            }
        }


        /// <summary>
        /// Commits the searcher
        /// </summary>
        public void Commit()
        {
            if (IndexReader != null)
            {
                IndexReader.Commit();
            }
        }


        /// <summary>
        /// Returns true if the searcher is valid
        /// </summary>
        public bool IsValid()
        {
            return (IndexReader != null) && IndexReader.IsCurrent();
        }


        /// <summary>
        /// Returns the number of documents available in the searcher
        /// </summary>
        public int NumberOfDocuments()
        {
            var ir = IndexReader;
            if (ir != null)
            {
                return ir.NumDocs();
            }

            return 0;
        }
    }
}
