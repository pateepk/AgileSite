using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing DocumentAliasInfo management.
    /// </summary>
    public class DocumentAliasInfoProvider : AbstractInfoProvider<DocumentAliasInfo, DocumentAliasInfoProvider>
    {
        private static Regex mWildcardSectionsRegex;


        private static Regex wildcardSectionsRegex => mWildcardSectionsRegex ?? (mWildcardSectionsRegex = RegexHelper.GetRegex(@"((?<!\[)%[^/\]]*)", RegexOptions.None));


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentAliasInfoProvider()
            : base(DocumentAliasInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the DocumentAliasInfo structure for the specified documentAlias.
        /// </summary>
        /// <param name="documentAliasId">DocumentAlias id</param>
        public static DocumentAliasInfo GetDocumentAliasInfo(int documentAliasId)
        {
            return ProviderObject.GetInfoById(documentAliasId);
        }


        /// <summary>
        /// Returns object query for aliases together with node data.
        /// </summary>
        public static ObjectQuery<DocumentAliasInfo> GetDocumentAliasesWithNodesDataQuery()
        {
            return ProviderObject.GetDocumentAliasesWithNodesDataQueryInternal();
        }


        /// <summary>
        /// Sets (updates or inserts) specified documentAlias.
        /// </summary>
        /// <param name="documentAlias">DocumentAlias to set</param>
        public static void SetDocumentAliasInfo(DocumentAliasInfo documentAlias)
        {
            ProviderObject.SetInfo(documentAlias);
        }


        /// <summary>
        /// Deletes specified documentAlias.
        /// </summary>
        /// <param name="infoObj">DocumentAlias object</param>
        public static void DeleteDocumentAliasInfo(DocumentAliasInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified documentAlias.
        /// </summary>
        /// <param name="documentAliasId">DocumentAlias id</param>
        public static void DeleteDocumentAliasInfo(int documentAliasId)
        {
            DocumentAliasInfo infoObj = GetDocumentAliasInfo(documentAliasId);
            DeleteDocumentAliasInfo(infoObj);
        }


        /// <summary>
        /// Returns the query for all document aliases.
        /// </summary>
        public static ObjectQuery<DocumentAliasInfo> GetDocumentAliases()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Moves document aliases from source tree node to the target tree node
        /// </summary>
        /// <param name="sourceNodeId">Source node id</param>
        /// <param name="targetNodeId">Target node id</param>
        /// <param name="culture">Culture code, if empty all cultures are moved</param>
        public static void MoveAliases(int sourceNodeId, int targetNodeId, string culture)
        {
            ProviderObject.MoveAliasesInternal(sourceNodeId, targetNodeId, culture);
        }


        /// <summary>
        /// Returns true if document alias is unique within the specified site
        /// </summary>
        /// <param name="aliasUrlPath">Alias URL path</param>
        /// <param name="editedAliasId">ID of edited document alias</param>
        /// <param name="culture">Alias culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="extensions">Alias extensions</param>
        /// <param name="checkDocumentUrlpath">If is true document alias is checked against document URL path and document aliases</param>
        /// <param name="nodeId">Node ID</param>
        public static bool IsUnique(string aliasUrlPath, int editedAliasId, string culture, string extensions, string siteName, bool checkDocumentUrlpath, int nodeId)
        {
            var data = ProviderObject.GetMatchingAliasesInternal(aliasUrlPath, editedAliasId, culture, extensions, siteName, checkDocumentUrlpath, nodeId);
            return DataHelper.DataSourceIsEmpty(data);
        }


        /// <summary>
        /// Returns true if the alias with the same settings as the given alias already exists.
        /// </summary>
        /// <param name="dai">Document alias to check</param>
        public static bool DocumentAliasExists(DocumentAliasInfo dai)
        {
            // Get the existing aliases count
            var count = GetDocumentAliases()
                .WhereEquals("AliasURLPath", dai.AliasURLPath)
                .WhereEquals("AliasExtensions", dai.AliasExtensions)
                .Count;

            return count > 0;
        }


        /// <summary>
        /// Deletes node aliases for given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public static void DeleteNodeAliases(int nodeId)
        {
            ProviderObject.DeleteNodeAliasesInternal(nodeId);
        }


        /// <summary>
        /// Creates wildcard rule for given URL.
        /// </summary>
        /// <param name="wildcardUrl">Wildcard URL.</param>
        /// <param name="wildcardSectionsCount">Number of URL sections containing at least one wildcard.</param>
        public static string CreateWildcardRule(string wildcardUrl, out int wildcardSectionsCount)
        {
            return ProviderObject.CreateWildcardRuleInternal(wildcardUrl, out wildcardSectionsCount);
        }


        /// <summary>
        /// Bulk inserts the given list of aliases
        /// </summary>
        /// <param name="aliases">Aliases to insert</param>
        internal static void BulkInsert(IEnumerable<DocumentAliasInfo> aliases)
        {
            ProviderObject.BulkInsertInfos(aliases);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns object query for aliases together with node data.
        /// </summary>
        protected virtual ObjectQuery<DocumentAliasInfo> GetDocumentAliasesWithNodesDataQueryInternal()
        {
            return GetDocumentAliases().Source(s => s.InnerJoin<DocumentNodeDataInfo>("AliasNodeID", "NodeID"));
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DocumentAliasInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            // Update wildcard rule
            if (!String.IsNullOrEmpty(info.AliasURLPath))
            {
                SetWildcardRuleWithPriority(info);
            }

            bool touchCache = info.AliasNodeID > 0;

            // Update or insert
            base.SetInfo(info);

            // Clear the dependent cache items
            if (touchCache && info.Generalized.TouchCacheDependencies)
            {
                CacheHelper.TouchKey("nodeid|" + info.AliasNodeID);
            }

            // Update search index
            if (SearchIndexInfoProvider.SearchEnabled && CMSActionContext.CurrentCreateSearchTask)
            {
                UpdateSearchIndex(info, SiteInfoProvider.GetSiteName(info.AliasSiteID));
            }
        }


        /// <summary>
        /// Creates wildcard rule from alias URL path and updates alias together with alias priority.
        /// </summary>
        /// <param name="documentAlias">Document alias.</param>
        internal static void SetWildcardRuleWithPriority(DocumentAliasInfo documentAlias)
        {
            documentAlias.AliasWildcardRule = CreateWildcardRule(documentAlias.AliasURLPath, out var wildcardSectionsCount);

            // Count the priority
            int slashCount = TreePathUtils.GetPathSlashCount(documentAlias.AliasURLPath);
            documentAlias.AliasPriority = slashCount - wildcardSectionsCount;
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(DocumentAliasInfo info)
        {
            if (info != null)
            {
                // Check site name
                SiteInfo si = SiteInfoProvider.GetSiteInfo(info.AliasSiteID);
                string siteName = si != null ? si.SiteName : TreeProvider.ALL_CULTURES;

                base.DeleteInfo(info);

                // Clear the dependent cache items
                if (info.Generalized.TouchCacheDependencies)
                {
                    CacheHelper.TouchKey("nodeid|" + info.AliasNodeID);
                }

                // Update search index
                if (SearchIndexInfoProvider.SearchEnabled && CMSActionContext.CurrentCreateSearchTask)
                {
                    UpdateSearchIndex(info, siteName);
                }
            }
        }

        #endregion


        #region "Internal methods - Advanced"


        /// <summary>
        /// Returns the alias matching specified wildcard rule. If does not exist alias
        /// matching specified wildcard rule, returns null.
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="currentCultureCode">Culture code of current site.</param>
        /// <param name="combineWithDefaultCulture">Indicates if</param>
        /// <remarks>Aliases of not published documents are not included in candidates set.</remarks>
        internal static DocumentAliasInfo GetDocumentAliasInternal(string path, string siteName, string currentCultureCode, bool combineWithDefaultCulture)
        {
            var culturePriority = GetCulturePriority(siteName, currentCultureCode, combineWithDefaultCulture);
            var whereCondition = GetDocumentAliasWhereCondition(path, siteName);

            // All candidates are ordered as follows:
            //  - by culture priority column (first current culture, next default culture, others)
            //  - by AliasPriority column (the most specific aliases first)
            //  - by count of sections in URL path (aliases with more sections are more specific)
            //  - alphabetically by culture code
            // The first row is the best alias candidate
            var query = GetDocumentAliases()
                .Where(whereCondition)
                .WhereIn("AliasNodeID", new DocumentQuery().Column("NodeID").Published())
                .OrderByDescending(DocumentQueryColumnBuilder.GetPlainAliasPriority().GetExpression())
                .OrderByDescending(culturePriority)
                .OrderByDescending("AliasPriority")
                .OrderByDescending(DocumentQueryColumnBuilder.AliasURLSectionsCountPriority)
                .OrderByDescending(DocumentQueryColumnBuilder.WildcardCountPriority)
                .OrderByAscending("AliasCulture")
                .TopN(1);

           return  query.FirstOrDefault();
        }


        /// <summary>
        /// Moves document aliases from source tree node to the target tree node
        /// </summary>
        /// <param name="sourceNodeId">Source node id</param>
        /// <param name="targetNodeId">Target node id</param>
        /// <param name="culture">Culture code, if empty all cultures are moved</param>
        protected virtual void MoveAliasesInternal(int sourceNodeId, int targetNodeId, string culture)
        {
            // Check culture settings
            WhereCondition condition = new WhereCondition().WhereEquals("AliasNodeID", sourceNodeId);
            if (!String.IsNullOrEmpty(culture))
            {
                condition.And().WhereEquals("AliasCulture", culture);
            }

            condition.Parameters.Add("@targetid", targetNodeId);

            // Move document aliases
            UpdateData("AliasNodeID = @targetid", condition.Parameters, condition.WhereCondition);
        }


        /// <summary>
        /// Returns dataset with matching aliases to the specified alias URL path
        /// </summary>
        /// <param name="aliasUrlPath">Alias URL path</param>
        /// <param name="editedAliasId">ID of edited document alias</param>
        /// <param name="culture">Alias culture</param>
        /// <param name="siteName">Site name</param>
        /// <param name="extensions">Alias extensions</param>
        /// <param name="checkDocumentUrlpath">If is true document alias is checked against document URL path and document aliases</param>
        /// <param name="nodeId">Node ID</param>
        protected virtual DataSet GetMatchingAliasesInternal(string aliasUrlPath, int editedAliasId, string culture, string extensions, string siteName, bool checkDocumentUrlpath, int nodeId)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                return null;
            }

            aliasUrlPath = GetNormalizedUrlPathInternal(aliasUrlPath);

            var containsWildcard = DocumentURLProvider.ContainsWildcard(aliasUrlPath);

            var aliasWhere = GetMatchingAliasesWhereCondition(aliasUrlPath, editedAliasId, culture, extensions, containsWildcard, nodeId, site.SiteID);

            DataSet matchingAliasesData = GetMatchingAliasData(aliasWhere);

            if (checkDocumentUrlpath)
            {
                // Add matching documents data
                var documentWhere = GetMatchingDocumentWhereCondition(aliasUrlPath, culture, extensions, containsWildcard, site.SiteID);

                DataSet matchingDocumentsData = GetMatchingDocumentsData(documentWhere);

                // Merge datasets
                matchingAliasesData.Merge(matchingDocumentsData);
                matchingAliasesData.AcceptChanges();
            }

            ValidateUrlSectionsCount(aliasUrlPath, matchingAliasesData);

            return matchingAliasesData;
        }


        private static void ValidateUrlSectionsCount(string aliasUrlPath, DataSet matchingAliasCandidates)
        {
            int aliasUrlPathSlashCount = TreePathUtils.GetPathSlashCount(aliasUrlPath);

            foreach (DataRow alias in matchingAliasCandidates.Tables[0].Rows)
            {
                int candidateSlashCount = TreePathUtils.GetPathSlashCount(alias["URLPath"].ToString());
                if (aliasUrlPathSlashCount != candidateSlashCount)
                {
                    alias.Delete();
                }
            }

            if (matchingAliasCandidates.HasChanges())
            {
                matchingAliasCandidates.AcceptChanges();
            }
        }


        /// <summary>
        /// Deletes node aliases for given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        protected virtual void DeleteNodeAliasesInternal(int nodeId)
        {
            // Get aliases
            var aliases = GetDocumentAliases().WhereEquals("AliasNodeID", nodeId);
            if (aliases.Count > 0)
            {
                // For each alias
                foreach (DocumentAliasInfo alias in aliases)
                {
                    if (alias != null)
                    {
                        // Delete document category info
                        DeleteDocumentAliasInfo(alias);
                    }
                }
            }
        }


        /// <summary>
        /// Creates wildcard rule for given URL.
        /// </summary>
        /// <param name="wildcardUrl">Wildcard URL</param>
        /// <param name="wildcardSectionsCount">Number of URL sections containing at least one wildcard.</param>
        protected virtual string CreateWildcardRuleInternal(string wildcardUrl, out int wildcardSectionsCount)
        {
            wildcardSectionsCount = 0;

            if (String.IsNullOrEmpty(wildcardUrl))
            {
                return String.Empty;
            }

            wildcardUrl = SqlHelper.EscapeLikeQueryPatterns(wildcardUrl);

            var re = DocumentURLProvider.WildcardRegex;
            if (!re.IsMatch(wildcardUrl))
            {
                return String.Empty;
            }

            var wildcardRule = re.Replace(wildcardUrl, "%");
            wildcardSectionsCount = GetWildcardSectionsCount(wildcardRule);

            return wildcardRule;
        }


        private int GetWildcardSectionsCount(string wildcardRule)
        {
            return wildcardSectionsRegex.Matches(wildcardRule).Count;
        }


        /// <summary>
        /// Returns URL path without MVC or ROUTE prefix
        /// </summary>
        /// <param name="urlPath">URL path</param>
        protected virtual string GetNormalizedUrlPathInternal(string urlPath)
        {
            // Remove MVC/ROUTE Prefix if present
            if (urlPath.StartsWith(TreePathUtils.URL_PREFIX_ROUTE, StringComparison.InvariantCultureIgnoreCase))
            {
                urlPath = urlPath.Substring(urlPath.LastIndexOf(':') + 1);
            }

            return urlPath;
        }


        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Parameters</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Updates search index.
        /// </summary>
        /// <param name="infoObj">Document alias object</param>
        /// <param name="siteName">Site name</param>
        private void UpdateSearchIndex(DocumentAliasInfo infoObj, string siteName)
        {
            TreeProvider tree = new TreeProvider();

            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                CultureCode = TreeProvider.ALL_CULTURES,
                CombineWithDefaultCulture = false,
                Where = "(NodeID = " + infoObj.AliasNodeID + ")",
            };

            // Get all nodes with given alias
            var result = tree.SelectNodes(parameters);

            // If any found then update search index
            if (!DataHelper.DataSourceIsEmpty(result))
            {
                List<SearchTaskCreationParameters> taskParameters = new List<SearchTaskCreationParameters>();

                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    // Get node
                    TreeNode node = TreeNode.New(Convert.ToString(dr["className"]), dr);

                    // Add node to list of updated nodes if published
                    if ((node != null) && node.PublishedVersionExists)
                    {
                        taskParameters.Add(new SearchTaskCreationParameters
                        {
                            TaskType = SearchTaskTypeEnum.Update,
                            ObjectType = TreeNode.OBJECT_TYPE,
                            ObjectField = SearchFieldsConstants.ID,
                            TaskValue = node.GetSearchID(),
                            RelatedObjectID = node.DocumentID
                        });
                    }
                }

                SearchTaskInfoProvider.CreateTasks(taskParameters, true);
            }
        }


        private static string GetCulturePriority(string siteName, string currentCultureCode, bool combineWithDefaultCulture)
        {
            string defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

            var culturePriority = DocumentQueryColumnBuilder.GetCulturePriorityColumn("AliasCulture", currentCultureCode, defaultCulture, combineWithDefaultCulture).GetExpression();
            return culturePriority;
        }


        /// <summary>
        /// Gets the where condition which limits aliases that are relevant to given URL and site (potential candidates).
        /// </summary>
        internal static WhereCondition GetDocumentAliasWhereCondition(string path, string siteName)
        {
            // Uses "AsColumn()" because specified column in database contains pattern for URL.
            return new WhereCondition()
                .WhereEquals("AliasSiteID", SiteInfoProvider.GetSiteID(siteName))
                .And(
                    new WhereCondition()
                        .WhereEquals(path.AsValue(), "AliasURLPath".AsColumn())
                        .Or().Where(path.AsValue(), QueryOperator.Like, "AliasWildcardRule".AsColumn())
                );
        }


        private static DataSet GetMatchingDocumentsData(WhereCondition documentWhereCondition)
        {
            var tree = new TreeProvider();
            var matchingDocuments = tree.SelectNodes()
                .Columns(
                    new QueryColumn("DocumentUrlPath").As("URLPath"),
                    new QueryColumn("NodeID"),
                    "0".AsExpression().AsColumn("IsDocumentAlias"))
                .All()
                .Where(documentWhereCondition);

            matchingDocuments.Properties.EnsureExtraColumns = false;

            return matchingDocuments.Result;
        }


        private static DataSet GetMatchingAliasData(WhereCondition where)
        {
            return GetDocumentAliases()
                .Columns(
                    new QueryColumn("AliasUrlPath").As("URLPath"),
                    new QueryColumn("AliasNodeID").As("NodeID"),
                    "1".AsExpression().AsColumn("IsDocumentAlias"))
                .Where(where)
                .Result;
        }


        internal static WhereCondition GetMatchingDocumentWhereCondition(string aliasUrlPath, string culture, string extensions, bool containsWildcard, int siteID)
        {
            WhereCondition whereCondition = new WhereCondition()
                .WhereEquals("NodeSiteID", siteID)
                .WhereNotEquals("DocumentWildcardRule".AsColumn().IsNull(String.Empty), String.Empty);

            // If culture is specified, check the specified culture and all cultures
            if (!String.IsNullOrEmpty(culture))
            {
                var cultureWhere = new WhereCondition()
                    .WhereEquals("DocumentCulture", culture);

                whereCondition.And().Where(cultureWhere);
            }

            // Format extension where condition for documents
            if (!String.IsNullOrEmpty(extensions))
            {
                var extensionWhere = GetExtensionWhereCondition(extensions, "DocumentExtensions");
                whereCondition.And().Where(extensionWhere);
            }

            WhereCondition whereMatchPath = GetMatchPathWhereCondition("DocumentUrlPath", "DocumentWildcardRule", aliasUrlPath, containsWildcard);

            whereCondition.And().Where(whereMatchPath);

            return whereCondition;
        }


        internal static WhereCondition GetMatchingAliasesWhereCondition(string aliasUrlPath, int editedAliasId, string culture, string extensions, bool containsWildcard, int nodeId, int siteID)
        {
            WhereCondition whereCondition = new WhereCondition()
                .WhereEquals("AliasSiteID", siteID)
                .WhereNotEquals("AliasWildcardRule".AsColumn().IsNull(String.Empty), String.Empty);

            if (editedAliasId > 0)
            {
                whereCondition.And().WhereNotEquals("AliasID", editedAliasId);
            }

            // If culture is specified, check the specified culture and all cultures
            if (!String.IsNullOrEmpty(culture))
            {
                var cultureWhere = new WhereCondition()
                    .WhereEquals("AliasCulture", culture)
                    .Or().WhereEquals("AliasCulture".AsColumn().IsNull(String.Empty), String.Empty);

                whereCondition.And().Where(cultureWhere);
            }

            if (!String.IsNullOrEmpty(extensions))
            {
                var extensionWhere = GetExtensionWhereCondition(extensions, "AliasExtensions");
                whereCondition.And().Where(extensionWhere);
            }

            // Do not check current document aliases
            if (nodeId > 0)
            {
                var nodeWhere = new WhereCondition()
                    .WhereNotEquals("AliasNodeID", nodeId);

                whereCondition.Where(nodeWhere);
            }

            WhereCondition whereMatchPath = GetMatchPathWhereCondition("AliasUrlPath", "AliasWildcardRule", aliasUrlPath, containsWildcard);

            whereCondition.And().Where(whereMatchPath);

            return whereCondition;
        }


        internal static WhereCondition GetMatchPathWhereCondition(string columnName, string wildcardRuleColumnName, string aliasUrlPath, bool containsWildcard)
        {
            if (containsWildcard)
            {
                var wildcardRule = CreateWildcardRule(aliasUrlPath, out _);

                return new WhereCondition()

                    .Where(columnName, QueryOperator.Like, wildcardRule)
                    .Or().Where(columnName, QueryOperator.Like, "%:" + wildcardRule);
            }

            var escapedUrlPath = SqlHelper.EscapeLikeQueryPatterns(aliasUrlPath);

            return new WhereCondition()
                .Where(columnName, QueryOperator.Like, escapedUrlPath)
                .Or().Where(columnName, QueryOperator.Like, "%:" + escapedUrlPath)
                .Or().Where(aliasUrlPath.AsValue(), QueryOperator.Like, wildcardRuleColumnName.AsColumn());
        }


        internal static WhereCondition GetExtensionWhereCondition(string extensions, string columnName)
        {
            var whereCondition = new WhereCondition();

            if (String.IsNullOrEmpty(extensions))
            {
                return whereCondition;
            }

            var extArr = extensions.Split(';');
            foreach (var extension in extArr)
            {
                var subcomplete = new WhereCondition()
                    .Where(x => x.WhereEquals(columnName, extension))
                    .Or(x => x.WhereStartsWith(columnName, extension + ";"))
                    .Or(x => x.WhereContains(columnName, ";" + extension + ";"))
                    .Or(x => x.WhereEndsWith(columnName, ";" + extension));

                whereCondition = whereCondition.Or(subcomplete);
            }

            return new WhereCondition()
                .WhereEmpty(columnName)
                .Or(whereCondition);
        }

        #endregion
    }
}