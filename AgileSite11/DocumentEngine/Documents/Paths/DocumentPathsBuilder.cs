using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides document paths based on document configuration and properties.
    /// </summary>
    internal sealed class DocumentPathsBuilder
    {
        private TreeNode Node
        {
            get;
            set;
        }


        private string SiteName
        {
            get;
            set;
        }


        private string DefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of <see cref="DocumentPathsBuilder"/>.
        /// </summary>
        /// <param name="node">Document which changes need to be propagated to the descendants</param>
        public DocumentPathsBuilder(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node", "The document instance needs to be provided.");
            }

            Node = node;
            SiteName = node.GetSiteName();
            DefaultCulture = CultureHelper.GetDefaultCultureCode(SiteName);
        }


        /// <summary>
        /// Gets document automatic URL path
        /// </summary>
        public string GetUrlPath()
        {
            if (Node.IsRoot())
            {
                return string.Empty;
            }

            var parent = Node.Parent;
            if (parent == null)
            {
                throw new NullReferenceException("Missing node parent.");
            }

            var paths = GetParentPathsConfiguration(parent.NodeData, parent.CultureData);
            return GetPathFromParentPath(paths.GetUrlPath(SiteName));
        }


        internal DocumentPathsConfiguration GetParentPathsConfiguration(DocumentNodeDataInfo parentNodeData, DocumentCultureDataInfo parentCultureData)
        {
            return IsCurrentParentBestMatchForNamePath(parentCultureData) ? GetCurrentParentPaths(parentCultureData) : GetBestMatchingParentPaths(parentNodeData.NodeOriginalNodeID);
        }


        /// <summary>
        /// Gets prefixes to be used for paths update in context of document delete action.
        /// </summary>
        public DocumentPathPrefixes GetDocumentPrefixesForDelete()
        {
            var prefixPaths = GetBestMatchingTranslatedPaths(Node.OriginalNodeID, true);
            if (prefixPaths == null)
            {
                return null;
            }

            return new DocumentPathPrefixes
            {
                // Current path prefix needs to be replaced by the prefix of best matching culture version available after the deletion
                new DocumentPathPrefixes.Prefix
                {
                    Culture = Node.DocumentCulture,
                    NamePath = new DocumentPathPrefixes.Path(Node.DocumentNamePath, prefixPaths.NamePath),
                    UrlPath = new DocumentPathPrefixes.Path(GetDocumentUrlPath(), prefixPaths.GetUrlPath(SiteName))
                }
            };
        }


        /// <summary>
        /// Gets prefixes to be used for paths update in context of new document culture creation action.
        /// </summary>
        public DocumentPathPrefixes GetDocumentPrefixesForNewCultureVersion()
        {
            var prefixPaths = GetBestMatchingTranslatedPaths(Node.OriginalNodeID, true);
            if (prefixPaths == null)
            {
                return null;
            }

            return new DocumentPathPrefixes
            {
                // Path prefix of the best matching culture version used so far needs to be replaced by the path prefix of newly created culture version
                new DocumentPathPrefixes.Prefix
                {
                    Culture = Node.DocumentCulture,
                    NamePath = new DocumentPathPrefixes.Path(prefixPaths.NamePath, Node.DocumentNamePath),
                    UrlPath = new DocumentPathPrefixes.Path(prefixPaths.GetUrlPath(SiteName), GetDocumentUrlPath())
                }
            };
        }


        /// <summary>
        /// Gets prefixes to be used for paths update in a culture version in context of document update action.
        /// </summary>
        public DocumentPathPrefixes GetDocumentPrefixesForCultureVersion()
        {
            return new DocumentPathPrefixes
            {
                new DocumentPathPrefixes.Prefix
                {
                    Culture = Node.DocumentCulture,
                    NamePath = new DocumentPathPrefixes.Path(Node.GetOriginalValue("DocumentNamePath").ToString(""), Node.DocumentNamePath),
                    UrlPath = new DocumentPathPrefixes.Path(GetDocumentOriginalUrlPath(), GetDocumentUrlPath())
                }
            };
        }


        /// <summary>
        /// Gets prefixes to be used for paths update in all culture versions in context of document update action.
        /// </summary>
        public DocumentPathPrefixes GetDocumentPrefixesForAllCultureVersions()
        {
            var prefixes = new DocumentPathPrefixes();
            var originalPaths = GetPrefixPaths(Node.GetOriginalValue("NodeParentID").ToInteger(0));
            var targetPaths = GetPrefixPaths(Node.NodeParentID);

            var cultures = Node.GetTranslatedCultures();
            foreach (var culture in cultures)
            {
                var prefix = GetDocumentPrefixForCultureVersion(culture, originalPaths, targetPaths);
                prefixes.Add(prefix);
            }

            return prefixes;
        }


        private bool IsCurrentParentBestMatchForNamePath(DocumentCultureDataInfo parentCultureData)
        {
            if (parentCultureData == null)
            {
                return false;
            }

            // Current parent path can be used for original document when located in first level or parent culture matches the document culture
            return !Node.IsLink && ((Node.NodeLevel <= 1) || parentCultureData.DocumentCulture.EqualsCSafe(Node.DocumentCulture, true));
        }


        private DocumentPathsConfiguration GetCurrentParentPaths(DocumentCultureDataInfo parentCultureData)
        {
            // Parent document paths for original document in first level are hardcoded
            if (!Node.IsLink && (Node.NodeLevel <= 1))
            {
                return new DocumentPathsConfiguration(true, "/", string.Empty);
            }

            if (parentCultureData == null)
            {
                return null;
            }

            return new DocumentPathsConfiguration(parentCultureData.DocumentUseNamePathForUrlPath, parentCultureData.DocumentNamePath, parentCultureData.DocumentUrlPath);
        }


        private DocumentPathsConfiguration GetBestMatchingParentPaths(int parentId)
        {
            if (Node.IsLink)
            {
                // Paths need to be evaluated in context of original document parent
                parentId = GetOriginalNodeParentId();
            }

            var paths = GetBestMatchingTranslatedPaths(parentId, false);
            if (paths == null)
            {
                throw new NullReferenceException("No parent document name path found.");
            }

            return paths;
        }


        private int GetOriginalNodeParentId()
        {
            var linkData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(Node.NodeLinkedNodeID);
            if (linkData == null)
            {
                throw new NullReferenceException("The original node for a linked document doesn't exist.");
            }

            return linkData.NodeParentID;
        }


        private DocumentPathsConfiguration GetBestMatchingTranslatedPaths(int nodeId, bool excludeCurrentDocumentCulture)
        {
            var query = GetTranslatedDataQuery(nodeId)
                .TopN(1)
                .Columns("DocumentNamePath", "DocumentUrlPath", "DocumentUseNamePathForUrlPath")
                .OrderByDescending(DocumentQueryColumnBuilder.GetCulturePriorityColumn("DocumentCulture", new[] { Node.DocumentCulture, DefaultCulture }).ToString())
                .OrderByAscending("DocumentCulture");

            if (excludeCurrentDocumentCulture)
            {
                query.WhereNotEquals("DocumentCulture", Node.DocumentCulture);
            }

            var data = query.Result;
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            var row = data.Tables[0].Rows[0];
            var namePath = row["DocumentNamePath"].ToString("");
            var urlPath = row["DocumentUrlPath"].ToString("");
            var useNamePathForUrlPath = row["DocumentUseNamePathForUrlPath"].ToBoolean(false);
            return new DocumentPathsConfiguration(useNamePathForUrlPath, namePath, urlPath);
        }


        private ObjectQuery<DocumentCultureDataInfo> GetTranslatedDataQuery(int nodeId)
        {
            return DocumentCultureDataInfoProvider.GetDocumentCultures().WhereEquals("DocumentNodeID", nodeId);
        }


        private string GetPathFromParentPath(string parentPath)
        {
            return string.Join("/", parentPath, TreePathUtils.GetSafeDocumentName(Node.DocumentName, SiteName));
        }


        private string GetDocumentUrlPath()
        {
            var paths = new DocumentPathsConfiguration(Node.DocumentUseNamePathForUrlPath, Node.DocumentNamePath, Node.DocumentUrlPath);
            return paths.GetUrlPath(SiteName);
        }


        private string GetDocumentOriginalUrlPath()
        {
            var originalUseNamePathForUrlPath = Node.GetOriginalValue("DocumentUseNamePathForUrlPath").ToBoolean(false);
            var originalNamePath = Node.GetOriginalValue("DocumentNamePath").ToString("");
            var originalUrlPath = Node.GetOriginalValue("DocumentUrlPath").ToString("");
            var paths = new DocumentPathsConfiguration(originalUseNamePathForUrlPath, originalNamePath, originalUrlPath);

            return paths.GetUrlPath(SiteName);
        }


        private DocumentPathPrefixes.Prefix GetDocumentPrefixForCultureVersion(string culture, SortedDictionary<string, DocumentPathsConfiguration> originalPaths, SortedDictionary<string, DocumentPathsConfiguration> targetPaths)
        {
            var original = GetBestMatchingPrefixPathsForCulture(culture, originalPaths);
            var target = GetBestMatchingPrefixPathsForCulture(culture, targetPaths);
            var originalUrlPath = original.GetUrlPath(SiteName);
            var targetUrlPath = target.GetUrlPath(SiteName);

            return new DocumentPathPrefixes.Prefix
            {
                Culture = culture,
                NamePath = new DocumentPathPrefixes.Path(original.NamePath, target.NamePath),
                UrlPath = new DocumentPathPrefixes.Path(originalUrlPath, targetUrlPath)
            };
        }


        /// <summary>
        /// Gets best matching paths from the dictionary of candidates for given culture.
        /// </summary>
        /// <param name="culture">Document culture</param>
        /// <param name="paths">Dictionary of all possible candidates</param>
        /// <returns>Best matching candidated based on this priorities:
        /// 1. Paths for given culture
        /// 2. Paths for default culture
        /// 3. Paths for the first culture in the candidates</returns>
        private DocumentPathsConfiguration GetBestMatchingPrefixPathsForCulture(string culture, SortedDictionary<string, DocumentPathsConfiguration> paths)
        {
            if (paths.Keys.Contains(culture, StringComparer.InvariantCultureIgnoreCase))
            {
                return paths[culture];
            }

            if (paths.Keys.Contains(DefaultCulture, StringComparer.InvariantCultureIgnoreCase))
            {
                return paths[DefaultCulture];
            }

            return paths.First().Value;
        }


        /// <summary>
        /// Gets name and URL path for each culture version of a document.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>Sorted dictionary of parent document paths for each culture version.</returns>
        private SortedDictionary<string, DocumentPathsConfiguration> GetPrefixPaths(int nodeId)
        {
            var paths = new SortedDictionary<string, DocumentPathsConfiguration>(StringComparer.InvariantCultureIgnoreCase);
            var parent = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            var originalNodeId = parent.NodeOriginalNodeID;

            GetTranslatedDataQuery(originalNodeId)
                .Columns("DocumentCulture", "DocumentNamePath", "DocumentUrlPath", "DocumentUseNamePathForUrlPath")
                .ForEachRow(row =>
                {
                    var culture = row["DocumentCulture"].ToString("");
                    var namePath = row["DocumentNamePath"].ToString("");
                    var urlPath = row["DocumentUrlPath"].ToString("");
                    var useNamePath = row["DocumentUseNamePathForUrlPath"].ToBoolean(false);
                    paths.Add(culture, new DocumentPathsConfiguration(useNamePath, namePath, urlPath));
                });

            return paths;
        }
    }
}
