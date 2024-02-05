using System;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Localization;
using CMS.Modules;
using CMS.Search;
using CMS.Search.Azure;
using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Class for generating data required for Azure search web part demo for Dancing Goat site.
    /// </summary>
    public class AzureSearchWebPartGenerator
    {
        private const string SMART_SEARCH_AZURE_WEB_PART_NAME = "SampleDancingGoatSmartSearchAzure";
        private const string SMART_SEARCH_AZURE_INDEX_NAME = "sample-dancinggoat-coffee-azure";


        /// <summary>
        /// Creates Azure web part and all required items. 
        /// </summary>
        /// <param name="indexServiceName">Index service name.</param>
        /// <param name="indexAdminKey">Index admin key.</param>
        public void Generate(string indexServiceName, string indexAdminKey)
        {
            var page = GetCoffeeAzurePage();
            if(page != null)
            {
                page.SetValue("DocumentMenuItemHideInNavigation", false);
                page.SetValue("DocumentShowInSiteMap", true);
                page.Update();
            }

            SampleWebPartsGenerator.EnsureWebpart(
                webPartName: SMART_SEARCH_AZURE_WEB_PART_NAME,
                webPartDisplayName: "Sample Dancing Goat - Smart search Azure",
                webPartFilePath: "DancingGoat.Samples/DancingGoatSmartSearchAzure.ascx",
                webPartResourceId: ResourceInfoProvider.GetResourceInfo("CMS.Search.Azure").ResourceID
            );

            EnsureIndex(indexServiceName, indexAdminKey);
        }


        private SearchIndexInfo GetNewPageIndex(string indexName, string indexServiceName, string indexAdminKey)
        {
            var indexSettingsInfo = new SearchIndexSettingsInfo
            {
                ID = Guid.NewGuid(),
                Path = "/Store/Coffee/%",
                Type = SearchIndexSettingsInfo.TYPE_ALLOWED,
                IncludeAttachments = false,
                IncludeBlogs = false,
                IncludeCategories = false,
                IncludeForums = false,
                IncludeMessageCommunication = false,
                ClassNames = String.Empty
            };

            var indexSettings = new SearchIndexSettings();
            indexSettings.SetSearchIndexSettingsInfo(indexSettingsInfo);

            var indexInfo = new SearchIndexInfo
            {
                IndexName = indexName,
                IndexDisplayName = "Sample Dancing Goat - Coffee",
                IndexType = TreeNode.OBJECT_TYPE,
                IndexProvider = SearchIndexInfo.AZURE_SEARCH_PROVIDER,
                IndexAdminKey = indexAdminKey,
                IndexSearchServiceName = indexServiceName,
                IndexSettings = indexSettings,
                IndexIsCommunityGroup = false
            };

            return indexInfo;
        }


        private TreeNode GetCoffeeAzurePage()
        {
            var node = new NodeSelectionParameters
            {
                AliasPath = "/Store/Coffee-Azure",
                CultureCode = "en-us"
            };

            return DocumentHelper.GetDocument(node, null);
        }


        private void EnsureIndex(string indexServiceName, string indexAdminKey)
        {
            var indexName = NamingHelper.GetValidIndexName(SMART_SEARCH_AZURE_INDEX_NAME);
            var indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexName);

            if (indexInfo == null)
            {
                indexInfo = GetNewPageIndex(indexName, indexServiceName, indexAdminKey);

                SearchIndexInfoProvider.SetSearchIndexInfo(indexInfo);

                SearchIndexSiteInfoProvider.AddSearchIndexToSite(indexInfo.IndexID, SiteContext.CurrentSiteID);
                SearchIndexCultureInfoProvider.AddSearchIndexCulture(indexInfo.IndexID, CultureInfoProvider.GetCultureInfo("en-us").CultureID);

                SearchHelper.CreateRebuildTask(indexInfo.IndexID);
            }
            else
            {
                indexInfo.IndexSearchServiceName = indexServiceName;
                indexInfo.IndexAdminKey = indexAdminKey;

                SearchIndexInfoProvider.SetSearchIndexInfo(indexInfo);
            }
        }
    }
}
