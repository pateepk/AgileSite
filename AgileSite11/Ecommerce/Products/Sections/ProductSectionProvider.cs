using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DocumentEngine.Internal;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides product section retrieving functionality based on product page hierarchy.
    /// </summary>
    internal sealed class ProductSectionProvider : IProductSectionProvider
    {
        private const char SLASH_SIGN = '/';

        private readonly ISiteService mSiteService;
        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Creates a new instance of <see cref="ProductSectionProvider"/>
        /// </summary>
        public ProductSectionProvider(ISiteService siteService, ISettingServiceFactory settingServiceFactory)
        {
            if (siteService == null)
            {
                throw new ArgumentNullException(nameof(siteService));
            }

            if (settingServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(settingServiceFactory));
            }

            mSiteService = siteService;
            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Returns product sections for combination of product and site.
        /// </summary>
        /// <param name="productID">Product identifier</param>
        public IEnumerable<int> GetSections(int productID)
        {
            var currentSite = mSiteService.CurrentSite;
            if (currentSite == null)
            {
                throw new InvalidOperationException("Could not retrieve the current site");
            }

            var siteID = currentSite.SiteID;

            productID = GetRelevantProductID(productID);

            var documentAliasPaths = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                          .OnSite(siteID)
                                          .Column("NodeAliasPath")
                                          .WhereEquals("NodeSKUID", productID)
                                          .GetListResult<string>();

            if (documentAliasPaths.Any())
            {
                var allSections = GetAllProductSections(documentAliasPaths, siteID);

                return GetNodeIDs(allSections, siteID);
            }

            return Enumerable.Empty<int>();
        }


        private static int GetRelevantProductID(int productID)
        {
            var product = SKUInfoProvider.GetSKUInfo(productID);
            return product != null && product.IsProductVariant ? product.SKUParentSKUID : productID;
        }


        /// <summary>
        /// Returns NodeAliasPaths of all product sections, where the selected documents belong.
        /// </summary>
        private ICollection<string> GetAllProductSections(IEnumerable<string> nodeAliasPaths, int siteID)
        {
            var service = mSettingServiceFactory.GetSettingService(siteID);
            var startingPath = service.GetStringValue(ECommerceSettings.PRODUCTS_STARTING_PATH);

            var sections = new List<string>();

            foreach (var path in nodeAliasPaths)
            {
                var pathSections = GetAllProductSectionsForPath(path, startingPath);
                sections.AddRange(pathSections);
            }

            return sections.Distinct().ToList();
        }


        /// <summary>
        /// Returns NodeAliasPath of each product section till the product section root for the selected NodeAliasPath. 
        /// Example: for path "/a/b/c/d", startingPath "/a/b", result => { "/a/b/c/d", "/a/b/c", "/a/b"}
        /// </summary>
        private static ICollection<string> GetAllProductSectionsForPath(string nodeAliasPath, string startingPath)
        {
            var sections = new List<string>();

            if (!nodeAliasPath.StartsWith(startingPath, StringComparison.OrdinalIgnoreCase))
            {
                return sections;
            }

            var currentPath = nodeAliasPath;
            sections.Add(currentPath);

            var sectionStartIndex = startingPath.Length;
            var lastSlashIndex = currentPath.LastIndexOf(SLASH_SIGN);

            while (lastSlashIndex >= sectionStartIndex)
            {
                currentPath = currentPath.Substring(0, lastSlashIndex);
                sections.Add(currentPath);
                lastSlashIndex = currentPath.LastIndexOf(SLASH_SIGN);
            }

            return sections;
        }


        private static IEnumerable<int> GetNodeIDs(ICollection<string> sectionAliasPaths, int siteID)
        {
            return DocumentNodeDataInfoProvider.GetDocumentNodes()
                                               .OnSite(siteID)
                                               .Column("NodeID")
                                               .WhereIn("NodeAliasPath", sectionAliasPaths)
                                               .GetListResult<int>();
        }
    }
}