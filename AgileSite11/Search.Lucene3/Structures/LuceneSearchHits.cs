using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

using Lucene.Net.Search;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Represents the search hits collection
    /// </summary>
    internal class LuceneSearchHits : ISearchHits
    {
        #region "Properties"

        /// <summary>
        /// Underlying search hits
        /// </summary>
        public TopDocs Hits
        {
            get;
            protected set;
        }


        /// <summary>
        /// Underlying search hits
        /// </summary>
        public Searcher Searcher
        {
            get;
            protected set;
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searcher">Searcher used to get the hits</param>
        /// <param name="hits">Base hits</param>
        public LuceneSearchHits(Searcher searcher, TopDocs hits)
        {
            Searcher = searcher;
            Hits = hits;
        }


        /// <summary>
        /// Returns the length of the hits collection
        /// </summary>
        public int Length()
        {
            return Hits != null ? Hits.TotalHits : 0;
        }


        /// <summary>
        /// Returns the score of the document at the specified index
        /// </summary>
        public float MaxScore()
        {
            return Hits.MaxScore;
        }

        
        /// <summary>
        /// Returns the document at the specified index
        /// </summary>
        /// <param name="i">Index</param>
        public ILuceneSearchDocument Doc(int i)
        {
            if (Hits.ScoreDocs.Length > i)
            {
                var docId = Hits.ScoreDocs[i].Doc;
                return new LuceneSearchDocument(Searcher.Doc(docId));
            }
            return new LuceneSearchDocument();
        }


        /// <summary>
        /// Returns the score of the document at the specified index
        /// </summary>
        /// <param name="i">Index</param>
        public float Score(int i)
        {
            if (Hits.ScoreDocs.Length > i)
            {
                return Hits.ScoreDocs[i].Score;
            }
            return 0;
        }

        #endregion
    }
}
