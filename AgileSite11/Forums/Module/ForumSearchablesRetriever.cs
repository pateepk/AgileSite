using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Search.Internal;

namespace CMS.Forums.Internal
{
    /// <summary>
    /// Helps with retrieving <see cref="ISearchable"/> objects and <see cref="SearchIndexInfo"/>s
    /// related to forum search.
    /// </summary>
    /// <seealso cref="SearchablesRetrievers.Register{TRetriever}(string)"/>
    public class ForumSearchablesRetriever : SearchablesRetriever
    {
        /// <summary>
        /// Gets the list of indexes relevant to the given object
        /// </summary>
        /// <param name="searchObject">Search object</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public override List<SearchIndexInfo> GetRelevantIndexes(ISearchable searchObject, string searchProvider)
        {
            List<SearchIndexInfo> relevantIndexes = new List<SearchIndexInfo>();

            string forumName = ValidationHelper.GetString(searchObject.GetValue("PostForumName"), String.Empty).ToLowerCSafe();
            bool isSearchable = ValidationHelper.GetBoolean(searchObject.GetValue("issearchable"), false);
            int siteId = ValidationHelper.GetInteger(searchObject.GetValue("PostSiteID"), 0);

            // Get site indexes from cache or DB
            List<int> indexes = SearchIndexInfoProvider.GetSiteIndexes(siteId);

            // Prepare array - because of concurent modification
            int[] indexIDs;
            lock (indexes)
            {
                indexIDs = new int[indexes.Count];
                indexes.CopyTo(indexIDs, 0);
            }

            // Loop trough all indexes
            foreach (int indexId in indexIDs)
            {
                // Get from cache or load from DB
                SearchIndexInfo sii = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);

                // Skip if not found
                if (sii == null || (!String.IsNullOrEmpty(searchProvider) && !sii.IndexProvider.Equals(searchProvider, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Default set
                bool isRelevant = false;

                // Get index settings
                SearchIndexSettings indexSettings = sii.IndexSettings;

                // If index is for documents
                if (ForumInfo.OBJECT_TYPE.EqualsCSafe(sii.IndexType, true))
                {
                    // Get settings mItems
                    Dictionary<Guid, SearchIndexSettingsInfo> settingsItems = indexSettings.Items;

                    // Prepare key array - because of concurent modification
                    Guid[] itemsKeys;
                    lock (settingsItems)
                    {
                        itemsKeys = new Guid[settingsItems.Count];
                        settingsItems.Keys.CopyTo(itemsKeys, 0);
                    }

                    foreach (Guid itemKey in itemsKeys)
                    {
                        SearchIndexSettingsInfo item = settingsItems[itemKey];
                        if (item == null)
                        {
                            continue;
                        }

                        if (item.Type == SearchIndexSettingsInfo.TYPE_ALLOWED)
                        {
                            if (!String.IsNullOrEmpty(item.ForumNames))
                            {
                                string forumNames = ";" + item.ForumNames.ToLowerCSafe() + ";";
                                if (forumNames.Contains(";" + forumName + ";"))
                                {
                                    isRelevant = true;
                                }
                            }
                            // All forums in all specified sites
                            else if (String.IsNullOrEmpty(item.SiteName))
                            {
                                if (isSearchable)
                                {
                                    isRelevant = true;
                                }
                            }
                            // All forums in site
                            else
                            {
                                SiteInfo si = SiteInfoProvider.GetSiteInfo(item.SiteName);
                                if ((si?.SiteID == siteId) && (isSearchable))
                                {
                                    isRelevant = true;
                                }
                            }
                        }
                        else
                        {
                            string forumNames = ";" + item.ForumNames.ToLowerCSafe() + ";";
                            if (forumNames.Contains(";" + forumName + ";"))
                            {
                                // One-time is exluded, always excluded
                                isRelevant = false;
                                break;
                            }
                        }
                    }
                }

                if (isRelevant)
                {
                    relevantIndexes.Add(sii);
                }
            }

            return relevantIndexes;
        }
    }
}
