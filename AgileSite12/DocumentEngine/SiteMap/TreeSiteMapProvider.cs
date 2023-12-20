using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing tree site map management.
    /// </summary>
    public class TreeSiteMapProvider : StaticSiteMapProvider
    {
        #region "Private items"

        private TreeProvider mTreeProvider;
        private string mPath = "/";
        private bool mSelectOnlyPublished = true;
        private bool? mSelectPublishedData;
        private int mMaxRelativeLevel = -1;
        private string mCultureCode;

        /// <summary>
        /// A table of all the nodes, indexed by NodeAliasPath.
        /// </summary>
        private readonly Dictionary<string, TreeSiteMapNode> mNodesByPath = new Dictionary<string, TreeSiteMapNode>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// A table of all the nodes, indexed by NodeID.
        /// </summary>
        private readonly Dictionary<int, TreeSiteMapNode> mNodesById = new Dictionary<int, TreeSiteMapNode>();

        /// <summary>
        /// Sitemap root node.
        /// </summary>
        private SiteMapNode mRootNode;

        #endregion


        #region "SiteMapProvider properties"

        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public virtual TreeProvider TreeProvider
        {
            get
            {
                if (mTreeProvider == null)
                {
                    mTreeProvider = new TreeProvider(MembershipContext.AuthenticatedUser);
                }

                return mTreeProvider;
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Property to get Sitemap root node.
        /// </summary>
        public override SiteMapNode RootNode
        {
            get
            {
                if (mRootNode == null)
                {
                    BuildSiteMap();
                }
                return mRootNode;
            }
        }


        /// <summary>
        /// Property to set and get the classnames list (separated by the semicolon).
        /// </summary>
        public string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the permissions should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        public string Path
        {
            get
            {
                return mPath;
            }
            set
            {
                mPath = value;
            }
        }


        /// <summary>
        /// Property to set and get the SiteName.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the CultureCode.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return mCultureCode ?? (mCultureCode = CultureHelper.GetPreferredCulture());
            }
            set
            {
                mCultureCode = value;
            }
        }


        /// <summary>
        /// Property to set and get the CombineWithDefaultCulture flag.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the WhereCondition.
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the OrderBy.
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Property to set and get the SelectOnlyPublished flag.
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return mSelectOnlyPublished;
            }
            set
            {
                mSelectOnlyPublished = value;
            }
        }


        /// <summary>
        /// If true, the published data.
        /// </summary>
        public bool SelectPublishedData
        {
            get
            {
                if (mSelectPublishedData == null)
                {
                    mSelectPublishedData = PortalContext.ViewMode.IsLiveSite();
                }
                return mSelectPublishedData.Value;
            }
            set
            {
                mSelectPublishedData = value;
            }
        }


        /// <summary>
        /// Property to set and get the MaxRelativeLevel.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel;
            }
            set
            {
                mMaxRelativeLevel = value;
            }
        }


        /// <summary>
        /// Specifies if the node data (TreeNode) should be bound to the nodes.
        /// </summary>
        public bool BindNodeData
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of tree nodes displayed within the tree.
        /// </summary>
        public int MaxTreeNodes
        {
            get;
            set;
        }


        /// <summary>
        /// Root node level.
        /// </summary>
        public int RootNodeLevel
        {
            get;
            private set;
        }

        #endregion


        #region "SiteMapProvider methods"

        /// <summary>
        /// Returns the the root sitemap node.
        /// </summary>
        protected override SiteMapNode GetRootNodeCore()
        {
            return RootNode;
        }


        /// <summary>
        /// Clean up any collections or other state that an instance of this may hold.
        /// </summary>
        protected override void Clear()
        {
            mRootNode = null;
            mNodesByPath.Clear();
            mNodesById.Clear();
            base.Clear();
        }


        /// <summary>
        /// Reloads the tree data.
        /// </summary>
        public void ReloadData()
        {
            Clear();
        }


        /// <summary>
        /// Performs the sitemap build.
        /// </summary>
        public override SiteMapNode BuildSiteMap()
        {
            // Lock the object
            if (mRootNode != null)
            {
                return mRootNode;
            }

            // Get the site
            var site = SiteInfoProvider.GetSiteInfo(SiteName);
            if (site == null)
            {
                throw new InvalidOperationException("Site with '" + SiteName + "' name not found.");
            }

            // Get the root node
            var rootData = GetDocumentsQuery()
                .Path(Path)
                .OrderBy("NodeAliasPath")
                .Published(false)
                .Result;

            // Make sure DataSet is not locked by cache
            rootData = rootData.AsModifyable();

            // Create dummy node when there is not result after permission check
            if (CheckPermissions && DataHelper.DataSourceIsEmpty(rootData))
            {
                var ci = DataClassInfoProvider.GetDataClassInfo(SystemDocumentTypes.Root);
                if (ci == null)
                {
                    throw new InvalidOperationException("Node class not found.");
                }

                rootData = DocumentHelper.GetTreeNodeDataSet(ci.ClassName, false, false);
                DataHelper.EnsureColumn(rootData.Tables[0], "ClassName", typeof(string));
                DataRow row = rootData.Tables[0].NewRow();
                row["NodeID"] = -1;
                row["DocumentID"] = -1;
                row["DocumentCulture"] = CultureCode;
                row["ClassName"] = ci.ClassName;
                row["NodeClassID"] = ci.ClassID;
                row["NodeLevel"] = 0;
                rootData.Tables[0].Rows.Add(row);
                rootData.AcceptChanges();
            }

            if (DataHelper.DataSourceIsEmpty(rootData))
            {
                throw new InvalidOperationException("Root node '" + Path + "' for the '" + SiteName + "' site not found.");
            }

            // Default root node
            DataRow rootNodeDr = rootData.Tables[0].Rows[0];
            string rootAliasPath = ValidationHelper.GetString(rootNodeDr["NodeAliasPath"], "/");
            RootNodeLevel = ValidationHelper.GetInteger(rootNodeDr["NodeLevel"], 0);
            TreeSiteMapNode root = new TreeSiteMapNode(this, rootAliasPath, "", "Web site");
            if (BindNodeData)
            {
                string rootClassName = ValidationHelper.GetString(rootNodeDr["ClassName"], "");
                if (rootClassName.Equals(SystemDocumentTypes.Root, StringComparison.InvariantCultureIgnoreCase))
                {
                    rootNodeDr["NodeName"] = ResHelper.LocalizeString(site.DisplayName);
                    rootNodeDr["DocumentName"] = ResHelper.LocalizeString(site.DisplayName);
                }
                root.TreeProvider = TreeProvider;
                root.NodeData = rootNodeDr;
            }
            root.ChildNodesLoaded = (MaxRelativeLevel > 0);

            AddNode(root);
            mNodesByPath[rootAliasPath] = root;
            int rootNodeId = ValidationHelper.GetInteger(rootNodeDr["NodeID"], 0);
            mNodesById[rootNodeId] = root;

            int rootNodeLevel = ValidationHelper.GetInteger(rootNodeDr["NodeLevel"], 0);

            var query = GetDocumentsQuery()
                .Path(Path, PathTypeEnum.Children)
                .Where(WhereCondition)
                .OrderByAscending("NodeLevel")
                .OrderBy(OrderBy)
                .NestingLevel(MaxRelativeLevel)
                .Published(SelectOnlyPublished);

            if (ClassNames != null)
            {
                query.Types(ClassNames.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            }

            query.ForEachRow(row =>
            {
                string aliasPath = (string)row["NodeAliasPath"];
                if (aliasPath == rootAliasPath)
                {
                    // Root is already present
                    return;
                }

                // Add only when the specified path not already present
                if (mNodesByPath.ContainsKey(aliasPath))
                {
                    return;
                }

                int nodeId = ValidationHelper.GetInteger(row["NodeID"], 0);
                int nodeLevel = ValidationHelper.GetInteger(row["NodeLevel"], 0);
                TreeSiteMapNode newNode = new TreeSiteMapNode(this, aliasPath, DocumentURLProvider.GetUrl(aliasPath), (string)row["NodeName"], "");
                if (BindNodeData)
                {
                    newNode.NodeData = row;
                    newNode.TreeProvider = TreeProvider;
                }
                newNode.ChildNodesLoaded = nodeLevel < rootNodeLevel + MaxRelativeLevel;
                mNodesByPath[aliasPath] = newNode;
                mNodesById[nodeId] = newNode;

                // Get the parent
                int parentNodeId = ValidationHelper.GetInteger(row["NodeParentID"], 0);
                var parent = mNodesById.ContainsKey(parentNodeId) ? mNodesById[parentNodeId] : root;
                AddNode(newNode, parent);
            });

            // Set the root
            mRootNode = root;
            return mRootNode;
        }


        private MultiDocumentQuery GetDocumentsQuery()
        {
            var columns = GetColumns();

            // Get documents
            var query = DocumentHelper.GetDocuments()
                                      .OnSite(SiteName)
                                      .Columns(columns)
                                      .LatestVersion(!SelectPublishedData)
                                      .CheckPermissions(CheckPermissions);

            TreeProvider.SetQueryCultures(query, CultureCode, CombineWithDefaultCulture);

            // Do not apply published from / to columns to make sure the published information is correctly evaluated
            query.Properties.ExcludedVersionedColumns = new[] { "DocumentPublishFrom", "DocumentPublishTo" };

            // Reflect tree provider preferred culture to serve the documents in correct one
            query.Properties.PreferredCultureCode = TreeProvider.PreferredCultureCode;

            return query;
        }


        /// <summary>
        /// Returns the node specified by given Node ID.
        /// </summary>
        /// <param name="nodeId">Node ID to retrieve</param>
        public TreeSiteMapNode GetNodeById(int nodeId)
        {
            BuildSiteMap();
            return mNodesById.ContainsKey(nodeId) ? mNodesById[nodeId] : null;
        }


        /// <summary>
        /// Returns the node by its Alias.
        /// </summary>
        public TreeSiteMapNode GetNodeByAliasPath(string aliasPath)
        {
            BuildSiteMap();

            if ((aliasPath == "") || (aliasPath == "/"))
            {
                return (TreeSiteMapNode)RootNode;
            }

            if (mNodesByPath.ContainsKey(aliasPath))
            {
                return mNodesByPath[aliasPath];
            }

            // Get the parent node
            var parentNode = GetNodeByAliasPath(TreePathUtils.GetParentPath(aliasPath));
            if (parentNode == null)
            {
                return null;
            }

            if (!parentNode.ChildNodesLoaded)
            {
                // Process the children
                int parentNodeId = (int)parentNode.NodeData["NodeID"];
                var childNodes = GetChildNodes(parentNodeId);
                parentNode.ChildNodes = childNodes;
                parentNode.ChildNodesLoaded = true;

                foreach (TreeSiteMapNode childNode in childNodes)
                {
                    childNode.ParentNode = parentNode;

                    var childPath = ValidationHelper.GetString(childNode.NodeData["NodeAliasPath"], "");
                    mNodesByPath[childPath] = childNode;

                    int childNodeId = (int)childNode.NodeData["NodeID"];
                    mNodesById[childNodeId] = childNode;
                }

                if (mNodesByPath.ContainsKey(aliasPath))
                {
                    return mNodesByPath[aliasPath];
                }
            }

            var query = GetDocumentsQuery()
                .Path(aliasPath)
                .Published(SelectOnlyPublished);

            if (ClassNames != null)
            {
                query.Types(ClassNames.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            }

            var data = query.Result;
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            // Create the node
            var row = data.Tables[0].Rows[0];
            var node = new TreeSiteMapNode(this, aliasPath, DocumentURLProvider.GetUrl(aliasPath), (string)row["NodeName"], "");
            if (BindNodeData)
            {
                node.NodeData = row;
                node.TreeProvider = TreeProvider;
            }
            node.ParentNode = parentNode;

            // Add to nodes collections
            mNodesByPath[aliasPath] = node;
            int nodeId = (int)node.NodeData["NodeID"];
            mNodesById[nodeId] = node;

            return node;
        }


        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        public SiteMapNodeCollection GetChildNodes(int nodeId)
        {
            return GetChildNodes(nodeId, -1);
        }


        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        public SiteMapNodeCollection GetChildNodes(int nodeId, int nodeLevel)
        {
            var nodes = new SiteMapNodeCollection();

            // Try to get from current tree structure
            var parentNode = GetNodeById(nodeId);
            if ((parentNode != null) && parentNode.ChildNodesLoaded)
            {
                return parentNode.ChildNodes;
            }

            var query = GetDocumentsQuery()
                .Where(WhereCondition)
                .Where(GetChildrenWhereCondition(nodeId))
                .OrderBy(OrderBy)
                .Published(SelectOnlyPublished)
                .TopN(MaxTreeNodes)
                .NestingLevel(GetNestingLevel(nodeLevel, parentNode));

            if (ClassNames != null)
            {
                query.Types(ClassNames.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            }

            query.ForEachRow(row =>
            {
                string aliasPath = (string)row["NodeAliasPath"];
                TreeSiteMapNode newNode = new TreeSiteMapNode(this, aliasPath, DocumentURLProvider.GetUrl(aliasPath), (string)row["NodeName"], "");
                if (BindNodeData)
                {
                    newNode.NodeData = row;
                    newNode.TreeProvider = TreeProvider;
                }

                // Add to nodes collections
                mNodesByPath[aliasPath] = newNode;
                int childNodeId = (int)newNode.NodeData["NodeID"];
                mNodesById[childNodeId] = newNode;
                nodes.Add(newNode);
            });

            // Update the node
            if (parentNode != null)
            {
                parentNode.ChildNodes = nodes;
                parentNode.ChildNodesLoaded = true;
            }

            return nodes;
        }


        private static WhereCondition GetChildrenWhereCondition(int nodeId)
        {
            return new WhereCondition()
                .WhereEquals("NodeParentID", nodeId)
                .And()
                .WhereNotEquals("NodeParentID", "NodeID".AsExpression());
        }


        private static int GetNestingLevel(int nodeLevel, TreeSiteMapNode parentNode)
        {
            if (nodeLevel >= 0)
            {
                return nodeLevel;
            }

            if ((nodeLevel == -1) && (parentNode != null) && (parentNode.TreeNode != null))
            {
                return parentNode.TreeNode.NodeLevel + 1;
            }

            return -1;
        }


        /// <summary>
        /// Gets columns to be selected.
        /// </summary>
        private string GetColumns()
        {
            // Set columns
            string columns = DocumentColumnLists.SELECTTREE_REQUIRED_COLUMNS + ", DocumentIsWaitingForTranslation, DocumentMenuRedirectToFirstChild";
            if (CheckPermissions)
            {
                columns = SqlHelper.MergeColumns(columns, DocumentColumnLists.SECURITYCHECK_REQUIRED_COLUMNS);
            }

            return columns;
        }

        #endregion
    }
}