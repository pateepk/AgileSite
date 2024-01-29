using System;

using CMS.Search;
using CMS.DocumentEngine;
using CMS.DataEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.Helpers;

namespace APIExamples
{
    /// <summary>
    /// Holds smart search API examples.
    /// </summary>
    /// <pageTitle>Smart search</pageTitle>
    internal class SmartSearch
    {
        /// <summary>
        /// Holds search index API examples.
        /// </summary>
        /// <groupHeading>Search indexes</groupHeading>
        private class SearchIndexes
        {
            /// <heading>Creating a search index</heading>
            private void CreateSearchIndex()
            {
                // Creates a new search index object
                SearchIndexInfo newIndex = new SearchIndexInfo();

                // Sets the search index properties
                newIndex.IndexDisplayName = "New index";
                newIndex.IndexName = "NewIndex";
                newIndex.IndexIsCommunityGroup = false;
                newIndex.IndexAnalyzerType = SearchAnalyzerTypeEnum.StandardAnalyzer;
                newIndex.StopWordsFile = "";
                
                // Sets the index type to Pages
                newIndex.IndexType = TreeNode.OBJECT_TYPE;

                /* The possible IndexType values are:
                 * Pages: TreeNode.OBJECT_TYPE
                 * Pages crawler: SearchHelper.DOCUMENTS_CRAWLER_INDEX
                 * Custom tables: CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE
                 * Users: UserInfo.OBJECT_TYPE
                 * Forums: PredefinedObjectType.FORUM)
                 * On-line forms: SearchHelper.ONLINEFORMINDEX
                 * General: SearchHelper.GENERALINDEX
                 * Custom: SearchHelper.CUSTOM_SEARCH_INDEX
                */
                
                // Saves the search index to the database
                SearchIndexInfoProvider.SetSearchIndexInfo(newIndex);
            }


            /// <heading>Updating a search index</heading>
            private void GetAndUpdateSearchIndex()
            {
                // Gets the search index
                SearchIndexInfo updateIndex = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                if (updateIndex != null)
                {
                    // Updates the index properties
                    updateIndex.IndexDisplayName = updateIndex.IndexDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    SearchIndexInfoProvider.SetSearchIndexInfo(updateIndex);
                }
            }


            /// <heading>Updating multiple search indexes</heading>
            private void GetAndBulkUpdateSearchIndexes()
            {                
                // Gets all smart search indexes whose code name starts with 'New'
                var indexes = SearchIndexInfoProvider.GetSearchIndexes().WhereStartsWith("IndexName", "New");

                // Loops through individual search indexes
                foreach (SearchIndexInfo index in indexes)
                {
                    // Updates the index properties
                    index.IndexDisplayName = index.IndexDisplayName.ToUpper();

                    // Saves the modified index to the database
                    SearchIndexInfoProvider.SetSearchIndexInfo(index);
                }
            }


            /// <heading>Configuring the Indexed content settings for search indexes</heading>
            private void CreateIndexSettings()
            {
                // Gets the search index
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                
                if (index != null)
                {
                    // Creates new index settings
                    SearchIndexSettingsInfo indexSettings = new SearchIndexSettingsInfo();
                    
                    // Configures the indexed content properties (for a Page index in this case)
                    indexSettings.IncludeBlogs = true;
                    indexSettings.IncludeForums = true;
                    indexSettings.IncludeMessageCommunication = true;
                    indexSettings.ClassNames = ""; // Allows indexing for all page types
                    indexSettings.Path = "/%";
                    indexSettings.Type = SearchIndexSettingsInfo.TYPE_ALLOWED;
                    
                    // Saves the index settings to the database and assigns them to the search index
                    SearchIndexSettings settings = new SearchIndexSettings();
                    settings.SetSearchIndexSettingsInfo(indexSettings);
                    index.IndexSettings = settings;

                    // Saves the search index to the database
                    SearchIndexInfoProvider.SetSearchIndexInfo(index);
                }
            }


            /// <heading>Assigning a search index to a site</heading>
            private void AddSearchIndexToSite()
            {
                // Gets the search index
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                if (index != null)
                {                    
                    // Assigns the index to the current site
                    SearchIndexSiteInfoProvider.AddSearchIndexToSite(index.IndexID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a search index from a site</heading>
            private void RemoveSearchIndexFromSite()
            {
                // Gets the search index
                SearchIndexInfo removeIndex = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                if (removeIndex != null)
                {
                    // Gets the relationship between the index and the current site
                    SearchIndexSiteInfo indexSite = SearchIndexSiteInfoProvider.GetSearchIndexSiteInfo(removeIndex.IndexID, SiteContext.CurrentSiteID);

                    // Removes the index from the site
                    SearchIndexSiteInfoProvider.DeleteSearchIndexSiteInfo(indexSite);
                }
            }


            /// <heading>Assigning a culture to a page search index</heading>
            private void AddCultureToSearchIndex()
            {
                // Gets the page search index and culture
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                CultureInfo culture = CultureInfoProvider.GetCultureInfo("en-us");

                if ((index != null) && (culture != null))
                {
                    // Assigns the culture to the index
                    SearchIndexCultureInfoProvider.AddSearchIndexCulture(index.IndexID, culture.CultureID);
                }
            }


            /// <heading>Removing a culture from a page search index</heading>
            private void RemoveCultureFromSearchIndex()
            {
                // Gets the page search index and culture
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");
                CultureInfo culture = CultureInfoProvider.GetCultureInfo("en-us");

                if ((index != null) && (culture != null))
                {
                    // Gets the relationship between the index and the culture
                    SearchIndexCultureInfo indexCulture = SearchIndexCultureInfoProvider.GetSearchIndexCultureInfo(index.IndexID, culture.CultureID);

                    // Removes the culture from the index
                    SearchIndexCultureInfoProvider.DeleteSearchIndexCultureInfo(indexCulture);
                }
            }


            /// <heading>Deleting a search index</heading>
            private void DeleteSearchIndex()
            {
                // Gets the search index
                SearchIndexInfo deleteIndex = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");

                if (deleteIndex != null)
                {
                    // Deletes the search index
                    SearchIndexInfoProvider.DeleteSearchIndexInfo(deleteIndex);
                }
            }
        }


        /// <summary>
        /// Holds search action API examples.
        /// </summary>
        /// <groupHeading>Search actions</groupHeading>
        private class SearchActions
        {
            /// <heading>Rebuilding a search index</heading>
            private void RebuildIndex()
            {
                // Gets the search index
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");

                if (index != null)
                {
                    // Creates a rebuild task for the index.
                    // The rebuild task will be processed as part of the next request handled by the application,
                    // or by a scheduled task if the application is configured to handle search tasks using the scheduler.
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Rebuild, null, null, index.IndexName, index.IndexID);
                }
            }


            /// <heading>Using an index to search through text</heading>
            private void SearchText()
            {
                // Gets the search index
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo("NewIndex");

                if (index != null)
                {
                    // Prepares the search parameters
                    SearchParameters parameters = new SearchParameters()
                    {
                        SearchFor = "home",
                        SearchSort = "##SCORE##",
                        Path = "/%",                        
                        CurrentCulture = "EN-US",
                        DefaultCulture = CultureHelper.EnglishCulture.IetfLanguageTag,
                        CombineWithDefaultCulture = false,
                        CheckPermissions = false,
                        SearchInAttachments = false,
                        User = (UserInfo)MembershipContext.AuthenticatedUser,
                        SearchIndexes = index.IndexName,
                        StartingPosition = 0,
                        DisplayResults = 100,
                        NumberOfProcessedResults = 100,
                        NumberOfResults = 0,
                        AttachmentWhere = String.Empty,
                        AttachmentOrderBy = String.Empty,
                        ClassNames = ""
                        /* The 'SearchParameters.ClassNames' property only limits the attachment search,
                        not the results of the basic SearchHelper.Search method. You can limit class names (page types)
                        by adding a Lucene search condition to the text of the 'SearchFor' query. */
                    };

                    // Performs the search and saves the results into a DataSet
                    System.Data.DataSet results = SearchHelper.Search(parameters);
                    
                    if (parameters.NumberOfResults > 0)
                    {
                        // The search found at least one matching result, and you can handle the results
                        
                    }
                }
            }


            /// <heading>Creating an update task for search indexes</heading>
            private void UpdateIndex()
            {
                // Gets a TreeProvider instance
                TreeProvider provider = new TreeProvider(MembershipContext.AuthenticatedUser);
                
                // Gets a page from the current site
                TreeNode node = provider.SelectNodes()
                    .Path("/")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;


                // Checks that the page exists and has search allowed
                if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                {
                    // Edits and saves the page
                    node.DocumentName += " changed";
                    node.Update();

                    // Creates a smart search update task for the page (for all search indexes that cover the given page)
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                }
            }
        }
    }
}
