using System;
using System.Collections;
using System.Data;
using System.Web;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Universal sitemap tree provider.
    /// </summary>
    public class UniTreeProvider : StaticSiteMapProvider
    {
        #region "Variables"

        // UniTreeNode collection
        private Hashtable nodesByKey;

        // Root node variable
        private UniTreeNode mRootNode;
        private bool mUseCustomRoots;

        // Object vaiables
        private string mObjectType;
        private string mQueryName;
        private QueryDataParameters mParameters;
        private string mObjectTypeColumn = string.Empty;

        // Query variables
        private string mWhereCondition;
        private string mColumns;
        private string mOrderBy;
        private int mTopN;

        // Tree - basic variables
        private string mDisplayNameColumn;
        private string mValueColumn;

        // Tree variables
        private int mMaxRelativeLevel = -1;
        private int mRootLevelOffset;
        private string mIDColumn;
        private string mParentIDColumn;
        private string mLevelColumn;
        private string mPathColumn;
        private string mOrderColumn;
        private string mChildCountColumn;
        private string mParameterColumn;
        private bool mBindNodeData = true;
        private string mCaptionColumn;


        /// <summary>
        /// Constructor
        /// </summary>
        public UniTreeProvider()
        {
            IconClassColumn = null;
            ImageColumn = null;
        }

        #endregion


        #region "Object properties"

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return mObjectType;
            }
            set
            {
                mObjectType = value;
            }
        }


        /// <summary>
        /// Gets or sets the query name. if query name is specified object type is not used to data loading.
        /// </summary>
        public string QueryName
        {
            get
            {
                return mQueryName;
            }
            set
            {
                mQueryName = value;
            }
        }


        /// <summary>
        /// Gets or sets query parametres.
        /// </summary>
        public QueryDataParameters Parameters
        {
            get
            {
                return mParameters;
            }
            set
            {
                mParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets the column name of the object type.
        /// </summary>
        public string ObjectTypeColumn
        {
            get
            {
                return mObjectTypeColumn;
            }
            set
            {
                mObjectTypeColumn = value;
            }
        }

        #endregion


        #region "Query properties"

        /// <summary>
        /// Gets or sets the query where condition.
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return mWhereCondition;
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// Gets or sets the query order by expression.
        /// </summary>
        public string OrderBy
        {
            get
            {
                return mOrderBy;
            }
            set
            {
                mOrderBy = value;
            }
        }


        /// <summary>
        /// Gets or sets the list of columns which should be selected.
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
        /// Gets or sets the number of rows which should be selected.
        /// </summary>
        public int TopN
        {
            get
            {
                return mTopN;
            }
            set
            {
                mTopN = value;
            }
        }

        #endregion


        #region "Tree - basic properties"

        /// <summary>
        /// Gets or sets the name of column which contains display name value.
        /// </summary>
        public string DisplayNameColumn
        {
            get
            {
                return mDisplayNameColumn;
            }
            set
            {
                mDisplayNameColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains value.
        /// </summary>
        public string ValueColumn
        {
            get
            {
                return mValueColumn;
            }
            set
            {
                mValueColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains image URL value.
        /// </summary>
        public string ImageColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of column which contains font icon CSS class.
        /// </summary>
        public string IconClassColumn
        {
            get;
            set;
        }

        #endregion


        #region "Tree properties"

        /// <summary>
        /// Gets or sets the maximal nested level which should be loaded.
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
        /// Gets or sets the name of column which contains object id value.
        /// </summary>
        public string IDColumn
        {
            get
            {
                return mIDColumn;
            }
            set
            {
                mIDColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains parent id value for current object.
        /// </summary>
        public string ParentIDColumn
        {
            get
            {
                return mParentIDColumn;
            }
            set
            {
                mParentIDColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains current object level value.
        /// </summary>
        public string LevelColumn
        {
            get
            {
                return mLevelColumn;
            }
            set
            {
                mLevelColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains caption of current object.
        /// </summary>
        public string CaptionColumn
        {
            get
            {
                return mCaptionColumn;
            }
            set
            {
                mCaptionColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains current object path.
        /// </summary>
        public string PathColumn
        {
            get
            {
                return mPathColumn;
            }
            set
            {
                mPathColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains current object order.
        /// </summary>
        public string OrderColumn
        {
            get
            {
                return mOrderColumn;
            }
            set
            {
                mOrderColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains current object child count.
        /// </summary>
        public string ChildCountColumn
        {
            get
            {
                return mChildCountColumn;
            }
            set
            {
                mChildCountColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of column which contains additional parameter.
        /// </summary>
        public string ParameterColumn
        {
            get
            {
                return mParameterColumn;
            }
            set
            {
                mParameterColumn = value;
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


        /// <summary>
        /// Specifies the root level offset.
        /// </summary>
        public int RootLevelOffset
        {
            get
            {
                return mRootLevelOffset;
            }
            set
            {
                mRootLevelOffset = value;
            }
        }


        /// <summary>
        /// Indicates whether using custom roots. When set to true, roots will not be obtained from DB. Default value is false.
        /// </summary>
        public bool UseCustomRoots
        {
            get
            {
                return mUseCustomRoots;
            }
            set
            {
                mUseCustomRoots = value;
            }
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Returns the root UniTreeNode.
        /// </summary>
        protected override SiteMapNode GetRootNodeCore()
        {
            return mRootNode;
        }


        /// <summary>
        /// Performs the site map build and returns UniTreeNode.
        /// </summary>
        public override SiteMapNode BuildSiteMap()
        {
            if (nodesByKey == null)
            {
                nodesByKey = new Hashtable();
                if (!UseCustomRoots)
                {
                    DataSet ds = LoadData(LevelColumn + " = " + RootLevelOffset);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Create root node
                        UniTreeNode rootNode = new UniTreeNode(this, "/");
                        rootNode.Title = Convert.ToString(ds.Tables[0].Rows[0][DisplayNameColumn]);
                        if (BindNodeData)
                        {
                            rootNode.ItemData = ds.Tables[0].Rows[0];
                        }


                        // Create key
                        string objectID = ds.Tables[0].Rows[0][IDColumn].ToString();
                        string key = objectID;

                        if (!string.IsNullOrEmpty(ObjectTypeColumn))
                        {
                            key += "_" + ds.Tables[0].Rows[0][ObjectTypeColumn];
                        }

                        // Add to the root collection
                        nodesByKey[key] = rootNode;

                        // Loop thru all items
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            if (ValidationHelper.GetString(dr[PathColumn], String.Empty) == "/")
                            {
                                // Root is already added to the final collection
                            }
                            else
                            {
                                // Create new uni tree node
                                UniTreeNode node = new UniTreeNode(this, Convert.ToString(dr[PathColumn]));
                                node.Title = Convert.ToString(ds.Tables[0].Rows[0][DisplayNameColumn]);
                                if (BindNodeData)
                                {
                                    node.ItemData = dr;
                                }

                                // Create key
                                objectID = ds.Tables[0].Rows[0][ParentIDColumn].ToString();
                                key = objectID;

                                if (!string.IsNullOrEmpty(ObjectTypeColumn))
                                {
                                    key += "_" + ds.Tables[0].Rows[0][ObjectTypeColumn];
                                }

                                // Find parent and add current node as sub-child
                                UniTreeNode parent = GetNodeByKey(key);
                                if ((parent != null) && (parent != node))
                                {
                                    AddNode(node, parent);
                                }
                            }
                        }

                        // Set root node
                        mRootNode = rootNode;
                        return rootNode;
                    }
                }
            }

            return null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads data with dependence on current settings
        /// <param name="internalWhere">Where condition</param>
        /// </summary>
        public DataSet LoadData(string internalWhere)
        {
            #region "Order by"

            // If order by is not specified use default order
            // Order by priority 1 - Level, 2 - Order, 3 - Display name
            string orderBy = OrderBy;

            if (String.IsNullOrEmpty(orderBy))
            {
                // Order by level column if is specified column name
                if (!String.IsNullOrEmpty(LevelColumn))
                {
                    orderBy += LevelColumn + ",";
                }

                // Order by order column if is specified column name
                if (!String.IsNullOrEmpty(OrderColumn))
                {
                    orderBy += OrderColumn + ",";
                }

                // Order by display name column if is specified column name
                if (!String.IsNullOrEmpty(DisplayNameColumn))
                {
                    orderBy += DisplayNameColumn;
                }

                // Remove trailing comma
                if (!string.IsNullOrEmpty(orderBy))
                {
                    orderBy = orderBy.TrimEnd(',');
                }
            }

            #endregion


            #region "Where condition"

            string where = WhereCondition;

            // Combine existing where condition with max relative level condition
            if (!String.IsNullOrEmpty(where) && (MaxRelativeLevel > 0) && (!String.IsNullOrEmpty(LevelColumn)))
            {
                where = "(" + where + ") AND ";
            }

            // Add level condition if it is required
            if ((MaxRelativeLevel > 0) && (!String.IsNullOrEmpty(LevelColumn)))
            {
                where += LevelColumn + " <= " + (MaxRelativeLevel + RootLevelOffset);
            }

            // Internal where condition
            if (!string.IsNullOrEmpty(internalWhere))
            {
                where = SqlHelper.AddWhereCondition(where, internalWhere);
            }

            #endregion


            #region "Data loading"

            // Data loading - load by object name
            if (!String.IsNullOrEmpty(QueryName))
            {
                return ConnectionHelper.ExecuteQuery(QueryName, Parameters, where, orderBy, TopN, Columns);
            }
            else if (!String.IsNullOrEmpty(ObjectType))
            {
                return ConnectionHelper.ExecuteQuery(ObjectType + ".selectall", Parameters, where, orderBy, TopN, Columns);
            }
            else
            {
                throw new Exception("[UniTreeProvider]: Object type or query name must be specified");
            }

            #endregion
        }


        /// <summary>
        /// Returns the UniTreeNode specified by given Node ID.
        /// </summary>
        /// <param name="key">Key of UniTreeNode object, ID or combination object type and ID</param>
        public UniTreeNode GetNodeByKey(string key)
        {
            return (UniTreeNode)nodesByKey[key];
        }


        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        /// <param name="parentNodeValue">Parent node value</param>
        public SiteMapNodeCollection GetChildNodes(string parentNodeValue)
        {
            return GetChildNodes(parentNodeValue, -1);
        }


        /// <summary>
        /// Returns the set of child nodes for the specified node.
        /// </summary>
        /// <param name="parentNodeValue">Parent node value</param>
        /// <param name="nodeLevel">Specified level</param>
        public SiteMapNodeCollection GetChildNodes(string parentNodeValue, int nodeLevel)
        {
            SiteMapNodeCollection nodes = new SiteMapNodeCollection();

            BuildSiteMap();
            // Try to get from current tree structure
            UniTreeNode parentNode = GetNodeByKey(parentNodeValue);

            if ((parentNode != null) && (parentNode.ChildNodesLoaded))
            {
                return parentNode.ChildNodes;
            }

            // Prepare where condition
            string val = parentNodeValue.Split('_')[0];
            string where;
            if (val == "NULL")
            {
                where = ParentIDColumn + " IS NULL";
            }
            else
            {
                // Get parent node ID
                where = string.Format("{0} = {1}", ParentIDColumn, ValidationHelper.GetInteger(val, 0));
            }

            if (nodeLevel >= 0)
            {
                where = SqlHelper.AddWhereCondition(LevelColumn + " = " + (nodeLevel + RootLevelOffset), where);
            }

            // Get the child nodes
            DataSet ds = LoadData(where);

            // Process the nodes            
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataTable dt in ds.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        // Create new uni tree node
                        UniTreeNode node = new UniTreeNode(this, Convert.ToString(dr[PathColumn]));
                        node.Title = Convert.ToString(ds.Tables[0].Rows[0][DisplayNameColumn]);
                        if (BindNodeData)
                        {
                            node.ItemData = dr;
                        }

                        nodes.Add(node);
                    }
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