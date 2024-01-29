using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Interface for the search index writer
    /// </summary>
    public interface IIndexWriter
    {
        /// <summary>
        /// Updates the document within the index
        /// </summary>
        /// <param name="doc">Document to update</param>
        /// <param name="docId">Document ID</param>
        void UpdateDocument(ILuceneSearchDocument doc, string docId);

        /// <summary>
        /// Adds the document to the index
        /// </summary>
        /// <param name="doc">Document to add</param>
        void AddDocument(ILuceneSearchDocument doc);

        /// <summary>
        /// Flushes the index buffer
        /// </summary>
        void Flush();

        /// <summary>
        /// Optimizes the index
        /// </summary>
        void Optimize();

        /// <summary>
        /// Closes the index
        /// </summary>
        void Close();

        /// <summary>
        /// Sets the maximum field length
        /// </summary>
        /// <param name="maxFieldLength">New maximum field length</param>
        void SetMaxFieldLength(int maxFieldLength);
    }
}
