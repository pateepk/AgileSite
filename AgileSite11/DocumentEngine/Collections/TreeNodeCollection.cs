using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;

namespace CMS.DocumentEngine
{
    #region "Generic collection"

    /// <summary>
    /// Generic strongly typed info object collection
    /// </summary>
    public class TreeNodeCollection<NodeType> : TreeNodeCollection, IEnumerable<NodeType> where NodeType : TreeNode, new()
    {
        #region "Methods"

        /// <summary>
        /// Constructor. Creates a static collection populated from DataSet
        /// </summary>
        /// <param name="className">Default class name for the documents (if ClassName column is not available)</param>
        /// <param name="sourceData">Source DataSet</param>
        public TreeNodeCollection(string className, DataSet sourceData)
            : base(className, sourceData)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="className">Default class name for the documents (if ClassName column is not available)</param>
        public TreeNodeCollection(string className)
            : base(className)
        {
        }

        #endregion


        #region "IEnumerable<NodeType> Members"

        /// <summary>
        /// Gets the strongly typed enumerator for the collection
        /// </summary>
        public new IEnumerator<NodeType> GetEnumerator()
        {
            // Encapsulate the enumerator and cast the results
            var baseEnum = GetEnumeratorInternal();

            while (baseEnum.MoveNext())
            {
                yield return (NodeType)baseEnum.Current;
            }
        }

        #endregion
    }

    #endregion


    /// <summary>
    /// Document collection.
    /// </summary>
    public class TreeNodeCollection : InfoObjectCollection<TreeNode>
    {
        #region "Variables"

        /// <summary>
        /// Tree provider object to use for the database access.
        /// </summary>
        protected TreeProvider mTreeProvider;


        /// <summary>
        /// Site name of the documents.
        /// </summary>
        public string SiteName = TreeProvider.ALL_SITES;

        /// <summary>
        /// Alias path.
        /// </summary>
        public string AliasPath = TreeProvider.ALL_DOCUMENTS;

        /// <summary>
        /// Culture of the documents.
        /// </summary>
        public string CultureCode = TreeProvider.ALL_CULTURES;

        /// <summary>
        /// Combine with default culture.
        /// </summary>
        public bool CombineWithDefaultCulture = true;

        /// <summary>
        /// Class names to select.
        /// </summary>
        public string ClassNames;

        /// <summary>
        /// Maximum relative level.
        /// </summary>
        public int MaxRelativeLevel = TreeProvider.ALL_LEVELS;

        /// <summary>
        /// Select only published documents.
        /// </summary>
        public bool SelectOnlyPublished = true;

        /// <summary>
        /// If true, the coupled data are retrieved in case class names are specified
        /// </summary>
        public bool SelectAllData;

        /// <summary>
        /// Collection including coupled data
        /// </summary>
        protected TreeNodeCollection mWithAllData;

        /// <summary>
        /// Collection with filtered documents based on permission check in context of current user
        /// </summary>
        protected TreeNodeCollection mWithPermissionsCheck;
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Name column name
        /// </summary>
        public override string NameColumn
        {
            get
            {
                if (base.NameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    base.NameColumn = "NodeAlias";
                }

                return base.NameColumn;
            }
            set
            {
                base.NameColumn = value;
            }
        }


        /// <summary>
        /// Default class name for the documents (if ClassName column is not available)
        /// </summary>
        public string DefaultClassName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns true if the collection is disconnected from the data source
        /// </summary>
        public override bool IsDisconnected
        {
            get
            {
                if ((ParentDocument != null) && ParentDocument.Generalized.IsDisconnected)
                {
                    return true;
                }

                return base.IsDisconnected;
            }
        }


        /// <summary>
        /// Parent document.
        /// </summary>
        public TreeNode ParentDocument
        {
            get;
            protected set;
        }


        /// <summary>
        /// User to be used for permissions check of the collection if required. If none provided, permissions are not checked.
        /// </summary>
        internal UserInfo User
        {
            get;
            set;
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
        /// Collection including coupled data
        /// </summary>
        public TreeNodeCollection WithAllData
        {
            get
            {
                if (mWithAllData == null)
                {
                    // Do the copy of the collection with coupled data enabled
                    var clone = (TreeNodeCollection)Clone();

                    clone.SelectAllData = true;

                    mWithAllData = clone;
                }

                return mWithAllData;
            }
        }


        /// <summary>
        /// Collection with filtered documents based on permission check in context of current user
        /// </summary>
        public TreeNodeCollection WithPermissionsCheck
        {
            get
            {
                if (mWithPermissionsCheck == null)
                {
                    // Do the copy of the collection
                    var clone = (TreeNodeCollection)Clone();

                    // Set user context
                    clone.User = TreeProvider.UserInfo;

                    mWithPermissionsCheck = clone;
                }

                return mWithPermissionsCheck;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor. Creates a static collection populated from DataSet
        /// </summary>
        /// <param name="className">Default class name for the documents (if ClassName column is not available)</param>
        /// <param name="sourceData">Source DataSet</param>
        public TreeNodeCollection(string className, DataSet sourceData = null)
        {
            if (sourceData != null)
            {
                UseData(sourceData);
            }
            
            DefaultClassName = className;
            AllowPaging = false;
        }
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="className">Default class name for the documents (if ClassName column is not available)</param>
        /// <param name="parentDocument">Parent document node</param>
        /// <param name="parentStorage">Parent storage</param>
        public TreeNodeCollection(string className, TreeNode parentDocument, ICMSStorage parentStorage)
            : this(className)
        {
            ParentStorage = parentStorage;
            ParentDocument = parentDocument;
        }


        /// <summary>
        /// Creates new instance of the encapsulated object.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        public override TreeNode CreateNewObject(DataRow dr)
        {
            string className = DataHelper.GetNotEmpty(DataHelper.GetDataRowValue(dr, "ClassName"), DefaultClassName);

            var node = TreeNode.New(className, dr, TreeProvider);
            node.IsLastVersion = IsLastVersion;
            node.HideTypeSpecificColumns = !SelectAllData;
            
            return node;
        }


        /// <summary>
        /// Gets the unique object name from the given object.
        /// </summary>
        /// <param name="obj">Object</param>
        public override string GetObjectName(TreeNode obj)
        {
            return ValidationHelper.GetString(obj.GetValue(NameColumn), String.Empty);
        }


        /// <summary>
        /// Gets the object by its name, internal representation that gets data from database
        /// </summary>
        /// <param name="name">Name of the object</param>
        protected override TreeNode GetObjectByNameInternal(string name)
        {
            TreeNode result;
            var originalSelectAllData = SelectAllData;

            try
            {
                // Temporarily allow select all data for single document selection
                SelectAllData = true;

                result = base.GetObjectByNameInternal(name);
            }
            finally
            {
                // Restore the flags
                SelectAllData = originalSelectAllData;
            }

            return result;
        }


        /// <summary>
        /// Gets the data for the collection.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="offset">Starting offset for the data.</param>
        /// <param name="maxRecords">Maximum number of records to get.</param>
        /// <param name="totalRecords">Returning total number of records.</param>
        protected override DataSet GetData(IWhereCondition where, int offset, int maxRecords, ref int totalRecords)
        {
            // No results if disconnected
            if (IsDisconnected)
            {
                return null;
            }

            var query = BuildQuery(where);

            DataSet result;
            if (User != null)
            {
                using (new CMSActionContext { User = User })
                {
                    result = query.Result;
                }
            }
            else
            {
                result = query.Result;
            }

            return result;
        }


        private MultiDocumentQuery BuildQuery(IWhereCondition where)
        {
            var checkPermissions = (User != null) && ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCheckPermissionsForDocumentCollection"], true);

            // Ensure ClassName column if columns are explicitly specified
            var columns = Columns;
            if (!string.IsNullOrEmpty(columns) && SelectAllData)
            {
                columns = SqlHelper.MergeColumns(columns, "ClassName");
            }

            var query =
                DocumentHelper.GetDocuments()
                              .LatestVersion(IsLastVersion)
                              .Path(AliasPath)   
                              .Where(new WhereCondition(Where).And(where))
                              .OrderBy(OrderByColumns)
                              .NestingLevel(MaxRelativeLevel)
                              .Published(SelectOnlyPublished)
                              .TopN(TopN)
                              .Columns(columns)
                              .WithCoupledColumns(SelectAllData)
                              .CheckPermissions(checkPermissions);

            if (!String.IsNullOrEmpty(SiteName) && (SiteName != TreeProvider.ALL_SITES))
            {
                query.OnSite(SiteName);
            }

            if (!String.IsNullOrEmpty(ClassNames))
            {
                query.Types(ClassNames.Trim(';').Split(';'));
            }

            TreeProvider.SetQueryCultures(query, CultureCode, CombineWithDefaultCulture);

            return query;
        }
        

        /// <summary>
        /// Copies the properties of this collection to the other collection
        /// </summary>
        /// <param name="col">Target collection</param>
        protected override void CopyPropertiesTo(IInfoObjectCollection col)
        {
            base.CopyPropertiesTo(col);

            var treeNodeCollection = col as TreeNodeCollection;
            if (treeNodeCollection != null)
            {
                CopyTreeNodeCollectionPropertiesTo(treeNodeCollection);
            }
        }


        private void CopyTreeNodeCollectionPropertiesTo(TreeNodeCollection col)
        {
            col.mTreeProvider = mTreeProvider;
            col.SiteName = SiteName;
            col.AliasPath = AliasPath;
            col.CultureCode = CultureCode;
            col.CombineWithDefaultCulture = CombineWithDefaultCulture;
            col.ClassNames = ClassNames;
            col.MaxRelativeLevel = MaxRelativeLevel;
            col.SelectOnlyPublished = SelectOnlyPublished;
            col.SelectAllData = SelectAllData;
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<TreeNode> Clone()
        {
            // Create new instance and copy over the properties
            TreeNodeCollection result = new TreeNodeCollection(DefaultClassName);
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public override void SubmitChanges()
        {
            bool clearItems = false;

            // Submit changed items
            if (Items != null)
            {
                foreach (TreeNode item in Items)
                {
                    if (item != null)
                    {
                        item.SubmitChanges(true, ParentDocument);
                    }
                }
            }

            // Delete the deleted items
            if (DeletedItems != null)
            {
                foreach (TreeNode item in DeletedItems)
                {
                    if (item != null)
                    {
                        item.Delete();
                        clearItems = true;
                    }
                }

                DeletedItems = null;
            }

            // Add the new items
            if (NewItems != null)
            {
                foreach (TreeNode item in NewItems)
                {
                    if (item != null)
                    {
                        item.SubmitChanges(true, ParentDocument);
                        clearItems = true;
                    }
                }

                NewItems = null;
            }

            // Clear the items if some changes are done
            if (clearItems)
            {
                Clear();
            }
        }

        #endregion


        #region "IHierarchicalObject Members"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            if (!SelectAllData)
            {
                RegisterProperty("WithAllData", m => ((TreeNodeCollection)m).WithAllData);
            }

            RegisterProperty("WithPermissionsCheck", m => ((TreeNodeCollection)m).WithPermissionsCheck);
        }


        /// <summary>
        /// Registers the Columns of this object
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("IsLastVersion", m => m.IsLastVersion);
        }

        #endregion
    }
}