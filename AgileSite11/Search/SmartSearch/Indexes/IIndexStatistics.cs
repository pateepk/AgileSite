using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Search
{
    /// <summary>
    /// Encapsulates statistics of an index.
    /// </summary>
    public interface IIndexStatistics
    {
        /// <summary>
        /// Gets the number of indexed documents.
        /// </summary>
        long DocumentCount
        {
            get;
        }


        /// <summary>
        /// Gets the index size in bytes.
        /// </summary>
        long Size
        {
            get;
        }
    }
}
