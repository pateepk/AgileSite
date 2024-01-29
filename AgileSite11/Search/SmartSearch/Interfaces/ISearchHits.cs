using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Interface for the search result hits
    /// </summary>
    public interface ISearchHits
    {
        /// <summary>
        /// Returns the length of the hits collection
        /// </summary>
        int Length();

        /// <summary>
        /// Returns the maximum score value encountered.
        /// </summary>
        /// <returns></returns>
        float MaxScore();

        /// <summary>
        /// Returns the document at the specified index
        /// </summary>
        /// <param name="i">Index</param>
        ILuceneSearchDocument Doc(int i);

        /// <summary>
        /// Returns the score of the document at the specified index
        /// </summary>
        /// <param name="i">Index</param>
        float Score(int i);
    }
}
