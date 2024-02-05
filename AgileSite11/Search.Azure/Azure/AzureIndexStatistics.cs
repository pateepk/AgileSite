using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Encapsulates statistics of an Azure index.
    /// </summary>
    public class AzureIndexStatistics : IIndexStatistics
    {
        /// <summary>
        /// Gets the number of indexed documents.
        /// </summary>
        public long DocumentCount
        {
            get;
        }


        /// <summary>
        /// Gets the index size in bytes.
        /// </summary>
        public long Size
        {
            get;
        }


        /// <summary>
        /// Initializes Azure index statistics.
        /// </summary>
        /// <param name="documentCount">Number of indexed documents.</param>
        /// <param name="size">Size of the index.</param>
        public AzureIndexStatistics(long documentCount, long size)
        {
            DocumentCount = documentCount;
            Size = size;
        }
    }
}
