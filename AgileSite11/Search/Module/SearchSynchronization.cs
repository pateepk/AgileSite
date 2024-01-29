using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Web farm synchronization for Forums
    /// </summary>
    internal class SearchSynchronization
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets value that indicates whether smart search synchronization is enabled.
        /// </summary>
        public static bool SynchronizeSmartSearch
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeSmartSearch", "CMSWebFarmSynchronizeSmartSearch", true);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SearchTaskType.RunSmartSearchIndexer,
                Execute = RunSmartSearchIndexer,
                Condition = CheckSynchronizeSmartSearch
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SearchTaskType.InvalidateSearcher,
                Execute = InvalidateSearcher,
                Condition = CheckSynchronizeSmartSearch,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the smart search is allowed
        /// </summary>
        private static bool CheckSynchronizeSmartSearch(IWebFarmTask task)
        {
            return SynchronizeSmartSearch;
        }


        /// <summary>
        /// Runs the smart search indexer
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RunSmartSearchIndexer(string target, string[] data, BinaryData binaryData)
        {
            SearchTaskInfoProvider.ProcessTasks();
        }


        /// <summary>
        /// Invalidates searcher.
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateSearcher(string target, string[] data, BinaryData binaryData)
        {
            SearchHelper.InvalidateSearcher(Guid.Parse(data.FirstOrDefault() ?? ""));
        }

        #endregion
    }
}
