using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

using CMS.DocumentEngine;
using CMS.Ecommerce;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Loads documents according to the settings and generates XML feed suitable for Strands. If offset and maxcount GET
    /// parameters are provided, the feed is paged.
    /// </summary>
    public sealed class StrandsCatalogFeed : IHttpHandler
    {
        #region "IHttpHandler implementation"

        /// <summary>
        /// Gets whether this handler can be reused for other request. This property is required by IHttpHandler interface.
        /// </summary>
        /// <value>Always false</value>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Processes request and generates XML feed of documents to the HTTP response.
        /// </summary>
        /// <param name="context">An HttpContext associated with this request</param>
        public void ProcessRequest(HttpContext context)
        {
            // Redirect to a page with nice message if current website is stopped or the requested domain name is not configured for any website
            if (string.IsNullOrEmpty(SiteContext.CurrentSiteName))
            {
                URLHelper.Redirect("~/CMSMessages/invalidwebsite.aspx");

                return;
            }

            HttpResponse httpResponse = context.Response;

            // If Strands API ID is not set, do not render feed and display an error message
            if (!StrandsSettings.IsStrandsEnabled(SiteContext.CurrentSiteName))
            {
                string strandsDisabledErrorMessage = ResHelper.GetString("strands.notoken");

                httpResponse.Write(strandsDisabledErrorMessage);

                return;
            }

            if (!StrandsCatalogFeedSecurity.CheckCredentials(context))
            {
                return;
            }

            PagingData pagingData = GetPagingInfoFromQueryParameters();

            WriteCatalogToOutputStream(httpResponse, pagingData);
        }

        #endregion


        #region "Response write methods"

        /// <summary>
        /// Generates XML catalog and writes it to the output stream.
        /// </summary>
        /// <param name="response">Response of HTTP context</param>
        /// <param name="pagingData">Indicates if paging is enabled and what page shall be returned</param>
        private static void WriteCatalogToOutputStream(HttpResponse response, PagingData pagingData)
        {
            using (var xmlTextWriter = new XmlTextWriter(response.OutputStream, Encoding.UTF8))
            {
                try
                {
                    TransformationInfo transformationInfo = GetTransformationInfoFromSettings();

                    // Obtain all products according to provided settings and total count of products 
                    CatalogData catalogData = pagingData.IsPaged ? GetPagedProducts(pagingData) : GetAllProducts();

                    List<CurrencyInfo> currencies = GetCurrencies();

                    // Set appropriate HTTP response headers (content type to xml and indicate that response will be streamed)
                    response.ContentType = "application/xml";
                    response.ContentEncoding = Encoding.UTF8;
                    response.Buffer = false;

                    // Write header information info the output stream including total count of products for pagination purposes
                    WriteCatalogBeginning(xmlTextWriter, catalogData.ItemsTotalCount);

                    // All items has to be enclosed in the products tag
                    xmlTextWriter.WriteStartElement("products");

                    MacroResolver macroResolver = MacroResolver.GetInstance();
                    foreach (ItemData product in catalogData.Items)
                    {
                        string resolvedItem = ResolveProduct(product, currencies, transformationInfo, macroResolver);
                        xmlTextWriter.WriteRaw(resolvedItem);
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Strands Recommender", "XMLFEEDGENERATING", ex);

                    // Flush XML writer to ensure error message won't be written in the middle of feed, but at its end
                    xmlTextWriter.Flush();

                    response.Write("Error occurred while rendering catalog feed: " + ex.Message);
                }
                // XmlTextWriter automatically closes all opened tags, no methods have to be called explicitly
            }
        }


        /// <summary>
        /// Resolves macros in given product.
        /// </summary>
        /// <param name="product">Product to be resolved</param>
        /// <param name="currencies">Currencies which will be specified within product</param>
        /// <param name="transformationInfo">Transformation info containing macros</param>
        /// <param name="macroResolver">Macro resolver which will be used to resolve macros in text</param>
        /// <returns>Resolved XML string</returns>
        private static string ResolveProduct(ItemData product, List<CurrencyInfo> currencies, TransformationInfo transformationInfo, MacroResolver macroResolver)
        {
            // Add needed properties as named ones
            macroResolver.SetNamedSourceData("ItemID", product.ID);
            macroResolver.SetNamedSourceData("ItemCategory", product.Category);
            macroResolver.SetNamedSourceData("SKU", product.SKU);

            // Set collection of documents (one language version per each item) and currencies
            macroResolver.SetNamedSourceData("LanguageVersions", product.LanguageVersions);
            macroResolver.SetNamedSourceData("Currencies", currencies);

            string resolvedItem = macroResolver.ResolveMacros(transformationInfo.TransformationCode);
            return resolvedItem;
        }


        /// <summary>
        /// Gets list containing all enabled currencies with set Exchange rate. If main currency is specified, it is added to the list. 
        /// </summary>
        /// <returns>Collection of all available currencies</returns>
        private static List<CurrencyInfo> GetCurrencies()
        {
            var siteID = SiteContext.CurrentSiteID;

            // Get all currencies that are enabled at the time and the shop is able to convert among them
            var allCurrencies = CurrencyInfoProvider.GetCurrenciesByCode(siteID).Values;
            return allCurrencies.Where(currency => CurrencyInfoProvider.IsCurrencyWithExchangeRate(currency.CurrencyID, siteID)).ToList();
        }


        /// <summary>
        /// Writes beginning of XML file and appends feed description required by Strands. 
        /// </summary>
        /// <param name="xmlTextWriter">Writer of XMl file</param>
        /// <param name="totalProducts">Total count of products in the database</param>
        private static void WriteCatalogBeginning(XmlTextWriter xmlTextWriter, int totalProducts)
        {
            xmlTextWriter.WriteStartDocument();

            xmlTextWriter.WriteStartElement("sbs-catalog");
            xmlTextWriter.WriteAttributeString("version", "1.0");

            xmlTextWriter.WriteElementString("company", SiteContext.CurrentSite.DisplayName);
            xmlTextWriter.WriteElementString("link", URLHelper.GetAbsoluteUrl(URLHelper.ResolveUrl("~/")));
            xmlTextWriter.WriteElementString("description", SiteContext.CurrentSite.Description);
            xmlTextWriter.WriteElementString("product_count", totalProducts.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion


        #region "Data retrieval methods"

        /// <summary>
        /// Retrieves products from the database with the paging specified by the <paramref name="pagingData" /> applied.
        /// </summary>
        /// <param name="pagingData">Specification of the page which shall be retrieved</param>
        /// <returns>Catalog data with paging applied</returns>
        private static CatalogData GetPagedProducts(PagingData pagingData)
        {
            if (!pagingData.IsPaged)
            {
                throw new ArgumentException("[StrandsCatalogFeed:GetPagedProducts]: Paging can be applied only if paging is enabled");
            }

            // First, create query for loading filtered nodes. Only one document per Node is returned (this is achieved by grouping by NodeID). This is needed in order to perform paging on nodes, not documents
            MultiDocumentQuery nodeIdsQuery = GetBaseDocumentQuery()
                .AllCultures()
                .Column("NodeID")
                .GroupBy("NodeID")
                .OrderBy("NodeID")
                .Page(pagingData.Page, pagingData.Count);

            // Then load all documents having NodeID the same as the ones loaded previously
            var includedTreeNodes = DocumentHelper.GetDocuments()
                                                    .PublishedVersion()
                                                    .Published()
                                                    .AllCultures()
                                                    .WithCoupledColumns()
                                                    .WhereIn("NodeID", nodeIdsQuery);

            return new CatalogData
            {
                Items = GetCatalogItems(includedTreeNodes),
                ItemsTotalCount = nodeIdsQuery.TotalRecords,
            };
        }


        /// <summary>
        /// Retrieves all products from the database.
        /// </summary>
        /// <returns>Catalog data with all products</returns>
        private static CatalogData GetAllProducts()
        {
            var allTreeNodes = GetBaseDocumentQuery()
                .AllCultures()
                .WithCoupledColumns();

            List<ItemData> catalogItems = GetCatalogItems(allTreeNodes);

            return new CatalogData
            {
                Items = catalogItems,
                ItemsTotalCount = catalogItems.Count,
            };
        }


        /// <summary>
        /// Creates Catalog items from the list of tree nodes which shall be included in the catalog.
        /// One tree node in <paramref name="includedTreeNodes" /> represents one language version of the document. Documents
        /// belonging to the same node will be grouped into one CatalogItem.
        /// </summary>
        /// <param name="includedTreeNodes">Documents (all language versions) included in the feed</param>
        /// <returns>
        /// List of items which shall be included in the catalog, containing node ID, language versions, SKU object and
        /// category of product
        /// </returns>
        private static List<ItemData> GetCatalogItems(IEnumerable<TreeNode> includedTreeNodes)
        {
            List<IGrouping<string, TreeNode>> groupedTreeNodes = includedTreeNodes
                .GroupBy(StrandsCatalogPropertiesMapper.GetItemID)
                .ToList();

            List<ItemData> items = (from treeNodesGroup in groupedTreeNodes
                                    let firstNode = treeNodesGroup.First()
                                    select new ItemData
                                    {
                                        ID = treeNodesGroup.Key,
                                        Category = StrandsCatalogPropertiesMapper.GetItemCategory(firstNode),
                                        LanguageVersions = treeNodesGroup.AsEnumerable(),
                                        SKU = SKUInfoProvider.GetSKUInfo(firstNode.NodeSKUID)
                                    }).ToList();

            return items;
        }


        /// <summary>
        /// Creates MultiDocumentQuery with properties (path, document types, etc.) set to the values specified in settings.
        /// Only documents from current site are returned. Culture filter is not set.
        /// </summary>
        private static MultiDocumentQuery GetBaseDocumentQuery()
        {
            string siteName = SiteContext.CurrentSiteName;

            return DocumentHelper.GetDocuments()
                                   .PublishedVersion()
                                   .Published()
                                   .OnSite(siteName)
                                   .Where("SKUID <> 0")
                                   .Where(StrandsSettings.GetWhereCondition(siteName))
                                   .Types(StrandsSettings.GetDocumentTypes(siteName))
                                   .Path(StrandsSettings.GetPath(siteName));
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets transformation info according to code name provided in settings.
        /// </summary>
        /// <exception cref="StrandsException">Transformation provided in settings was not found or is not of type text</exception>
        /// <returns>Transformation info corresponding to settings value</returns>
        private static TransformationInfo GetTransformationInfoFromSettings()
        {
            TransformationInfo transformationInfo = StrandsSettings.GetTransformationInfo(SiteContext.CurrentSiteName);

            if (transformationInfo == null)
            {
                throw new StrandsException("[StrandsCatalogFeed.GetTransformationInfoFromSettings]: Transformation which shall be used to render Strands catalog feed was not found. Please select existing transformation in Site manager -> Settings -> Integration -> Strands Recommender");
            }
            if (transformationInfo.TransformationType != TransformationTypeEnum.Text)
            {
                throw new StrandsException(String.Format("[StrandsCatalogFeed.GetTransformationInfoFromSettings]: Only transformations of type '{0}' can be used to render Strands catalog feed", ResHelper.GetString("TransformationType.Text")));
            }

            return transformationInfo;
        }


        /// <summary>
        /// Returns information about paging which should be applied to the catalog feed.
        /// If pagination is enabled at Strands, request contains offset and maxcount parameters determining what products shall be
        /// included.
        /// If parameters are not present, it indicates that paging is not enabled.
        /// </summary>
        private static PagingData GetPagingInfoFromQueryParameters()
        {
            int offset = QueryHelper.GetInteger("offset", -1);
            int maxcount = QueryHelper.GetInteger("maxcount", 0);

            PagingData pagingData;
            if ((offset > -1) && (maxcount > 0))
            {
                pagingData = new PagingData(offset, maxcount);
            }
            else
            {
                pagingData = new PagingData();
            }
            return pagingData;
        }

        #endregion


        #region "Nested types"

        /// <summary>
        /// Holds all retrieved catalog items and count of the total items in database.
        /// </summary>
        private class CatalogData
        {
            public int ItemsTotalCount
            {
                get;
                set;
            }


            public IList<ItemData> Items
            {
                get;
                set;
            }
        }


        /// <summary>
        /// Holds data about one item in the catalog.
        /// </summary>
        private class ItemData
        {
            public string ID
            {
                get;
                set;
            }


            public string Category
            {
                get;
                set;
            }


            public IEnumerable<TreeNode> LanguageVersions
            {
                get;
                set;
            }


            public SKUInfo SKU
            {
                get;
                set;
            }
        }


        /// <summary>
        /// This class knows (contains information) if paging shall be performed and which page shall be retrieved when querying
        /// documents.
        /// </summary>
        private class PagingData
        {
            /// <summary>
            /// Indicates that feed should not be paged.
            /// </summary>
            public PagingData()
            {
                IsPaged = false;
            }


            /// <summary>
            /// Indicates that feed shall be paged and that number of documents specified by <paramref name="count" /> and starting
            /// from <paramref name="offset" /> shall be retrieved.
            /// </summary>
            /// <param name="offset">Number of documents which will be skipped</param>
            /// <param name="count">Maximum number of documents which will be retrieved</param>
            public PagingData(int offset, int count)
            {
                Offset = offset;
                Count = count;
                IsPaged = true;
            }


            public bool IsPaged
            {
                get;
                private set;
            }


            public int Offset
            {
                get;
                private set;
            }


            public int Count
            {
                get;
                private set;
            }


            /// <summary>
            /// Gets 0-based page calculated from <see cref="Offset" /> and <see cref="Count" /> properties.
            /// </summary>
            public int Page
            {
                get
                {
                    return Offset / Count;
                }
            }
        }

        #endregion
    }
}