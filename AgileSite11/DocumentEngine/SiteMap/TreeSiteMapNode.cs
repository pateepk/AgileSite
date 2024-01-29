using System;
using System.Collections.Generic;
using System.Web;
using System.Data;

using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Site map node for the CMS tree sitemap structure.
    /// </summary>
    public class TreeSiteMapNode : SiteMapNode, IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// True if child nodes has been already loaded for this node.
        /// </summary>
        protected bool mChildNodesLoaded = false;

        /// <summary>
        /// Tree node.
        /// </summary>
        protected TreeNode mTreeNode = null;

        /// <summary>
        /// Node data.
        /// </summary>
        protected DataRow mNodeData = null;

        /// <summary>
        /// Tree provider object to use for the database access.
        /// </summary>
        protected TreeProvider mTreeProvider = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                if (NodeData != null)
                {
                    return new DataRowContainer(NodeData).ColumnNames;
                }

                if (TreeNode != null)
                {
                    return TreeNode.ColumnNames;
                }

                return null;
            }
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Flag saying whether the child nodes are already loaded to the node.
        /// </summary>
        public bool ChildNodesLoaded
        {
            get
            {
                return mChildNodesLoaded;
            }
            set
            {
                mChildNodesLoaded = value;
            }
        }


        /// <summary>
        /// Property to get and set the inner TreeNode.
        /// </summary>
        public TreeNode TreeNode
        {
            get
            {
                if ((mTreeNode == null) && (mNodeData != null))
                {
                    mTreeNode = TreeNode.New((string)mNodeData["ClassName"], mNodeData, TreeProvider);
                }
                return mTreeNode;
            }
            set
            {
                mTreeNode = value;
            }
        }


        /// <summary>
        /// Property to get and set the inner data.
        /// </summary>
        public DataRow NodeData
        {
            get
            {
                return mNodeData;
            }
            set
            {
                mNodeData = value;
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        public TreeSiteMapNode(TreeSiteMapProvider provider, string key)
            : base(provider, key)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        public TreeSiteMapNode(TreeSiteMapProvider provider, string key, string url)
            : base(provider, key, url)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        /// <param name="title">Node title</param>
        public TreeSiteMapNode(TreeSiteMapProvider provider, string key, string url, string title)
            : base(provider, key, url, title)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        /// <param name="title">Node title</param>
        /// <param name="description">Node description</param>
        public TreeSiteMapNode(TreeSiteMapProvider provider, string key, string url, string title, string description)
            : base(provider, key, url, title, description)
        {
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            if (NodeData != null)
            {
                return NodeData[columnName];
            }

            if (TreeNode != null)
            {
                return TreeNode.GetValue(columnName);
            }

            return null;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            if (NodeData != null)
            {
                return (value = NodeData[columnName]) != null;
            }

            if (TreeNode != null)
            {
                return TreeNode.TryGetValue(columnName, out value);
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName);
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}