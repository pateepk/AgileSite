using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Index writer for Lucene search engine
    /// </summary>
    internal class LuceneIndexWriter : IndexWriter, IIndexWriter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Index directory</param>
        /// <param name="a">Analyzer</param>
        /// <param name="create">Create index</param>
        public LuceneIndexWriter(Directory directory, Analyzer a, bool create)
            : base(directory, a, create, new MaxFieldLength(DEFAULT_MAX_FIELD_LENGTH))
        {
        }


        /// <summary>
        /// Updates the document within the index
        /// </summary>
        /// <param name="doc">Document to update</param>
        /// <param name="docId">Document ID</param>
        public void UpdateDocument(ILuceneSearchDocument doc, string docId)
        {
            var term = new Term(SearchFieldsConstants.ID, docId);

            UpdateDocument(term, GetLuceneDocument(doc));
        }


        /// <summary>
        /// Adds the specific document to the writer
        /// </summary>
        /// <param name="doc">Document to add</param>
        public void AddDocument(ILuceneSearchDocument doc)
        {
            AddDocument(GetLuceneDocument(doc));
        }


        /// <summary>
        /// Gets the Lucene search document from the given document
        /// </summary>
        /// <param name="doc"></param>
        private static Document GetLuceneDocument(ILuceneSearchDocument doc)
        {
            var lucDoc = (LuceneSearchDocument)doc;

            return lucDoc.Document;
        }


        /// <summary>
        /// Flushes the writer.
        /// </summary>
        public void Flush()
        {
            Flush(true, true, true);
        }
    }
}
