using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Updates NodeAliasPath, DocumentNamePath and DocumentUrlPath properties of the document in rest of the culture versions of given document and all the descending documents.
    /// </summary>
    /// <remarks>If necessary, document aliases are generated to ensure the documents to be accessible via the original path.</remarks>
    internal sealed class DocumentPathsUpdater
    {
        private TreeNode Node
        {
            get;
            set;
        }


        private TreeProvider TreeProvider
        {
            get;
            set;
        }


        private string SiteName
        {
            get;
            set;
        }


        private bool GenerateAliases
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="DocumentPathsUpdater"/>.
        /// </summary>
        /// <param name="node">Document which changes need to be propagated to the descendants</param>
        public DocumentPathsUpdater(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "The document instance needs to be provided.");
            }

            Node = node;
            TreeProvider = node.TreeProvider;

            SiteName = node.GetSiteName();
            GenerateAliases = !Node.NodeIsContentOnly && TreeProvider.EnableDocumentAliases && TreePathUtils.KeepChangedDocumentsAccessible(SiteName);
        }


        /// <summary>
        /// Updates the paths for rest of the culture versions of given document and its descendants when document gets updated. 
        /// Generates document aliases if necessary.
        /// </summary>
        public void UpdateOtherCultureVersionsAndDescendantsPathsForUpdate()
        {
            if (TreeProvider.UpdatePaths)
            {
                UpdateOtherCultureVersionsAndDescendantsAllPaths();
            }

            var aliasesGenerator = new DocumentAliasesGenerator(Node);
            aliasesGenerator.ChangeCurrentAliasesSite();

            if (GenerateAliases)
            {
                aliasesGenerator.GenerateAliasesForUpdate();
            }
        }


        /// <summary>
        /// Updates the paths for document descendants when new culture version is created. 
        /// Generates document alias based on node alias path change if necessary.
        /// </summary>
        public void UpdateDescendantsPathsForNewCultureVersion()
        {
            if (TreeProvider.UpdatePaths)
            {
                UpdateDescendantsAllPathsForNewCultureVersion();
            }

            if (GenerateAliases)
            {
                var aliasesGenerator = new DocumentAliasesGenerator(Node);
                aliasesGenerator.GenerateAliasForNewCultureVersion();
            }
        }


        /// <summary>
        /// Updates the paths for document descendants when a culture version is deleted.
        /// </summary>
        public void UpdateDescendantsPathsForDelete()
        {
            if (!TreeProvider.UpdatePaths)
            {
                return;
            }

            // Child documents either of the original or linked document are influenced
            if (!Node.NodeHasChildren && !Node.IsLink && !Node.NodeHasLinks)
            {
                return;
            }

            var pathsBuilder = new DocumentPathsBuilder(Node);
            var prefixes = pathsBuilder.GetDocumentPrefixesForDelete();
            UpdateDocumentPaths(prefixes, false);
        }


        /// <summary>
        /// Updates paths for document when inserting a new document.
        /// </summary>
        /// <param name="parent">Parent document where the document is being inserted</param>
        /// <param name="ensureAlias">Indicates if node alias value should be ensured</param>
        public void UpdateDocumentPathsForInsert(TreeNode parent, bool ensureAlias)
        {
            UpdateDocumentNodeAliasForInsert(ensureAlias);
            UpdateDocumentNodeAliasPath(parent.NodeData);
            EnsureDocumentUseNamePathForUrlPathValue();

            if (!TreeProvider.UpdatePaths)
            {
                return;
            }

            UpdateDocumentPathsInternal(parent.NodeData, parent.CultureData);
        }


        /// <summary>
        /// Updates node alias value based on document name changes.
        /// </summary>
        public void UpdateDocumentNodeAlias()
        {
            if (Node.IsDefaultCulture && TreeProvider.AutomaticallyUpdateDocumentAlias && TreePathUtils.AutomaticallyUpdateDocumentAlias(SiteName))
            {
                UpdateNodeAliasFromSourceField();
            }

            Node.NodeAlias = GetValidNodeAlias(Node.NodeID);
        }


        /// <summary>
        /// Updates node alias path for insert if path is based on ID column of a document.
        /// </summary>
        /// <returns>Returns <c>true</c> if node data should be updated.</returns>
        public bool UpdateDocumentNodeAliasPathBasedOnIDForInsert(DocumentNodeDataInfo parentNodeData)
        {
            if (!Node.ClassNodeAliasSourceDefined())
            {
                return false;
            }

            if (!IsNodeAliasSourceIDField())
            {
                return false;
            }

            UpdateDocumentNodeAliasForInsert(true);
            UpdateDocumentNodeAliasPath(parentNodeData);

            return true;
        }


        private string GetNodeAliasSource()
        {
            return Node.ClassNodeAliasSourceDefined() ? Node.DataClassInfo.ClassNodeAliasSource : "NodeName";
        }


        private void UpdateNodeAliasFromSourceField()
        {
            var aliasSource = GetNodeAliasSource();
            Node.NodeAlias = DataHelper.GetNotEmpty(Node.GetValue(aliasSource), Node.DocumentName);
        }


        private string GetValidNodeAlias(int nodeId)
        {
            var nodeAlias = Node.NodeAlias;

            if (TreeProvider.CheckUniqueNames)
            {
                nodeAlias = TreePathUtils.GetUniqueNodeAlias(nodeAlias, SiteName, Node.NodeParentID, nodeId);
            }
            else
            {
                if (TreeProvider.EnsureSafeNodeAlias)
                {
                    nodeAlias = TreePathUtils.GetSafeNodeAlias(nodeAlias, SiteName);
                }
                nodeAlias = TreePathUtils.EnsureMaxNodeAliasLength(nodeAlias);
            }

            return nodeAlias;
        }


        private void UpdateDocumentNodeAliasForInsert(bool ensureAlias)
        {
            if (ensureAlias)
            {
                UpdateNodeAliasFromSourceField();
            }
            Node.NodeAlias = GetValidNodeAlias(0);
        }


        /// <summary>
        /// Updates paths for document when inserting a link.
        /// </summary>
        /// <param name="parentNodeData">Node data of the parent document</param>
        public void UpdateDocumentPathsForLink(DocumentNodeDataInfo parentNodeData)
        {
            Node.NodeAlias = GetValidNodeAlias(0);
            UpdateDocumentNodeAliasPath(parentNodeData);
        }


        private bool IsNodeAliasSourceIDField()
        {
            var aliasSource = Node.DataClassInfo.ClassNodeAliasSource;

            return aliasSource.EqualsCSafe("NodeID", true) ||
                   aliasSource.EqualsCSafe("DocumentID", true) ||
                   aliasSource.EqualsCSafe("DocumentForeignKeyValue") ||
                   (Node.IsCoupled && aliasSource.EqualsCSafe(Node.CoupledData.TypeInfo.IDColumn, true));
        }


        /// <summary>
        /// Updates paths for document when inserting a new culture version of a document.
        /// </summary>
        /// <param name="parentNodeData">Node data of the parent document</param>
        public void UpdateDocumentPathsForNewCultureVersion(DocumentNodeDataInfo parentNodeData)
        {
            UpdateDocumentNodeAliasPath(parentNodeData);
            EnsureDocumentUseNamePathForUrlPathValue();

            if (!TreeProvider.UpdatePaths)
            {
                return;
            }

            UpdateDocumentPathsInternal(parentNodeData);
        }


        /// <summary>
        /// Updates paths for document when updating document properties.
        /// </summary>
        /// <param name="parentNodeData">Node data of the parent document</param>
        public void UpdateDocumentPathsForUpdate(DocumentNodeDataInfo parentNodeData)
        {
            UpdateDocumentNodeAliasPath(parentNodeData);
            EnsureDocumentUseNamePathForUrlPathValue();

            if (!TreeProvider.UpdatePaths)
            {
                return;
            }

            // Name path is changed only if document parent is changed or document name is changed or name path is missing
            var updateDocumentNamePath = string.IsNullOrEmpty(Node.DocumentNamePath) || ParentChanged() || Node.ItemChanged("DocumentName");
            // URL path is changed if name path is changed or document configuration changed to use automatic URL path
            var updateDocumentUrlPath = updateDocumentNamePath || (Node.ItemChanged("DocumentUseNamePathForUrlPath") && Node.DocumentUseNamePathForUrlPath);
            UpdateDocumentPathsInternal(parentNodeData, updateDocumentNamePath: updateDocumentNamePath, updateDocumentUrlPath: updateDocumentUrlPath, excludeCurrentDocumentFromGettingUniqueUrlPath: true);
        }


        private void UpdateDocumentPathsInternal(DocumentNodeDataInfo parentNodeData, DocumentCultureDataInfo parentCultureData = null, bool updateDocumentNamePath = true, bool updateDocumentUrlPath = true, bool excludeCurrentDocumentFromGettingUniqueUrlPath = false)
        {
            if (updateDocumentNamePath || updateDocumentUrlPath)
            {
                var pathsBuilder = new DocumentPathsBuilder(Node);
                var paths = pathsBuilder.GetParentPathsConfiguration(parentNodeData, parentCultureData);

                if (updateDocumentNamePath)
                {
                    UpdateDocumentNamePath(paths.NamePath);
                }

                if (updateDocumentUrlPath)
                {
                    UpdateDocumentUrlPath(paths.GetUrlPath(SiteName));
                }
            }

            EnsureSafeAndUniqueDocumentUrlPath(excludeCurrentDocumentFromGettingUniqueUrlPath);
        }


        private void EnsureDocumentUseNamePathForUrlPathValue()
        {
            if (Node.GetValue("DocumentUseNamePathForUrlPath") == null)
            {
                Node.DocumentUseNamePathForUrlPath = TreePathUtils.UseNamePathForUrlPath(SiteName);
            }
        }


        private void UpdateDocumentNodeAliasPath(DocumentNodeDataInfo parentNodeData)
        {
            Node.NodeAliasPath = string.Join("/", parentNodeData.NodeAliasPath.TrimEnd('/'), Node.NodeAlias);
        }


        private void UpdateDocumentNamePath(string parentNamePath)
        {
            if (parentNamePath != null)
            {
                parentNamePath = parentNamePath.TrimEnd('/');
            }

            Node.DocumentNamePath = GetPathFromParentPath(parentNamePath);
        }


        private void UpdateDocumentUrlPath(string parentUrlPath)
        {
            if (!TreeProvider.UpdateUrlPath)
            {
                return;
            }

            if (!Node.DocumentUseNamePathForUrlPath)
            {
                // Custom URL path is used
                return;
            }

            if (parentUrlPath != null)
            {
                parentUrlPath = parentUrlPath.TrimEnd('/');
            }

            Node.DocumentUrlPath = GetPathFromParentPath(parentUrlPath);
        }


        private string GetPathFromParentPath(string parentPath)
        {
            return string.Join("/", parentPath, TreePathUtils.GetSafeDocumentName(Node.DocumentName, SiteName));
        }


        private void EnsureSafeAndUniqueDocumentUrlPath(bool excludeCurrentDocumentFromGettingUniqueUrlPath)
        {
            if (!TreeProvider.UpdateUrlPath)
            {
                return;
            }

            if (!TreeProvider.CheckUniqueNames)
            {
                Node.DocumentUrlPath = TreePathUtils.GetSafeUrlPath(Node.DocumentUrlPath, SiteName);
            }
            else
            {
                // For automatically generated URL path use culture suffix
                var cultureSuffix = Node.DocumentUseNamePathForUrlPath ? CultureHelper.GetShortCultureCode(Node.DocumentCulture) : null;
                var documentId = excludeCurrentDocumentFromGettingUniqueUrlPath ? Node.DocumentID : 0;
                Node.DocumentUrlPath = TreePathUtils.GetUniqueUrlPath(Node.DocumentUrlPath, documentId, SiteName, cultureSuffix);
            }
        }


        private void UpdateOtherCultureVersionsAndDescendantsAllPaths()
        {
            var parentChanged = ParentChanged();

            // Paths are changed if node alias path is changed or parent document is changed
            if (Node.NodeHasChildren && (parentChanged || Node.ItemChanged("NodeAliasPath")))
            {
                UpdateChildrenNodeAliasPath();
            }

            var documentNameChanged = Node.ItemChanged("DocumentName");
            var urlPathChanged = Node.DocumentUseNamePathForUrlPath && Node.ItemChanged("DocumentUrlPath");

            // For link update the paths only when document name or URL path changed, because in fact it is change of the original data.
            if (Node.IsLink)
            {
                // Move of the linked document doesn't influence the document paths of the child documents. These are always built in context of the original document.
                if (documentNameChanged || urlPathChanged)
                {
                    var pathsBuilder = new DocumentPathsBuilder(Node);
                    var prefixes = pathsBuilder.GetDocumentPrefixesForCultureVersion();
                    UpdateDocumentPaths(prefixes, false);
                }
            }
            // For standard documents the paths are always updated when the document is moved. 
            // When document name or URL path is changed, paths are updated only if there are child documents or links available. 
            // Linked documents use paths from their originals and changes need to be reflected to their child documents as well.
            else if (parentChanged || ((documentNameChanged || urlPathChanged) && (Node.NodeHasChildren || Node.NodeHasLinks)))
            {
                var pathsBuilder = new DocumentPathsBuilder(Node);
                var prefixes = parentChanged ? pathsBuilder.GetDocumentPrefixesForAllCultureVersions() : pathsBuilder.GetDocumentPrefixesForCultureVersion();
                UpdateDocumentPaths(prefixes, parentChanged);
            }
        }


        private bool ParentChanged()
        {
            var originalParentId = Node.GetOriginalValue("NodeParentID").ToInteger(0);
            return (originalParentId > 0) && (originalParentId != Node.NodeParentID);
        }


        private void UpdateDescendantsAllPathsForNewCultureVersion()
        {
            if (!Node.NodeHasChildren)
            {
                return;
            }

            if (Node.ItemChanged("NodeAliasPath"))
            {
                UpdateChildrenNodeAliasPath();
            }

            // Child documents either of the original or linked document are influenced
            if (!Node.NodeHasChildren && !Node.IsLink && !Node.NodeHasLinks)
            {
                return;
            }

            var pathsBuilder = new DocumentPathsBuilder(Node);
            var prefixes = pathsBuilder.GetDocumentPrefixesForNewCultureVersion();
            UpdateDocumentPaths(prefixes, false);
        }


        private void UpdateChildrenNodeAliasPath()
        {
            var originalAliasPath = Node.GetOriginalValue("NodeAliasPath").ToString(String.Empty);
            var originalSiteId = Node.GetOriginalValue("NodeSiteID").ToInteger(0);
            var originalNodeLevel = Node.GetOriginalValue("NodeLevel").ToInteger(0);

            var aliasPathChanged = (originalAliasPath != Node.NodeAliasPath);

            // Include only children, current document is not included, as it is handled higher by regular update
            var childWhere = TreePathUtils.GetSubTreeWhereCondition(originalSiteId, originalAliasPath, false);

            // Update Document GUIDs to unique ones if site changed (there may be collisions)
            if (originalSiteId != Node.NodeSiteID)
            {
                var documentWhere =
                    new WhereCondition()
                        .WhereIn(
                            "DocumentNodeID",
                            DocumentNodeDataInfoProvider.GetDocumentNodes()
                                .Where(childWhere)
                                // Skip links, as they will be updated through their originals
                                .WhereNull("NodeLinkedNodeID")
                        );

                DocumentCultureDataInfoProvider.BulkUpdateData(
                    documentWhere,
                    new Dictionary<string, object> {
                        { "DocumentGUID", new QueryExpression("NEWID()") }
                    }
                );
            }

            var values = new Dictionary<string, object>();

            if (aliasPathChanged)
            {
                if (GenerateAliases)
                {
                    var aliasesGenerator = new DocumentAliasesGenerator(Node);
                    aliasesGenerator.GenerateNodeAliasPathAliases(childWhere);
                }

                // Update child path if path changed
                var expr =
                    new QueryExpression(
                        "@NodeAliasPath + RIGHT(NodeAliasPath, LEN(NodeAliasPath) - @OriginalAliasPathLength)",
                        new QueryDataParameters
                        {
                            { "NodeAliasPath", Node.NodeAliasPath },
                            { "OriginalAliasPathLength", originalAliasPath.Length }
                        }
                    );

                values.Add("NodeAliasPath", expr);
            }

            if (originalSiteId != Node.NodeSiteID)
            {
                values.Add("NodeSiteID", Node.NodeSiteID);

                // Update NodeGUID if site changed (there may be collisions)
                values.Add("NodeGUID", new QueryExpression("NEWID()"));
            }

            if (originalNodeLevel != Node.NodeLevel)
            {
                // Update child nodes level if level changed
                var levelDiff = Node.NodeLevel - originalNodeLevel;
                var expr =
                    new QueryExpression(
                        "NodeLevel + @LevelDiff",
                        new QueryDataParameters
                        {
                            { "LevelDiff", levelDiff }
                        }
                    );

                values.Add("NodeLevel", expr);
            }

            DocumentNodeDataInfoProvider.BulkUpdateData(childWhere, values);
        }


        private void UpdateDocumentPaths(DocumentPathPrefixes prefixes, bool allCultureVersions)
        {
            if (prefixes == null)
            {
                return;
            }

            if (!prefixes.Changed())
            {
                // None of the prefixes was changed
                return;
            }

            // This is a bulk update of documents, CI needs to be refreshed for the given set of pages
            // Current document is not included, as it is handled higher by the update itself
            var bulkWhere = GetBulkWhereCondition();
            DocumentContinuousIntegrationHelper.HandleBulkUpdate(true, bulkWhere, () =>
            {
                var aliasIds = ExecuteUpdateDocumentPath(prefixes, allCultureVersions);
                StoreGeneratedAliasesToCIRepository(aliasIds);
            });
        }


        private static void StoreGeneratedAliasesToCIRepository(DataSet aliasIds)
        {
            if (DataHelper.DataSourceIsEmpty(aliasIds))
            {
                return;
            }

            var ids = aliasIds.Tables[0].AsEnumerable().Select(row => row.Field<int>(0)).ToList();
            RepositoryBulkOperations.StoreObjects(DocumentAliasInfo.TYPEINFO, new WhereCondition().WhereIn(DocumentAliasInfo.TYPEINFO.IDColumn, ids));
        }


        private DataSet ExecuteUpdateDocumentPath(DocumentPathPrefixes prefixes, bool allCultureVersions)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@NodeID", allCultureVersions ? Node.OriginalNodeID : 0);
            parameters.Add("@DocumentID", Node.DocumentID);
            parameters.Add("@DefaultCultureCode", CultureHelper.GetDefaultCultureCode(SiteName));
            parameters.Add("@UpdateUrlPath", TreeProvider.UpdateUrlPath);
            parameters.Add("@GenerateAliases", GenerateAliases);
            parameters.Add("@CurrentDate", DateTime.Now);
            parameters.Add("@PrefixesXML", prefixes.Serialize());

            return ConnectionHelper.ExecuteQuery("CMS.Document.UpdateDocumentNamePath", parameters);
        }


        private WhereCondition GetBulkWhereCondition()
        {
            var nodeData = Node.IsLink ? DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(Node.OriginalNodeID) : Node.NodeData;
            var bulkWhere = TreePathUtils.GetSubTreeWhereCondition(nodeData.NodeSiteID, nodeData.NodeAliasPath, false);

            // Generate where conditions to cover links' child documents
            DocumentNodeDataInfoProvider.GetDocumentNodes()
                                        .Columns("NodeAliasPath", "NodeSiteID")
                                        .Where(GetLinksToSectionWhereCondition(nodeData))
                                        .OrderBy("NodeLevel")
                                        .ForEachRow(row =>
                                        {
                                            var siteId = row["NodeSiteID"].ToInteger(0);
                                            var path = row["NodeAliasPath"].ToString("");
                                            bulkWhere.Or(TreePathUtils.GetSubTreeWhereCondition(siteId, path, false));
                                        });

            return bulkWhere;
        }


        private WhereCondition GetLinksToSectionWhereCondition(DocumentNodeDataInfo nodeData)
        {
            var query = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                    .Column("NodeID")
                                                    .Where(TreePathUtils.GetSubTreeWhereCondition(nodeData.NodeSiteID, nodeData.NodeAliasPath, true));

            return new WhereCondition().WhereIn("NodeLinkedNodeID", query);
        }
    }
}
