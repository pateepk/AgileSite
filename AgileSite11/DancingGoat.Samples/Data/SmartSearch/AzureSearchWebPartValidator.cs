using CMS.DocumentEngine;
using CMS.PortalEngine;
using CMS.Search;
using CMS.Search.Azure;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Class for providing data validation required Azure search web part demo for Dancing Goat site.
    /// </summary>
    public class AzureSearchWebPartValidator
    {
        private const string SMART_SEARCH_AZURE_WEB_PART_NAME = "SampleDancingGoatSmartSearchAzure";
        private const string SMART_SEARCH_AZURE_WEB_PART_CATEGORY_NAME = "SampleCategoryDancingGoat";
        private const string SMART_SEARCH_AZURE_INDEX_NAME = "sample-dancinggoat-coffee-azure";


        /// <summary>
        /// Returns <c>true</c> if all dependent items already exist and Coffee-Azure page is correctly set, <c>false</c> otherwise.
        /// </summary>
        public bool Validate()
        {
            var categoryInfo = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName(SMART_SEARCH_AZURE_WEB_PART_CATEGORY_NAME);

            var webPartInfo = WebPartInfoProvider.GetWebPartInfo(SMART_SEARCH_AZURE_WEB_PART_NAME);

            var indexName = NamingHelper.GetValidIndexName(SMART_SEARCH_AZURE_INDEX_NAME);
            var indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexName);

            var page = GetCoffeeAzurePage();

            return indexInfo != null
                   && webPartInfo != null
                   && categoryInfo != null
                   && page != null
                   && !page.GetValue("DocumentMenuItemHideInNavigation", true)
                   && page.GetValue("DocumentShowInSiteMap", false);
        }


        /// <summary>
        /// Returns <c>true</c> if Coffee-Azure page exists, <c>false</c> otherwise.
        /// </summary>
        /// <returns></returns>
        public bool IsAzurePageAvailable()
        {
            return GetCoffeeAzurePage() != null;
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
    }
}
