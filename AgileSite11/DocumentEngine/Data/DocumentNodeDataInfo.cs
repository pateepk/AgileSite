using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.DocumentEngine.Internal;

[assembly: RegisterObjectType(typeof(DocumentNodeDataInfo), DocumentNodeDataInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class representing document tree structure data container.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// </remarks>
    public class DocumentNodeDataInfo : AbstractInfo<DocumentNodeDataInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NODE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentNodeDataInfoProvider), OBJECT_TYPE, "CMS.Tree", "NodeID", null, "NodeGUID", "NodeAliasPath", "NodeName", null, "NodeSiteID", "NodeParentID", OBJECT_TYPE)
        {
            CompositeObjectType = TreeNode.OBJECT_TYPE,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("NodeClassID", DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("NodeACLID", AclInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("NodeOwner", PredefinedObjectType.USER, ObjectDependencyEnum.RequiredHasDefault),
                new ObjectDependency("NodeTemplateID", PageTemplateInfo.OBJECT_TYPE),
                new ObjectDependency("NodeLinkedNodeID", OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("NodeLinkedNodeSiteID", SiteInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("NodeOriginalNodeID", OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(EventLogInfo.OBJECT_TYPE, "NodeID"),
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            LogIntegration = false,
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsInvalidation = true,
            ObjectPathColumn = "NodeAliasPath",
            ObjectLevelColumn = "NodeLevel",
            OrderColumn = "NodeOrder",
            DefaultOrderBy = "NodeName",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "NodeAliasPath",
                    "NodeHasChildren",
                    "NodeHasLinks",
                    "NodeOriginalNodeID"
                }
            },
            ContinuousIntegrationSettings =
            {
                ObjectFileNameFields = { "NodeAliasPath" }
            }
        };

        #endregion


        #region "Variables"

        private DocumentNodeDataInfo mNodeParent;
        private SiteInfo mSite;
        private ContainerCustomData mNodeCustomData;

        #endregion


        #region "Data properties"

        /// <summary>
        /// Node HTML DOCTYPE.
        /// </summary>
        [DatabaseField]
        public string NodeDocType
        {
            get
            {
                return GetValue("NodeDocType", String.Empty);
            }
            set
            {
                SetValue("NodeDocType", value);
            }
        }


        /// <summary>
        /// Node custom HTML header tags.
        /// </summary>
        [DatabaseField]
        public string NodeHeadTags
        {
            get
            {
                return GetValue("NodeHeadTags", String.Empty);
            }
            set
            {
                SetValue("NodeHeadTags", value);
            }
        }


        /// <summary>
        /// Node HTML body element attributes - custom attributes of html body tag.
        /// </summary>
        [DatabaseField]
        public string NodeBodyElementAttributes
        {
            get
            {
                return GetValue("NodeBodyElementAttributes", String.Empty);
            }
            set
            {
                SetValue("NodeBodyElementAttributes", value);
            }
        }


        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        [DatabaseField]
        public string NodeBodyScripts
        {
            get
            {
                return GetValue("NodeBodyScripts", String.Empty);
            }
            set
            {
                SetValue("NodeBodyScripts", value);
            }
        }


        /// <summary>
        /// Indicates if the security should be checked for this node. The 'null' value means that the value is inherited from the parent node.
        /// </summary>
        [DatabaseField(ValueType = typeof(bool))]
        public bool? IsSecuredNode
        {
            get
            {
                var value = GetValue("IsSecuredNode");
                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetValue<bool>(value);
            }
            set
            {
                SetValue("IsSecuredNode", value);
            }
        }


        /// <summary>
        /// Indicates if this node requires SSL protocol. The 'null' value means that the value is inherited from the parent node.
        /// 0 - Do not require SSL protocol
        /// 1 - Require SSL protocol
        /// 2 - Never require SSL protocol
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public int? RequiresSSL
        {
            get
            {
                var value = GetValue("RequiresSSL");
                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetValue<int>(value);
            }
            set
            {
                SetValue("RequiresSSL", value, value >= 0);
            }
        }


        /// <summary>
        /// Identifies whether the document allows output caching in the file system. The 'null' value means that the value is inherited from the parent node.
        /// </summary>
        [DatabaseField(ValueType = typeof(bool))]
        public bool? NodeAllowCacheInFileSystem
        {
            get
            {
                var value = GetValue("NodeAllowCacheInFileSystem");
                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetValue<bool>(value);
            }
            set
            {
                SetValue("NodeAllowCacheInFileSystem", value);
            }
        }


        /// <summary>
        /// Node name.
        /// </summary>
        [DatabaseField]
        public string NodeName
        {
            get
            {
                return GetValue("NodeName", String.Empty);
            }
            set
            {
                SetValue("NodeName", value);
            }
        }


        /// <summary>
        /// Node alias.
        /// </summary>
        [DatabaseField]
        public string NodeAlias
        {
            get
            {
                return GetValue("NodeAlias", String.Empty);
            }
            set
            {
                SetValue("NodeAlias", value);
            }
        }


        /// <summary>
        /// Node alias path.
        /// </summary>
        [DatabaseField]
        public string NodeAliasPath
        {
            get
            {
                return GetValue("NodeAliasPath", String.Empty);
            }
            set
            {
                SetValue("NodeAliasPath", value);
            }
        }


        /// <summary>
        /// Node GUID.
        /// </summary>
        [DatabaseField]
        public Guid NodeGUID
        {
            get
            {
                return GetValue("NodeGUID", Guid.Empty);
            }
            set
            {
                SetValue("NodeGUID", value);
            }
        }


        /// <summary>
        /// Indicates if node has child nodes.
        /// </summary>
        [DatabaseField]
        public bool NodeHasChildren
        {
            get
            {
                return GetBooleanValue("NodeHasChildren", false);
            }
            set
            {
                SetValue("NodeHasChildren", value);
            }
        }


        /// <summary>
        /// Indicates if node has linked nodes.
        /// </summary>
        [DatabaseField]
        public bool NodeHasLinks
        {
            get
            {
                return GetBooleanValue("NodeHasLinks", false);
            }
            set
            {
                SetValue("NodeHasLinks", value);
            }
        }


        /// <summary>
        /// Node ID.
        /// </summary>
        [DatabaseField]
        public int NodeID
        {
            get
            {
                return GetValue("NodeID", 0);
            }
            set
            {
                SetValue("NodeID", value);
            }
        }


        /// <summary>
        /// Node parent ID.
        /// </summary>
        [DatabaseField]
        public int NodeParentID
        {
            get
            {
                return GetValue("NodeParentID", 0);
            }
            set
            {
                SetValue("NodeParentID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates how long should node stay in cache (in minutes). The 'null' means that the value is inherited from parent node.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public int? NodeCacheMinutes
        {
            get
            {
                var value = GetValue("NodeCacheMinutes");
                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetValue<int>(value);
            }
            set
            {
                SetValue("NodeCacheMinutes", value);
            }
        }


        /// <summary>
        /// Node level in the tree hierarchy (starting from 0 for root node).
        /// </summary>
        [DatabaseField]
        public int NodeLevel
        {
            get
            {
                return GetValue("NodeLevel", 0);
            }
            set
            {
                SetValue("NodeLevel", value);
            }
        }


        /// <summary>
        /// Node site ID.
        /// </summary>
        [DatabaseField]
        public int NodeSiteID
        {
            get
            {
                return GetValue("NodeSiteID", 0);
            }
            set
            {
                SetValue("NodeSiteID", value);
            }
        }


        /// <summary>
        /// Node order in sibling sequence (same level).
        /// </summary>
        [DatabaseField]
        public int NodeOrder
        {
            get
            {
                return GetValue("NodeOrder", 0);
            }
            set
            {
                SetValue("NodeOrder", value);
            }
        }


        /// <summary>
        /// User ID representing the node owner.
        /// </summary>
        [DatabaseField]
        public int NodeOwner
        {
            get
            {
                return GetValue("NodeOwner", 0);
            }
            set
            {
                SetValue("NodeOwner", value, value > 0);
            }
        }


        /// <summary>
        /// E-commerce SKU (product) ID.
        /// </summary>
        [DatabaseField]
        public int NodeSKUID
        {
            get
            {
                return GetValue("NodeSKUID", 0);
            }
            set
            {
                SetValue("NodeSKUID", value, value > 0);
            }
        }


        /// <summary>
        /// Specifies from which level(s) should the document inherit the content.
        /// Empty string represents inheritance from all levels in the hierarchy
        /// "/" means that no content is inherited.
        /// "\" means that only content from master document is inherited.
        /// "/{0}/{2}" means that content from first and third level is inherited.
        /// </summary>
        [DatabaseField]
        public string NodeInheritPageLevels
        {
            get
            {
                return GetValue("NodeInheritPageLevels", String.Empty);
            }
            set
            {
                SetValue("NodeInheritPageLevels", value);
            }
        }


        /// <summary>
        /// Node page template ID - used for all culture versions if NodeTemplateForAllCultures property is true.
        /// </summary>
        [DatabaseField]
        public int NodeTemplateID
        {
            get
            {
                return GetValue("NodeTemplateID", 0);
            }
            set
            {
                SetValue("NodeTemplateID", value, value > 0);
            }
        }


        /// <summary>
        /// If true, the document uses the same template (NodeTemplateID) for all culture versions.
        /// </summary>
        [DatabaseField]
        public bool NodeTemplateForAllCultures
        {
            get
            {
                return GetValue("NodeTemplateForAllCultures", false);
            }
            set
            {
                SetValue("NodeTemplateForAllCultures", value);
            }
        }


        /// <summary>
        /// If true, the document inherits the template from parent.
        /// </summary>
        [DatabaseField]
        public bool NodeInheritPageTemplate
        {
            get
            {
                return GetValue("NodeInheritPageTemplate", false);
            }
            set
            {
                SetValue("NodeInheritPageTemplate", value);
            }
        }


        /// <summary>
        /// ID of a community group which owns the node.
        /// </summary>
        [DatabaseField]
        public int NodeGroupID
        {
            get
            {
                return GetValue("NodeGroupID", 0);
            }
            set
            {
                SetValue("NodeGroupID", value, value > 0);
            }
        }


        /// <summary>
        /// ID of a document type of this document.
        /// </summary>
        [DatabaseField]
        public int NodeClassID
        {
            get
            {
                return GetValue("NodeClassID", 0);
            }
            set
            {
                SetValue("NodeClassID", value);
            }
        }


        /// <summary>
        /// ID of a access controller list assigned to the node.
        /// </summary>
        [DatabaseField]
        public int NodeACLID
        {
            get
            {
                return GetValue("NodeACLID", 0);
            }
            set
            {
                SetValue("NodeACLID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates if node owns its ACL item.
        /// </summary>
        [DatabaseField]
        public bool NodeIsACLOwner
        {
            get
            {
                return GetValue("NodeIsACLOwner", false);
            }
            internal set
            {
                SetValue("NodeIsACLOwner", value);
            }
        }


        /// <summary>
        /// ID of a document to which this linked document is related.
        /// </summary>
        [DatabaseField]
        public int NodeLinkedNodeID
        {
            get
            {
                return GetValue("NodeLinkedNodeID", 0);
            }
            set
            {
                SetValue("NodeLinkedNodeID", value, value > 0);
            }
        }


        /// <summary>
        /// Site ID of a document to which this linked document is related.
        /// </summary>
        [DatabaseField]
        public int NodeLinkedNodeSiteID
        {
            get
            {
                return GetValue("NodeLinkedNodeSiteID", 0);
            }
            set
            {
                SetValue("NodeLinkedNodeSiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Node custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ContainerCustomData NodeCustomData
        {
            get
            {
                return mNodeCustomData ?? (mNodeCustomData = new ContainerCustomData(this, "NodeCustomData"));
            }
        }


        /// <summary>
        /// Original node ID.
        /// </summary>
        /// <remarks>
        /// Represents <see cref="NodeID"/> for standard document, <see cref="NodeLinkedNodeID"/> for linked document. 
        /// </remarks>
        [DatabaseField]
        public int NodeOriginalNodeID
        {
            get
            {
                return GetValue("NodeOriginalNodeID", 0);
            }
            set
            {
                SetValue("NodeOriginalNodeID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates whether the document is based on a content only type.
        /// </summary>
        [DatabaseField]
        internal bool NodeIsContentOnly
        {
            get
            {
                return GetValue("NodeIsContentOnly", false);
            }
            set
            {
                SetValue("NodeIsContentOnly", value);
            }
        }

        #endregion


        #region "Automatic properties"

        /// <summary>
        /// Node site name.
        /// </summary>
        [DatabaseMapping(false)]
        public string NodeSiteName
        {
            get
            {
                return Site != null ? Site.SiteName : String.Empty;
            }
        }


        /// <summary>
        /// Site of the document.
        /// </summary>
        public new SiteInfo Site
        {
            get
            {
                // Get site
                return mSite ?? (mSite = SiteInfoProvider.GetSiteInfo(NodeSiteID));
            }
        }


        /// <summary>
        /// Node class name in format application.class.
        /// </summary>
        [DatabaseMapping(false)]
        public string NodeClassName
        {
            get
            {
                return DataClassInfoProvider.GetClassName(NodeClassID);
            }
        }


        /// <summary>
        /// Indicates whether the document contains SKU data.
        /// </summary>
        [DatabaseMapping("(NodeSKUID IS NOT NULL)")]
        public bool HasSKU
        {
            get
            {
                return NodeSKUID != 0;
            }
        }


        /// <summary>
        /// Indicates whether the document is link to another document.
        /// </summary>
        [DatabaseMapping("(NodeLinkedNodeID IS NOT NULL)")]
        public bool IsLink
        {
            get
            {
                return GetValue("NodeLinkedNodeID") != null;
            }
        }


        /// <summary>
        /// Original node site ID. Returns NodeSiteID for standard document, LinkedNodeSiteID for linked document.
        /// </summary>
        [DatabaseMapping(false)]
        public int OriginalNodeSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NodeLinkedNodeSiteID"), NodeSiteID);
            }
        }


        /// <summary>
        /// Parent node instance.
        /// </summary>
        internal DocumentNodeDataInfo NodeParent
        {
            get
            {
                return mNodeParent ?? (mNodeParent = NodeParentID > 0 ? DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(NodeParentID) : null);
            }
            set
            {
                mNodeParent = value;
                if (mNodeParent != null)
                {
                    NodeParentID = mNodeParent.NodeID;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="DocumentNodeDataInfo"/>.
        /// </summary>
        public DocumentNodeDataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Initializes the data from the Data container, can be called only after calling the empty constructor.
        /// </summary>
        /// <param name="dc">Data container with the data</param>
        internal void InitFromDataContainer(IDataContainer dc)
        {
            LoadData(new LoadDataSettings(dc));
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Data need to be inserted with parent specified.  Specify NodeParentID property or use Insert(DocumentNodeDataInfo parent) instead.
        /// </summary>
        public override void Insert()
        {
            if (NodeParentID <= 0)
            {
                throw new NotSupportedException("[DocumentNodeDataInfo.Insert]: Data need to be inserted with parent specified. Specify NodeParentID property or use Insert(DocumentNodeDataInfo parent) instead.");
            }

            Insert(NodeParent);
        }


        /// <summary>
        /// Inserts node data.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        public void Insert(DocumentNodeDataInfo parent)
        {
            if (parent == null)
            {
                throw new NullReferenceException("[DocumentNodeDataInfo.Insert]: Parent node not specified.");
            }

            // Set parent
            NodeParent = parent;

            base.Insert();
        }


        /// <summary>
        /// Sets node data.
        /// </summary>
        protected override void SetObject()
        {
            DocumentNodeDataInfoProvider.SetDocumentNodeDataInfo(this);
        }


        /// <summary>
        /// Deletes node data.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentNodeDataInfoProvider.DeleteDocumentNodeDataInfo(this);
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null.</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentially modified are cleared.</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            RelationshipInfoProvider.DeleteRelationships(NodeID, true, true);
            DocumentAliasInfoProvider.DeleteNodeAliases(NodeID);

            var parameters = new QueryDataParameters();
            parameters.Add("@NodeID", NodeID);

            ConnectionHelper.ExecuteQuery("cms.tree.removedependencies", parameters);

            if (NodeIsACLOwner)
            {
                AclInfoProvider.RemoveACLsFromNodes(new WhereCondition().WhereEquals("NodeID", NodeID));
            }
        }


        /// <summary>
        /// Sets value of the specified node column.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="value">Value.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public override bool SetValue(string columnName, object value)
        {
            var result = base.SetValue(columnName, value);

            switch (columnName.ToLowerInvariant())
            {
                case "nodeid":
                    {
                        if (!IsLink)
                        {
                            // Synchronize node ID
                            var nodeId = ValidationHelper.GetInteger(value, 0);
                            NodeOriginalNodeID = nodeId;
                        }
                    }
                    break;

                case "nodelinkednodeid":
                    {
                        if (IsLink)
                        {
                            // Synchronize node ID
                            var nodeId = ValidationHelper.GetInteger(value, 0);
                            NodeOriginalNodeID = nodeId;
                        }
                    }
                    break;

                case "nodesiteid":
                    {
                        if (ItemChanged("NodeSiteID"))
                        {
                            // Clear site property if site ID changed
                            mSite = null;
                        }
                    }
                    break;
            }

            return result;
        }


        /// <summary>
        /// Loads the default object data.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            // We don't want automatic code name provided by base method, as we want the API to provide alias path
            NodeAliasPath = null;

            NodeInheritPageLevels = String.Empty;
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object.
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            var path = NodeAliasPath;
            if (String.IsNullOrEmpty(path))
            {
                // Ensure alias path if empty
                path = GetAliasPath();
            }

            return
                new WhereCondition()
                    .WhereEquals("NodeSiteID", NodeSiteID)
                    .Where(w => w
                        // Add condition for node GUID to get correct node when link or original document is retrieved
                        .WhereEquals("NodeGUID", NodeGUID)
                        .Or()
                        // "Code name" lookup
                        .WhereEquals("NodeAliasPath", path)
                    );
        }


        /// <summary>
        /// Gets the alias path from parent and node alias.
        /// </summary>
        internal string GetAliasPath()
        {
            string path = String.Empty;

            var dci = DataClassInfoProvider.GetDataClassInfo(NodeClassID);
            if (dci != null)
            {
                if (TreeNodeMethods.IsRootClassName(dci.ClassName))
                {
                    // Root path
                    path = "/";
                }
                else
                {
                    // Populate node alias path from parent if not currently available
                    var parent = NodeParent;
                    if (parent != null)
                    {
                        path = parent.NodeAliasPath.TrimEnd('/') + "/" + NodeAlias;
                    }
                }
            }

            return path;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets site name. Throws exception when site is not specified.
        /// </summary>
        internal string GetSiteName()
        {
            // Check if site is specified
            if (Site == null)
            {
                throw new NullReferenceException("Site is not specified or specified site does not exist.");
            }

            return Site.SiteName;
        }


        /// <summary>
        /// Inserts the root node data.
        /// </summary>
        internal void InsertRootInternal()
        {
            base.Insert();
        }

        #endregion
    }
}