using System;
using System.Web;
using System.Collections;
using System.Data;

using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Forum posts tree provider.
    /// </summary>
    public class ForumPostTreeProvider : StaticSiteMapProvider
    {
        #region "Variables and constants"

        /// <summary>
        /// All relative levels.
        /// </summary>
        public const int ALL_LEVELS = -1;

        private int mForumId = 0;
        private string mPath = String.Empty;
        private string mWhereCondition = String.Empty;
        private bool mSelectOnlyApproved = true;
        private bool mSortPostAscending = true;
        private int mMaxRelativeLevel = ALL_LEVELS;
        private string mOrderBy = String.Empty;
        private bool mBindNodeData = false;
        private string mColumns = String.Empty;
        private int mMaxPostNodes = 0;
        private int mRootNodeLevel = 0;
        private string mSelectPostPath = null;
        private Hashtable mNodesByID = null;
        private Hashtable mNodes = null;
        private SiteMapNode mRootNode = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// ForumID.
        /// </summary>
        public int ForumID
        {
            get
            {
                return mForumId;
            }
            set
            {
                mForumId = value;
            }
        }


        /// <summary>
        /// Path.
        /// </summary>
        public string Path
        {
            get
            {
                return ValidationHelper.GetString(mPath, String.Empty);
            }
            set
            {
                mPath = value;
            }
        }


        /// <summary>
        /// Gets or sets the path of selected post. If this path is defined all forum posts on selected
        /// path will be retrieved.
        /// </summary>
        public string SelectPostPath
        {
            get
            {
                return ValidationHelper.GetString(mSelectPostPath, String.Empty);
            }
            set
            {
                mSelectPostPath = value;
            }
        }


        /// <summary>
        /// WhereCondition.
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return ValidationHelper.GetString(mWhereCondition, String.Empty);
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// SelectOnlyApproved.
        /// </summary>
        public bool SelectOnlyApproved
        {
            get
            {
                return mSelectOnlyApproved;
            }
            set
            {
                mSelectOnlyApproved = value;
            }
        }


        /// <summary>
        /// SortPostAscending.
        /// </summary>
        public bool SortPostAscending
        {
            get
            {
                return mSortPostAscending;
            }
            set
            {
                mSortPostAscending = value;
            }
        }


        /// <summary>
        /// MaxRelativeLevel.
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
        /// Order by.
        /// </summary>
        public string OrderBy
        {
            get
            {
                return ValidationHelper.GetString(mOrderBy, String.Empty);
            }
            set
            {
                mOrderBy = value;
            }
        }


        /// <summary>
        /// Gets or sets the columns which should be selected.
        /// </summary>
        public string Columns
        {
            get
            {
                return mColumns;
            }
            set
            {
                mColumns = value;
            }
        }


        /// <summary>
        /// Maximum number of forum post nodes displayed within one level of the tree.
        /// </summary>
        public int MaxPostNodes
        {
            get
            {
                return mMaxPostNodes;
            }
            set
            {
                mMaxPostNodes = value;
            }
        }


        /// <summary>
        /// Root node level.
        /// </summary>
        public int RootNodeLevel
        {
            get
            {
                return mRootNodeLevel;
            }
        }


        /// <summary>
        /// Specifies if the node data (TreeNode) should be bound to the nodes.
        /// </summary>
        public bool BindNodeData
        {
            get
            {
                return mBindNodeData;
            }
            set
            {
                mBindNodeData = value;
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

        #endregion


        #region  "Methods"

        /// <summary>
        /// Returns the root sitemap node.
        /// </summary>
        protected override SiteMapNode GetRootNodeCore()
        {
            return mRootNode;
        }


        /// <summary>
        /// Performs the sitemap build.
        /// </summary>
        public override SiteMapNode BuildSiteMap()
        {
            //Lock the object
            if (mRootNode != null)
            {
                return mRootNode;
            }

            mNodes = new Hashtable();
            mNodesByID = new Hashtable();

            string order = "PostLevel ASC, PostTime ASC";

            if (!SortPostAscending)
            {
                order = "PostLevel ASC, PostTime DESC";
            }


            if (!String.IsNullOrEmpty(OrderBy))
            {
                order = order + ", " + OrderBy;
            }


            //Default root node
            ForumPostTreeNode root = new ForumPostTreeNode(this, "/", String.Empty, String.Empty);
            root.ChildNodesLoaded = (MaxRelativeLevel == ALL_LEVELS) || (MaxRelativeLevel > 0);

            int rootNodeId = 0;
            mNodesByID[rootNodeId] = root;

            AddNode(root);


            // Normalize root path
            string rootNodePath = Path;
            int rootNodeLevel = 0;

            rootNodePath = rootNodePath.TrimEnd('%');
            rootNodePath = rootNodePath.TrimEnd('/');

            if (rootNodePath == String.Empty)
            {
                rootNodePath = "/";
            }

            int postLevel = 0;
            bool useSelectedPath = !String.IsNullOrEmpty(SelectPostPath);
            DataSet ds = null;

            if (!useSelectedPath)
            {
                // Do not use max post nodes if all levels are retrieved
                int maxPostNodes = (MaxRelativeLevel == ALL_LEVELS) ? 0 : MaxPostNodes;

                //Get the tree nodes, max rel level decrease cause there is no root record(/) so postlevel 0 should seem as postlevel 1
                ds = ForumPostInfoProvider.SelectForumPosts(ForumID, Path, WhereCondition, order, MaxRelativeLevel - 1, SelectOnlyApproved, maxPostNodes, Columns);
            }
            else
            {
                string[] selectedPosts = SelectPostPath.Split('/');
                string fullPostIdPath = String.Empty;

                string fullPathWhereCondition = String.Empty;

                // Create path condition for each post level                
                foreach (string selectedPost in selectedPosts)
                {
                    fullPostIdPath += selectedPost + "/";
                    fullPathWhereCondition += "((PostIDPath LIKE '" + fullPostIdPath.Replace("'", "''") + "%') AND (PostLevel = " + postLevel + ")) OR ";
                    postLevel++;
                }

                // Remove OR at the end
                fullPathWhereCondition = TextHelper.TrimEndingWord(fullPathWhereCondition, "OR");

                // Combine with original where condition
                string where = SqlHelper.AddWhereCondition(WhereCondition, fullPathWhereCondition);

                // Get post on all levels of selected path
                ds = ForumPostInfoProvider.SelectForumPosts(ForumID, null, where, order, ALL_LEVELS, SelectOnlyApproved, 0, Columns);
            }

            //Build the sitemap
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // If not default (imaginary) root /, use first row as root
                if (rootNodePath != "/")
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    rootNodeId = ValidationHelper.GetInteger(dr["PostID"], 0);
                    rootNodeLevel = ValidationHelper.GetInteger(dr["PostLevel"], 0);

                    root = new ForumPostTreeNode(this, mPath, mPath, (string)dr["PostSubject"], "");
                    if (BindNodeData)
                    {
                        root.ItemData = dr;
                    }

                    mNodesByID[rootNodeId] = root;
                }

                foreach (DataTable dt in ds.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if ((string)dr["PostIDPath"] == rootNodePath)
                        {
                            //null, root is already present
                        }
                        else
                        {
                            string mPath = (string)dr["PostIDPath"];
                            // Add only when the specified path not already present
                            if (!mNodes.Contains(mPath))
                            {
                                int nodeId = ValidationHelper.GetInteger(dr["PostID"], 0);
                                int nodeLevel = ValidationHelper.GetInteger(dr["PostLevel"], 0);

                                ForumPostTreeNode newNode = new ForumPostTreeNode(this, mPath, mPath, (string)dr["PostSubject"], "");
                                if (BindNodeData)
                                {
                                    newNode.ItemData = dr;
                                }

                                if (useSelectedPath)
                                {
                                    // Is on path and in level
                                    newNode.ChildNodesLoaded = (MaxRelativeLevel == ALL_LEVELS) || (((nodeLevel + 1) < postLevel) && (SelectPostPath.StartsWithCSafe(mPath)));
                                }
                                else
                                {
                                    newNode.ChildNodesLoaded = (MaxRelativeLevel == ALL_LEVELS) || ((nodeLevel + 1) < (rootNodeLevel + MaxRelativeLevel));
                                }

                                mNodes[mPath] = newNode;
                                mNodesByID[nodeId] = newNode;

                                //Get the parent
                                int parentNodeId = ValidationHelper.GetInteger(dr["PostParentID"], 0);
                                ForumPostTreeNode parent = (ForumPostTreeNode)mNodesByID[parentNodeId];
                                if (parent == null)
                                {
                                    parent = root;
                                }

                                AddNode(newNode, parent);
                            }
                        }
                    }
                }
            }

            //Set the root
            mRootNodeLevel = rootNodeLevel;
            mRootNode = root;
            return mRootNode;
        }


        /// <summary>
        /// Returns the node specified by given Node ID.
        /// </summary>
        /// <param name="nodeId">Node ID to retrieve</param>
        public ForumPostTreeNode GetNodeById(int nodeId)
        {
            if (mRootNode == null)
            {
                BuildSiteMap();
            }

            return (ForumPostTreeNode)mNodesByID[nodeId];
        }


        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        public SiteMapNodeCollection GetChildNodes(int nodeId, bool canUseSiteMap)
        {
            SiteMapNodeCollection nodes = new SiteMapNodeCollection();

            // Try to get from current tree structure
            ForumPostTreeNode parentNode = null;
            if (canUseSiteMap)
            {
                parentNode = GetNodeById(nodeId);
                if ((parentNode != null) && (parentNode.ChildNodesLoaded))
                {
                    return parentNode.ChildNodes;
                }
            }

            // Prepare where condition
            string where = String.Empty;

            // If node is not root
            if (nodeId > 0)
            {
                where = "[PostParentID] = " + nodeId;
            }
            else if (nodeId == 0)
            {
                where = "[PostLevel] = 0";
            }

            where = SqlHelper.AddWhereCondition(where, WhereCondition);


            // Order by
            string order = "PostLevel ASC, PostTime ASC";

            if (!SortPostAscending)
            {
                order = "PostLevel ASC, PostTime DESC";
            }

            if (!String.IsNullOrEmpty(OrderBy))
            {
                order = order + ", " + OrderBy;
            }


            // Get the tree nodes
            DataSet ds = ForumPostInfoProvider.SelectForumPosts(ForumID, null, where, order, ALL_LEVELS, SelectOnlyApproved, MaxPostNodes, Columns);

            // Build the sitemap
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string mPath = (string)dr["PostIDPath"];

                    ForumPostTreeNode newNode = new ForumPostTreeNode(this, mPath, mPath, (string)dr["PostSubject"], "");
                    if (BindNodeData)
                    {
                        newNode.ItemData = dr;
                    }

                    // Add to children
                    nodes.Add(newNode);
                }
            }

            // Update the node
            if (parentNode != null)
            {
                parentNode.ChildNodes = nodes;
                parentNode.ChildNodesLoaded = true;
            }

            return nodes;
        }

        #endregion
    }
}