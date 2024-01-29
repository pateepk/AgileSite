using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides the dependency cache keys for documents
    /// </summary>
    public sealed class DocumentDependencyCacheKeysBuilder
    {
        /// <summary>
        /// Returns the cache dependencies keys array for the node - cache item keys affected when the node is changed.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="siteName">Site name</param>
        internal static List<string> GetNodeDependencyCacheKeys(TreeNode node, string siteName)
        {
            if (node == null)
            {
                return null;
            }

            var dependencies = new List<string>();
            dependencies.AddRange(GetPathDependencyCacheKeys(siteName, node.NodeAliasPath, node.DocumentCulture));
            dependencies.Add(CacheHelper.GetCacheItemName(null, "nodeid", node.NodeID));
            if (node.IsLink)
            {
                dependencies.Add(CacheHelper.GetCacheItemName(null, "nodeid", node.NodeLinkedNodeID));
            }
            dependencies.Add(CacheHelper.GetCacheItemName(null, "documentid", node.DocumentID));
            dependencies.Add(CacheHelper.GetCacheItemName(null, "documentid", node.DocumentID, "attachments"));
            dependencies.Add(GetAllNodesCacheKey(siteName, node.NodeClassName));
            dependencies.Add(CacheHelper.GetCacheItemName(null, "nodeguid", siteName, node.NodeGUID));

            if (node.NodeGroupID > 0)
            {
                dependencies.Add(GetCommunityGroupCacheKey(node.NodeGroupID));
            }

            var childNodesDependencies = TreePathUtils.GetNodeAliasPathsOnPath(node.NodeAliasPath, true, false)
                                                      .Select(p => GetChildNodesCacheKey(siteName, p));
            dependencies.AddRange(childNodesDependencies);

            return dependencies;
        }


        /// <summary>
        /// Returns the cache dependencies keys array for order change in a section.
        /// </summary>
        /// <param name="nodeParentId">Node parent ID</param>
        internal static List<string> GetChangeOrderDependencyCacheKeys(int nodeParentId)
        {
            var nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeParentId);
            if (nodeData == null)
            {
                return null;
            }

            var dependencies = new List<string>();
            dependencies.Add(GetChildNodesCacheKey(nodeData.NodeSiteName, nodeData.NodeAliasPath));
            dependencies.Add(CacheHelper.GetCacheItemName(null, "nodeorder"));

            return dependencies;
        }


        /// <summary>
        /// Returns the cache dependencies keys array for document path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Document path</param>
        /// <param name="cultureCode">Document culture code</param>
        internal static List<string> GetPathDependencyCacheKeys(string siteName, string path, string cultureCode = null)
        {
            var dependencies = new List<string>();
            if (path.EndsWithCSafe("/%"))
            {
                dependencies.Add(GetChildNodesCacheKey(siteName, TreePathUtils.GetParentPath(path)));
            }
            else
            {
                dependencies.Add(CacheHelper.GetCacheItemName(null, "node", siteName, path));
                if (cultureCode != null)
                {
                    dependencies.Add(CacheHelper.GetCacheItemName(null, "node", siteName, path, cultureCode));
                }
            }

            return dependencies;
        }


        /// <summary>
        /// Returns cache dependencies keys for all parent documents on give path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="path">Document path</param>
        public static List<string> GetParentPathsDependencyCacheKeys(string siteName, string path)
        {
            var dependencies = new List<string>();
            var parentPaths = TreePathUtils.GetNodeAliasPathsOnPath(path, true, true);
            foreach (var parentPath in parentPaths)
            {
                dependencies.AddRange(GetPathDependencyCacheKeys(siteName, parentPath));
            }

            return dependencies;
        }


        /// <summary>
        /// Returns cache dependencies keys for the documents set based on given parameters.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="classNames">List of class names separated by semicolon. Pass null or empty string to cover all classes.</param>
        /// <param name="path">Document path</param>
        /// <param name="groupId">Community group ID</param>
        public static List<string> GetDependencyCacheKeys(string siteName, string classNames, string path, int groupId = 0)
        {
            var dependencies = new List<string>();

            if (!String.IsNullOrEmpty(siteName))
            {
                // Get classes
                if (!String.IsNullOrEmpty(classNames))
                {
                    string[] cnames = classNames.Split(';');
                    foreach (string className in cnames)
                    {
                        dependencies.Add(GetAllNodesCacheKey(siteName, className.Trim()));
                    }
                }

                dependencies.AddRange(GetPathDependencyCacheKeys(siteName, path));
            }

            if (groupId > 0)
            {
                dependencies.Add(GetCommunityGroupCacheKey(groupId));
            }

            return dependencies;
        }


        private static string GetCommunityGroupCacheKey(int groupId)
        {
            return CacheHelper.GetCacheItemName(null, "community.group", "byid", groupId);
        }


        private static string GetAllNodesCacheKey(string siteName, string className)
        {
            return CacheHelper.GetCacheItemName(null, "nodes", siteName, className, "all");
        }


        private static string GetChildNodesCacheKey(string siteName, string path)
        {
            return CacheHelper.GetCacheItemName(null, "node", siteName, path, "childnodes");
        }
    }
}
