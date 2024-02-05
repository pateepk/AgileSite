using CMS.IO;

namespace CMS.Search
{
    /// <summary>
    /// Encapsulates statistics of a Lucene index.
    /// </summary>
    public class LuceneIndexStatistics : IIndexStatistics
    {
        private readonly SearchIndexInfo index;
        private long size = -1;
        private long documentCount = -1;
        private readonly object initializationLock = new object();

        /// <summary>
        /// Gets the number of indexed documents.
        /// </summary>
        public long DocumentCount
        {
            get
            {
                if (documentCount == -1)
                {
                    lock (initializationLock)
                    {
                        if (documentCount == -1)
                        {
                            documentCount = index.Provider.GetNumberOfIndexedItems();
                        }
                    }
                }

                return documentCount;
            }
        }


        /// <summary>
        /// Gets the total size of Lucene index files.
        /// </summary>
        public long Size
        {
            get
            {
                if (size == -1)
                {
                    lock (initializationLock)
                    {
                        if (size == -1)
                        {
                            try
                            {
                                size = ComputeIndexFileSize();
                            }
                            catch
                            {
                                size = 0;
                            }
                        }
                    }
                }

                return size;
            }
        }


        /// <summary>
        /// Computes sum of size of files located in <see cref="SearchIndexInfo.CurrentIndexPath"/>
        /// </summary>
        private long ComputeIndexFileSize()
        {
            long totalSize = 0;
            string[] files = Directory.GetFiles(index.CurrentIndexPath);
            foreach (string filename in files)
            {
                FileInfo file = FileInfo.New(filename);
                totalSize += file.Length;
            }
            
            return totalSize;
        }


        /// <summary>
        /// Initializes statistics for a Lucene index.
        /// </summary>
        /// <param name="searchIndex">Lucene index to initialize statistics for.</param>
        public LuceneIndexStatistics(SearchIndexInfo searchIndex)
        {
            index = searchIndex;
        }
    }
}
