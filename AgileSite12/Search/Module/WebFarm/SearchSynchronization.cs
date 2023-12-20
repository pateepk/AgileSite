using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Web farm synchronization for Forums.
    /// </summary>
    internal static class SearchSynchronization
    {
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


        /// <summary>
        /// Initializes the tasks for media files synchronization.
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<InvalidateSearcherWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RunSmartSearchIndexerWebFarmTask>();
        }
    }
}
