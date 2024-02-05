using System;
using System.Linq;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Search
{
    /// <summary>
    /// Provides indexing for particular search index type
    /// </summary>
    public class SearchIndexProvider
    {
        #region "Variables"

        private SearchIndexInfo mSearchIndexInfo;

        /// <summary>
        /// Indicates whether unlock on first call of GetWriter/GetSearcher method was performed
        /// </summary>
        private bool unlockPerformed = false;

        /// <summary>
        /// Indexing analyzer object.
        /// </summary>
        private ISearchAnalyzer mCurrentAnalyzer = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchIndexInfo">Search index info</param>
        public SearchIndexProvider(SearchIndexInfo searchIndexInfo)
        {
            mSearchIndexInfo = searchIndexInfo;
        }


        /// <summary>
        /// Returns true if the index is optimized
        /// </summary>
        public bool IsOptimized()
        {
            var searcher = GetSearcher();
            if (searcher != null)
            {
                return searcher.IsOptimized();
            }

            return false;
        }


        /// <summary>
        /// Returns the search index status
        /// </summary>
        public virtual IndexStatusEnum GetIndexStatus()
        {
            try
            {
                // Get the searcher
                var iSearcher = GetSearcher();
                if (iSearcher == null)
                {
                    return IndexStatusEnum.NEW;
                }
                else
                {
                    return IndexStatusEnum.READY;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SearchIndex", "GETSTATUS", ex);

                return IndexStatusEnum.UNKNOWN;
            }
        }


        /// <summary>
        /// Returns time when index files were updated based last write time of it's files. 
        /// </summary>
        internal virtual DateTime GetIndexLastUpdateTime()
        {
            DateTime indexFilesLastUpdate = DateTimeHelper.ZERO_TIME;

            try
            {
                // Find the file with the latest date time
                var di = DirectoryInfo.New(mSearchIndexInfo.CurrentIndexPath);
                if (di.Exists)
                {
                    // Directory does not exist until index is built
                    indexFilesLastUpdate = di.GetFiles().Aggregate(DateTimeHelper.ZERO_TIME, (max, fi) => fi.LastWriteTime > max ? fi.LastWriteTime : max);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SearchIndex", "GETLASTUPDATE", ex);
            }
            finally 
            {
                if (indexFilesLastUpdate == DateTimeHelper.ZERO_TIME)
                {
                    indexFilesLastUpdate = DateTime.Now;
                }
            }

            return indexFilesLastUpdate;
        }


        /// <summary>
        /// Gets the number of the indexed items
        /// </summary>
        public int GetNumberOfIndexedItems()
        {
            var iSearcher = GetSearcher();
            if (iSearcher != null)
            {
                return iSearcher.NumberOfDocuments();
            }

            return 0;
        }


        /// <summary>
        /// Returns current object analyzer.
        /// </summary>
        /// <param name="isSearch">Indicates whether analyzer should be used for search or indexing</param>
        public ISearchAnalyzer GetAnalyzer(bool isSearch)
        {
            // Set search analyzer if is required
            ISearchAnalyzer currentAnalyzer;

            if (isSearch)
            {
                currentAnalyzer = SearchManager.CreateAnalyzer(mSearchIndexInfo, true);
            }
            else
            {
                currentAnalyzer = mCurrentAnalyzer ?? (mCurrentAnalyzer = SearchManager.CreateAnalyzer(mSearchIndexInfo, false));
            }

            return currentAnalyzer;
        }


        /// <summary>
        /// Returns IndexWriter object with dependence on current index.
        /// </summary>
        /// <param name="create">Indicates whether index should be created</param>
        public IIndexWriter GetWriter(bool create)
        {   
            // Unlock current index if it is first call of modify method
            if (!unlockPerformed)
            {
                unlockPerformed = true;
                Unlock();
            }

            string path = mSearchIndexInfo.CurrentIndexPath;

            IIndexWriter iw;

            try
            {
                iw = SearchManager.CreateIndexWriter(path, GetAnalyzer(false), create);
            }
            catch (Exception ex)
            {
                throw new SearchIndexException(mSearchIndexInfo, "An error occured while obtaining the index writer. This may be caused by a fatal error or index corruption. Consider rebuilding the index if the problem persists.", ex);
            }

            // Change maximal field length
            if (SearchHelper.MaxFieldLength > 0)
            {
                iw.SetMaxFieldLength(SearchHelper.MaxFieldLength);
            }

            return iw;
        }


        /// <summary>
        /// Returns IndexSearcher object with dependence on current index.
        /// </summary>
        public IIndexSearcher GetSearcher()
        {
            return GetSearcher(false);
        }


        /// <summary>
        /// Returns IndexSearcher object with dependence on current index.
        /// </summary>
        /// <param name="readOnly">Indicates whether searcher will be used for deleting (false) or searching (true)</param>
        public IIndexSearcher GetSearcher(bool readOnly)
        {
            string path = mSearchIndexInfo.CurrentIndexPath;

            // Get current index key name
            string keyName = mSearchIndexInfo.IndexGUID.ToString();

            // Add read only postfix for read only Searchers
            if (readOnly)
            {
                keyName += "_readonly";
            }
            // Perform unlock action if it is first modify method
            else if (!unlockPerformed)
            {
                unlockPerformed = true;
                Unlock();
            }

            IIndexSearcher searcher = null;
            for (var i = 0; (searcher == null) && (i < 5); i++)
            {
                try
                {
                    // Synchronization Searchers lock
                    lock (SearchHelper.Searchers)
                    {
                        // Create new searcher if doesn't exist yet
                        searcher = SearchHelper.Searchers[keyName] ?? (SearchHelper.Searchers[keyName] = SearchManager.CreateIndexSearcher(path));
                    }
                    if ((searcher != null) && !searcher.IsValid())
                    {
                        SearchHelper.InvalidateSearcher(mSearchIndexInfo.IndexGUID);
                        searcher = null;
                    }
                }
                catch
                {
                }
            }

            return searcher;
        }


        /// <summary>
        /// Invalidates cached analyzer for current index.
        /// </summary>
        public void InvalidateAnalyzer()
        {
            mCurrentAnalyzer = null;
        }


        /// <summary>
        /// Forcibly unlock current index.
        /// </summary>
        public void Unlock()
        {
            SearchManager.Unlock(mSearchIndexInfo.CurrentIndexPath);
        }

        #endregion
    }
}