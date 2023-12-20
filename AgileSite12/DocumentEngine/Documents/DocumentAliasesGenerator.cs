using System;
using System.Text;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Generates document aliases based on document changes.
    /// </summary>
    internal sealed class DocumentAliasesGenerator
    {
        private TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Size of batch used in method <see cref="GenerateNodeAliasPathAliases"/>.
        /// </summary>
        internal int BatchSize { get; set; } = TreeProvider.PROCESSING_BATCH;


        /// <summary>
        /// Creates instance of <see cref="DocumentAliasesGenerator"/>.
        /// </summary>
        /// <param name="node">Document which changes need to be propagated to the descendants</param>
        public DocumentAliasesGenerator(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "The document instance needs to be provided.");
            }

            Node = node;
        }


        /// <summary>
        /// Generates document alias based on node alias path change when new culture version is being created.
        /// </summary>
        public void GenerateAliasForNewCultureVersion()
        {
            var originalNodeAliasPath = Node.GetOriginalValue("NodeAliasPath").ToString(String.Empty);
            bool nodeAliasPathChanged = !string.IsNullOrEmpty(originalNodeAliasPath) && !Node.NodeAliasPath.EqualsCSafe(originalNodeAliasPath, true);
            if (!nodeAliasPathChanged)
            {
                return;
            }

            var alias = new DocumentAliasInfo
            {
                AliasURLPath = originalNodeAliasPath,
                AliasWildcardRule = Node.DocumentWildcardRule,
                AliasNodeID = Node.NodeID,
                AliasSiteID = Node.NodeSiteID,
                AliasExtensions = string.Empty
            };

            // Generate alias only if unique
            var siteName = Node.GetSiteName();
            if (DocumentAliasInfoProvider.IsUnique(alias.AliasURLPath, 0, null, alias.AliasExtensions, siteName, true, alias.AliasNodeID))
            {
                DocumentAliasInfoProvider.SetDocumentAliasInfo(alias);
            }
        }


        /// <summary>
        /// Generates document aliases based on node alias and document URL path change when new document is being updated.
        /// </summary>
        public void GenerateAliasesForUpdate()
        {
            GenerateNodeAliasPathAlias();
            GenerateDocumentUrlPathAlias();
        }


        /// <summary>
        /// Changes site of the already created document aliases
        /// </summary>
        public void ChangeCurrentAliasesSite()
        {
            var siteChanged = Node.GetOriginalValue("NodeSiteID").ToInteger(0) != Node.NodeSiteID;
            if (!siteChanged)
            {
                return;
            }

            var where = new WhereCondition().WhereIn("AliasNodeID", Node.GetNodeIDsQueryByPathAndSite());
            var parameters = where.Parameters;
            parameters.Add("@NewSiteID", Node.NodeSiteID);
            DocumentAliasInfoProvider.UpdateData("AliasSiteID = @NewSiteID", where.ToString(), where.Parameters);
        }


        private void GenerateNodeAliasPathAlias()
        {
            var originalExtensions = Node.GetOriginalValue("DocumentExtensions").ToString(String.Empty);
            var originalUrlPath = Node.GetOriginalValue("DocumentUrlPath").ToString(String.Empty);
            var originalNodeAliasPath = Node.GetOriginalValue("NodeAliasPath").ToString(String.Empty);
            bool extensionsChanged = !string.IsNullOrEmpty(originalExtensions) && !Node.DocumentExtensions.EqualsCSafe(originalExtensions, true);
            bool nodeAliasPathChanged = !string.IsNullOrEmpty(originalNodeAliasPath) && !Node.NodeAliasPath.EqualsCSafe(originalNodeAliasPath, true);
            bool documentUrlPathChanged = !string.IsNullOrEmpty(originalUrlPath) && !Node.DocumentUrlPath.EqualsCSafe(originalUrlPath, true);

            // Create new document alias for all cultures
            if (!nodeAliasPathChanged && (!extensionsChanged || documentUrlPathChanged))
            {
                return;
            }

            var alias = new DocumentAliasInfo
            {
                AliasURLPath = originalNodeAliasPath,
                AliasWildcardRule = Node.DocumentWildcardRule,
                AliasNodeID = Node.NodeID,
                AliasSiteID = Node.NodeSiteID,
                AliasExtensions = originalExtensions
            };

            // If the alias does not yet exist, create it
            var siteName = Node.GetSiteName();
            if (DocumentAliasInfoProvider.IsUnique(alias.AliasURLPath, 0, null, alias.AliasExtensions, siteName, true, alias.AliasNodeID))
            {
                DocumentAliasInfoProvider.SetDocumentAliasInfo(alias);
            }
        }


        private void GenerateDocumentUrlPathAlias()
        {
            var originalExtensions = Node.GetOriginalValue("DocumentExtensions").ToString(String.Empty);
            var originalUrlPath = Node.GetOriginalValue("DocumentUrlPath").ToString(String.Empty);
            var originalNodeAliasPath = Node.GetOriginalValue("NodeAliasPath").ToString(String.Empty);
            bool extensionsChanged = !string.IsNullOrEmpty(originalExtensions) && !Node.DocumentExtensions.EqualsCSafe(originalExtensions, true);
            bool nodeAliasPathChanged = !string.IsNullOrEmpty(originalNodeAliasPath) && !Node.NodeAliasPath.EqualsCSafe(originalNodeAliasPath, true);
            bool documentUrlPathChanged = !string.IsNullOrEmpty(originalUrlPath) && !Node.DocumentUrlPath.EqualsCSafe(originalUrlPath, true);
            bool pathsAreDifferent = originalNodeAliasPath != originalUrlPath;

            // Create new document alias for specific culture (if document URL path is changed and document extension is changed and original node alias path is different than document URL path to avoid duplicity)
            if ((!documentUrlPathChanged || !pathsAreDifferent) && (!documentUrlPathChanged || !extensionsChanged || nodeAliasPathChanged))
            {
                return;
            }

            var alias = new DocumentAliasInfo
            {
                AliasURLPath = originalUrlPath,
                AliasWildcardRule = Node.DocumentWildcardRule,
                AliasNodeID = Node.OriginalNodeID,
                AliasCulture = Node.DocumentCulture,
                AliasSiteID = Node.NodeSiteID,
                AliasExtensions = originalExtensions
            };

            // If the alias does not yet exist, create it
            var siteName = Node.GetSiteName();
            if (!AliasExists(alias) && DocumentAliasInfoProvider.IsUnique(alias.AliasURLPath, 0, Node.DocumentCulture, alias.AliasExtensions, siteName, true, alias.AliasNodeID))
            {
                DocumentAliasInfoProvider.SetDocumentAliasInfo(alias);
            }
        }


        private bool AliasExists(DocumentAliasInfo alias)
        {
            return DocumentAliasInfoProvider.GetDocumentAliases()
                                            .TopN(1)
                                            .Column("AliasID")
                                            .WhereEquals("AliasURLPath", alias.AliasURLPath)
                                            .WhereEquals("AliasNodeID", alias.AliasNodeID)
                                            .WhereEquals("AliasExtensions", alias.AliasExtensions)
                                            .Where(new WhereCondition()
                                                .WhereEmpty("AliasCulture")
                                                .Or()
                                                .WhereEquals("AliasCulture", Node.DocumentCulture))
                                            .Count > 0;
        }


        /// <summary>
        /// Generates document aliases for documents based on their current node alias path.
        /// </summary>
        /// <param name="where">Where condition to limit the set of documents.</param>
        public void GenerateNodeAliasPathAliases(WhereCondition where)
        {
            var idsOfDocumentsWithExistingAliases =
                DocumentHelper.GetDocuments()
                              .All()
                              .Distinct()
                              .Where(where)
                              .WhereFalse("NodeIsContentOnly")
                              .WhereExists(
                                  DocumentAliasInfoProvider.GetDocumentAliases()
                                                           .WhereEquals("AliasNodeID", "NodeID".AsColumn())
                                                           .WhereEquals("AliasURLPath", "NodeAliasPath".AsColumn())
                                                           .WhereEquals(
                                                               "AliasExtensions".AsColumn().IsNull(""),
                                                               "DocumentExtensions".AsColumn().IsNull("")
                                                           )
                              )
                              .AsSingleColumn("NodeID")
                              .GetListResult<int>();
            // Generate aliases for child documents which will change their alias path.
            // There is no need to generate aliases if the document was moved to the same location (even between sites)
            var aliasesData =
                DocumentHelper.GetDocuments()
                              .All()
                              .Distinct()
                              .Columns(
                                  new QueryColumn("NodeID").As("AliasNodeID"),
                                  new QueryColumn("NodeAliasPath").As("AliasUrlPath"),
                                  new QueryColumn("DocumentExtensions").As("AliasExtensions")
                              )
                              .Where(where)
                              .WhereFalse("NodeIsContentOnly")
                              .WhereNotIn("NodeID", idsOfDocumentsWithExistingAliases)
                              .AsNested<ObjectQuery<DocumentAliasInfo>>();

            // Generate new aliases in batches
            aliasesData.ForEachPage(
                q =>
                {
                    var newAliases = q.TypedResult;

                    foreach (var alias in newAliases)
                    {
                        alias.AliasSiteID = Node.NodeSiteID;
                        DocumentAliasInfoProvider.SetWildcardRuleWithPriority(alias);
                    }

                    DocumentAliasInfoProvider.BulkInsert(newAliases);
                },
                BatchSize
                );
        }
    }
}
