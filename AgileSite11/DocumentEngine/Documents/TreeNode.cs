using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Relationships;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using TaskInfo = CMS.Scheduler.TaskInfo;
using TaskInfoProvider = CMS.Scheduler.TaskInfoProvider;

[assembly: RegisterObjectType(typeof(TreeNode), TreeNode.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a document
    /// </summary>
    [DebuggerDisplay("{GetType()}: {NodeAliasPath} ({DocumentName}) - {InstanceGUID}")]
    public class TreeNode : AbstractCompositeInfo<TreeNode>, ITreeNode, IDisposable, ITreeNodeMethods
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.DOCUMENT;


        /// <summary>
        /// Type information
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new TreeNodeTypeInfo(typeof(TreeNodeProvider), OBJECT_TYPE, "CMS.Document", "DocumentID", "DocumentModifiedWhen", "DocumentGUID", null, "DocumentName", null, "NodeSiteID", null, null);

        #endregion


        #region "Variables"

        private ComponentsDataLoader mDataLoader;
        private DataClassInfo mDataClassInfo;
        private List<string> mPrioritizedProperties;

        private TreeProvider mTreeProvider;
        private VersionManager mVersionManager;
        private WorkflowManager mWorkflowManager;

        private DocumentCultureDataInfo mCultureData;
        private DocumentFieldsInfo mCoupledData;

        private ContainerCustomData mNodeCustomData;
        private ContainerCustomData mDocumentCustomData;

        /// <summary>
        /// Node class name.
        /// </summary>
        protected string mClassName;

        /// <summary>
        /// Document editable content.
        /// </summary>
        protected EditableItems mDocumentContent;

        /// <summary>
        /// Document workflow step
        /// </summary>
        protected BaseInfo mWorkflowStep;

        /// <summary>
        /// Document workflow step timeout
        /// </summary>
        protected DateTime mWorkflowStepTimeout = DateTime.MinValue;

        /// <summary>
        /// Parent document.
        /// </summary>
        protected TreeNode mParent;

        /// <summary>
        /// Grouped attachments.
        /// </summary>
        protected DocumentAttachmentRepository mGroupedAttachments;

        /// <summary>
        /// If true, the document is currently in the document helper and should call normal methods.
        /// </summary>
        protected bool isInDocumentHelper;

        // Object for locking this instance context
        private readonly object lockObject = new object();

        private DocumentConnectedObjects mConnected;

        #endregion


        #region "Properties"

        /// <summary>
        /// DataClass info
        /// </summary>
        internal DataClassInfo DataClassInfo
        {
            get
            {
                string className = NodeClassName;
                if (string.IsNullOrEmpty(className) || string.Equals(className, "cms.document", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                if (mDataClassInfo == null)
                {
                    // Get the data class info
                    var dci = DataClassInfoProvider.GetDataClassInfo(className);

                    // Verify the data class
                    if (dci == null)
                    {
                        throw new DocumentTypeNotExistsException("Page type with '" + className + "' class name not found.");
                    }

                    if (!dci.ClassIsDocumentType)
                    {
                        throw new DocumentTypeNotExistsException("The '" + className + "' class name is not a page type.");
                    }

                    mDataClassInfo = dci;
                }

                return mDataClassInfo;
            }
        }


        /// <summary>
        /// Returns true if node is content only.
        /// </summary>
        [DatabaseField]
        public bool NodeIsContentOnly
        {
            get
            {
                // Prefer value from database field if set
                var dbValue = GetValue("NodeIsContentOnly");
                if (dbValue != null)
                {
                    return ValidationHelper.GetBoolean(dbValue, false);
                }

                // Otherwise evaluate based on used class
                var dci = DataClassInfo;
                return (dci != null) && dci.ClassIsContentOnly;
            }
            internal set
            {
                SetValue("NodeIsContentOnly", value);
            }
        }


        /// <summary>
        /// Data loader to load the data for <see cref="TreeNode"/> components.
        /// </summary>
        internal ComponentsDataLoader DataLoader
        {
            get
            {
                return mDataLoader ?? (mDataLoader = new ComponentsDataLoader(this));
            }
            set
            {
                mDataLoader = value;
            }
        }


        /// <summary>
        /// Document tree node data
        /// </summary>
        internal DocumentNodeDataInfo NodeData { get; set; } = DocumentNodeDataInfo.New();


        /// <summary>
        /// Document culture specific data.
        /// </summary>
        internal DocumentCultureDataInfo CultureData
        {
            get
            {
                return mCultureData ?? (mCultureData = DataLoader.LoadCultureData());
            }
            set
            {
                mCultureData = value;
            }
        }


        /// <summary>
        /// Document coupled fields data.
        /// </summary>
        internal DocumentFieldsInfo CoupledData
        {
            get
            {
                return mCoupledData ?? (mCoupledData = DataLoader.LoadCoupledData());
            }
            set
            {
                mCoupledData = value;
            }
        }


        /// <summary>
        /// If true, the document specific columns are hidden from the document
        /// </summary>
        internal bool HideTypeSpecificColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Gets current status of the object.
        /// </summary>
        protected override ObjectStatusEnum ObjectStatus
        {
            get
            {
                if (NodeID <= 0)
                {
                    return ObjectStatusEnum.New;
                }

                return mStatus;
            }
            set
            {
                if (mStatus == ObjectStatusEnum.WasDeleted)
                {
                    throw new NotSupportedException("Status of deleted object cannot be changed.");
                }

                mStatus = value;
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
                // If set to null, create a new TreeProvider
                if (value == null)
                {
                    value = new TreeProvider();
                }
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Workflow manager instance.
        /// </summary>
        public virtual WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Version manager instance.
        /// </summary>
        public virtual VersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = VersionManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Class name in format application.class.
        /// </summary>
        [DatabaseMapping("ClassName")]
        public override string ClassName
        {
            get
            {
                return NodeClassName;
            }
        }


        /// <summary>
        /// Node class name in format application.class.
        /// </summary>
        [DatabaseField("ClassName")]
        public virtual string NodeClassName
        {
            get
            {
                return mClassName;
            }
            private set
            {
                // Refresh dependant properties if class name changed
                var hasChanged = !string.Equals(mClassName, value, StringComparison.InvariantCultureIgnoreCase);
                if (hasChanged)
                {
                    // Update class name
                    mClassName = value;

                    // Refresh dependant properties
                    mDataClassInfo = null;
                    mCoupledData = null;
                    RefreshNodeDataClassId();
                    TypeInfo = TreeNodeProvider.GetTypeInfo(value);
                }
            }
        }


        /// <summary>
        /// Indicates whether the document contains coupled data.
        /// </summary>
        [DatabaseMapping("(DocumentForeignKeyValue IS NOT NULL)")]
        public virtual bool IsCoupled
        {
            get
            {
                var dataClass = DataClassInfo;
                return (dataClass != null) && dataClass.ClassIsCoupledClass;
            }
        }


        /// <summary>
        /// Indicates whether the document contains SKU data.
        /// </summary>
        [DatabaseMapping("(NodeSKUID IS NOT NULL)")]
        public virtual bool HasSKU
        {
            get
            {
                return NodeSKUID > 0;
            }
        }


        /// <summary>
        /// Indicates whether the document is link to another document.
        /// </summary>
        [DatabaseMapping("(NodeLinkedNodeID IS NOT NULL)")]
        public virtual bool IsLink
        {
            get
            {
                return NodeLinkedNodeID > 0;
            }
        }


        /// <summary>
        /// Original node ID. Represents NodeID for standard document, LinkedNodeID for linked document.
        /// </summary>
        [DatabaseField("NodeOriginalNodeID")]
        public virtual int OriginalNodeID
        {
            get
            {
                return GetValue("NodeOriginalNodeID", 0);
            }
            internal set
            {
                SetValue("NodeOriginalNodeID", value, value > 0);
            }
        }


        /// <summary>
        /// Original node site ID. Returns NodeSiteID for standard document, LinkedNodeSiteID for linked document.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual int OriginalNodeSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NodeLinkedNodeSiteID"), NodeSiteID);
            }
        }


        /// <summary>
        /// Node custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual ContainerCustomData NodeCustomData
        {
            get
            {
                return mNodeCustomData ?? (mNodeCustomData = new ContainerCustomData(this, "NodeCustomData"));
            }
        }


        /// <summary>
        /// Document custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual ContainerCustomData DocumentCustomData
        {
            get
            {
                return mDocumentCustomData ?? (mDocumentCustomData = new ContainerCustomData(this, "DocumentCustomData"));
            }
        }


        /// <summary>
        /// Document content.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual EditableItems DocumentContent
        {
            get
            {
                if (mDocumentContent == null)
                {
                    // Load the custom data
                    mDocumentContent = new EditableItems();
                    mDocumentContent.LoadContentXml(ValidationHelper.GetString(CultureData.GetValue("DocumentContent"), String.Empty));
                }
                return mDocumentContent;
            }
            internal set
            {
                CultureData.DocumentContent = value;
            }
        }


        /// <summary>
        /// Name of the coupled class ID column.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual string CoupledClassIDColumn
        {
            get
            {
                return IsCoupled ? CoupledData.TypeInfo.IDColumn : null;
            }
        }


        /// <summary>
        /// Returns search type.
        /// </summary>
        public override string SearchType
        {
            get
            {
                return OBJECT_TYPE;
            }
        }


        /// <summary>
        /// If true, Events tasks are logged on the object update.
        /// </summary>
        protected override bool LogEvents
        {
            get
            {
                return base.LogEvents && TreeProvider.LogEvents;
            }
            set
            {
                base.LogEvents = value;
            }
        }


        /// <summary>
        /// Indicates if the record should be logged to Event log for document actions.
        /// </summary>
        internal bool LogEventsInternal
        {
            get
            {
                return LogEvents;
            }
        }


        /// <summary>
        /// If true, cache dependencies are touched when the object is changed.
        /// </summary>
        protected override bool TouchCacheDependencies
        {
            get
            {
                return base.TouchCacheDependencies && TreeProvider.TouchCacheDependencies;
            }
            set
            {
                base.TouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// If true, the document is the last version (retrieved using DocumentHelper.GetDocument).
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool IsLastVersion
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the document is in the default culture for the given web site.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool IsDefaultCulture
        {
            get
            {
                // Compare the document culture with the default one
                string defaultCulture = CultureHelper.GetDefaultCultureCode(NodeSiteName);
                return string.Equals(DocumentCulture, defaultCulture, StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// List of properties which should be prioritized in the macro controls (IntelliSense, MacroTree).
        /// </summary>
        protected override List<string> PrioritizedProperties
        {
            get
            {
                if (mPrioritizedProperties == null)
                {
                    mPrioritizedProperties = new List<string>();

                    if (!HideTypeSpecificColumns)
                    {
                        mPrioritizedProperties.AddRange(GetTypeSpecificColumnNames());
                    }

                    mPrioritizedProperties.Add("DocumentName");
                }

                return mPrioritizedProperties;
            }
        }


        /// <summary>
        /// Gets document absolute URL.
        /// </summary>
        [DatabaseMapping(false)]
        public string AbsoluteURL
        {
            get
            {
                // Get site domain
                string domainName = null;
                if (Site != null)
                {
                    domainName = Site.DomainName;
                }

                return URLHelper.GetAbsoluteUrl(RelativeURL, domainName);
            }
        }


        /// <summary>
        /// Gets document relative URL.
        /// </summary>
        [DatabaseMapping(false)]
        public string RelativeURL
        {
            get
            {
                return DocumentURLProvider.GetUrl(this);
            }
        }


        /// <summary>
        /// Gets document permanent absolute URL.
        /// </summary>
        [DatabaseMapping(false)]
        public string PermanentURL
        {
            get
            {
                // Get site domain
                string domainName = null;
                if (Site != null)
                {
                    domainName = Site.DomainName;
                }

                var permanentUrl = DocumentURLProvider.GetPermanentDocUrl(NodeGUID, NodeAlias, NodeSiteName);

                return URLHelper.GetAbsoluteUrl(permanentUrl, domainName);
            }
        }

        #endregion


        #region "Connected objects properties"

        /// <summary>
        /// Document owner
        /// </summary>
        public virtual UserInfo Owner
        {
            get
            {
                // Get the node owner
                object value;
                GetReferencedObject("NodeOwner", out value);

                return (UserInfo)value;
            }
        }


        /// <summary>
        /// Object display name.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                var name = base.ObjectDisplayName;
                if (String.IsNullOrEmpty(name))
                {
                    return ResHelper.Slash;
                }

                return name;
            }
            set
            {
                base.ObjectDisplayName = value;
            }
        }


        /// <summary>
        /// Object parent
        /// </summary>
        protected override BaseInfo ObjectParent
        {
            get
            {
                return Parent ?? base.ObjectParent;
            }
            set
            {
                // Assign the internal parent
                var node = value as TreeNode;
                if (node != null)
                {
                    mParent = node;
                }

                base.ObjectParent = value;
            }
        }


        /// <summary>
        /// Parent document.
        /// </summary>
        public new virtual TreeNode Parent
        {
            get
            {
                // No parent for root document
                if (IsRootNode())
                {
                    return null;
                }

                return mParent ?? (mParent = GetParentNode());
            }
        }


        /// <summary>
        /// Connected documents.
        /// </summary>
        public virtual TreeNodeRepository ConnectedDocuments
        {
            get
            {
                return Connected.ConnectedDocuments;
            }
        }


        /// <summary>
        /// Collection of child nodes.
        /// </summary>
        public virtual new TreeNodeCollection Children
        {
            get
            {
                return Connected.Children;
            }
        }


        /// <summary>
        /// Collection of all child nodes from all levels.
        /// </summary>
        public virtual TreeNodeCollection AllChildren
        {
            get
            {
                return Connected.AllChildren;
            }
        }


        /// <summary>
        /// Collection of linked nodes, including the original.
        /// </summary>
        public virtual TreeNodeCollection Links
        {
            get
            {
                return Connected.Links;
            }
        }


        /// <summary>
        /// Collection of all culture versions of this document.
        /// </summary>
        public virtual TreeNodeCollection CultureVersions
        {
            get
            {
                return Connected.CultureVersions;
            }
        }


        /// <summary>
        /// Collection of documents on the path to the current document
        /// </summary>
        public virtual TreeNodeCollection DocumentsOnPath
        {
            get
            {
                return Connected.DocumentsOnPath;
            }
        }


        /// <summary>
        /// Connected objects.
        /// </summary>
        public virtual InfoObjectRepository ConnectedObjects
        {
            get
            {
                return Connected.ConnectedObjects;
            }
        }


        /// <summary>
        /// Document grouped attachments.
        /// </summary>
        public virtual DocumentAttachmentRepository GroupedAttachments
        {
            get
            {
                if (mGroupedAttachments == null)
                {
                    lock (lockObject)
                    {
                        if (mGroupedAttachments == null)
                        {
                            // Prepare the repository
                            var repository = new DocumentAttachmentRepository(this)
                            {
                                // Disable nice names of the collections to avoid collisions e.g. Image vs. Images
                                AllowNiceNames = false
                            };

                            // Register grouped attachments
                            RegisterGroupedAttachments(repository);

                            mGroupedAttachments = repository;
                        }
                    }
                }

                return mGroupedAttachments;
            }
        }


        /// <summary>
        /// Collection of document tags.
        /// </summary>
        public virtual IInfoObjectCollection Tags
        {
            get
            {
                return Connected.Tags;
            }
        }


        /// <summary>
        /// Collection of document categories.
        /// </summary>
        public virtual IInfoObjectCollection Categories
        {
            get
            {
                return Connected.Categories;
            }
        }


        /// <summary>
        /// Collection of all document attachments (all field, grouped and unsorted).
        /// </summary>
        public virtual DocumentAttachmentCollection AllAttachments
        {
            get
            {
                return Connected.AllAttachments;
            }
        }


        /// <summary>
        /// Collection of unsorted document attachments.
        /// </summary>
        public virtual DocumentAttachmentCollection Attachments
        {
            get
            {
                return Connected.Attachments;
            }
        }


        /// <summary>
        /// Collection of the version history of the document
        /// </summary>
        public virtual IInfoObjectCollection VersionHistory
        {
            get
            {
                return Connected.VersionHistory;
            }
        }


        /// <summary>
        /// Collection of the attachment history of the document
        /// </summary>
        public virtual IInfoObjectCollection AttachmentHistory
        {
            get
            {
                return Connected.AttachmentHistory;
            }
        }


        /// <summary>
        /// Collection of the workflow history of the document
        /// </summary>
        public virtual IInfoObjectCollection WorkflowHistory
        {
            get
            {
                return Connected.WorkflowHistory;
            }
        }


        /// <summary>
        /// Collection of the document aliases.
        /// </summary>
        public virtual IInfoObjectCollection Aliases
        {
            get
            {
                return Connected.Aliases;
            }
        }


        /// <summary>
        /// Collection of the message boards for this document
        /// </summary>
        public virtual IInfoObjectCollection MessageBoards
        {
            get
            {
                return Connected.MessageBoards;
            }
        }


        /// <summary>
        /// Collection of the ad-hoc forums for this document
        /// </summary>
        public virtual IInfoObjectCollection Forums
        {
            get
            {
                return Connected.Forums;
            }
        }


        /// <summary>
        /// Collection of the personalizations (widget settings) for the document
        /// </summary>
        public virtual IInfoObjectCollection Personalizations
        {
            get
            {
                return Connected.Personalizations;
            }
        }


        /// <summary>
        /// Related documents.
        /// </summary>
        public virtual TreeNodeRepository RelatedDocuments
        {
            get
            {
                return Connected.RelatedDocuments;
            }
        }

        #endregion


        #region "Node properties"

        /// <summary>
        /// Node document type.
        /// </summary>
        [DatabaseField]
        public virtual string NodeDocType
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
        /// Node header tags.
        /// </summary>
        [DatabaseField]
        public virtual string NodeHeadTags
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
        /// Node body element attributes - attributes of html body tag.
        /// </summary>
        [DatabaseField]
        public virtual string NodeBodyElementAttributes
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
        public virtual string NodeBodyScripts
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
        public virtual bool? IsSecuredNode
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
        public virtual int? RequiresSSL
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
        public virtual bool? NodeAllowCacheInFileSystem
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
        public virtual string NodeName
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
        public virtual string NodeAlias
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
        /// Node alias path. This property is read only.
        /// </summary>
        [DatabaseField]
        public virtual string NodeAliasPath
        {
            get
            {
                return GetValue("NodeAliasPath", String.Empty);
            }
            internal set
            {
                SetValue("NodeAliasPath", value);
            }
        }


        /// <summary>
        /// Node GUID to identify document node within site tree.
        /// </summary>
        [DatabaseField]
        public virtual Guid NodeGUID
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
        /// Indicates if node has children.
        /// </summary>
        [DatabaseField]
        public bool NodeHasChildren
        {
            get
            {
                return GetBooleanValue("NodeHasChildren", false);
            }
            internal set
            {
                SetValue("NodeHasChildren", value);
            }
        }


        /// <summary>
        /// Indicates if node has links.
        /// </summary>
        [DatabaseField]
        public bool NodeHasLinks
        {
            get
            {
                return GetBooleanValue("NodeHasLinks", false);
            }
            internal set
            {
                SetValue("NodeHasLinks", value);
            }
        }


        /// <summary>
        /// Node ID. This property is read only.
        /// </summary>
        [DatabaseField]
        public virtual int NodeID
        {
            get
            {
                return GetValue("NodeID", 0);
            }
            internal set
            {
                SetValue("NodeID", value);
            }
        }


        /// <summary>
        /// Document parent node ID.
        /// </summary>
        [DatabaseField]
        public virtual int NodeParentID
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
        public virtual int? NodeCacheMinutes
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
        /// Document node level. This property is read only.
        /// </summary>
        [DatabaseField]
        public virtual int NodeLevel
        {
            get
            {
                return GetValue("NodeLevel", 0);
            }
            internal set
            {
                SetValue("NodeLevel", value);
            }
        }


        /// <summary>
        /// Node site ID.
        /// </summary>
        [DatabaseField]
        public virtual int NodeSiteID
        {
            get
            {
                return GetValue("NodeSiteID", 0);
            }
            internal set
            {
                SetValue("NodeSiteID", value);
            }
        }


        /// <summary>
        /// Node site name.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual string NodeSiteName
        {
            get
            {
                return NodeData.NodeSiteName;
            }
        }


        /// <summary>
        /// Site of the document
        /// </summary>
        public new SiteInfo Site
        {
            get
            {
                // Get site
                return NodeData.Site;
            }
        }


        /// <summary>
        /// Node order in sibling sequence. This property is read only.
        /// </summary>
        [DatabaseField]
        public virtual int NodeOrder
        {
            get
            {
                return GetValue("NodeOrder", 0);
            }
            internal set
            {
                SetValue("NodeOrder", value, value >= 0);
            }
        }


        /// <summary>
        /// Node owner, read only.
        /// </summary>
        [DatabaseField]
        public virtual int NodeOwner
        {
            get
            {
                return GetValue("NodeOwner", 0);
            }
            set
            {
                SetIntegerValue("NodeOwner", value, false);
            }
        }


        /// <summary>
        /// E-commerce SKU (product) ID.
        /// </summary>
        [DatabaseField]
        public virtual int NodeSKUID
        {
            get
            {
                return GetValue("NodeSKUID", 0);
            }
            set
            {
                SetIntegerValue("NodeSKUID", value, false);
            }
        }


        /// <summary>
        /// Node inherit page levels.
        /// </summary>
        [DatabaseField]
        public virtual string NodeInheritPageLevels
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
        /// Node page template ID - used for all culture versions if NodeTemplateForAllCultures is true
        /// </summary>
        [DatabaseField]
        public virtual int NodeTemplateID
        {
            get
            {
                return GetValue("NodeTemplateID", 0);
            }
            set
            {
                SetIntegerValue("NodeTemplateID", value, false);
            }
        }


        /// <summary>
        /// If true, the document uses the same template (NodeTemplateID) for all culture versions
        /// </summary>
        [DatabaseField]
        public virtual bool NodeTemplateForAllCultures
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
        /// If true, the document inherits the page template from parent
        /// </summary>
        [DatabaseField]
        public virtual bool NodeInheritPageTemplate
        {
            get
            {
                // Get the value
                object value = GetValue("NodeInheritPageTemplate");
                if (value == null)
                {
                    // Evaluate automatically
                    return (GetUsedPageTemplateId() <= 0);
                }

                return ValidationHelper.GetBoolean(value, false);
            }
            set
            {
                SetValue("NodeInheritPageTemplate", value);
            }
        }


        /// <summary>
        /// ID of a access controller list assigned to the node.
        /// </summary>
        [DatabaseField]
        public virtual int NodeACLID
        {
            get
            {
                return GetValue("NodeACLID", 0);
            }
            internal set
            {
                SetIntegerValue("NodeACLID", value, false);
            }
        }


        /// <summary>
        /// Indicates if node owns its ACL item
        /// </summary>
        [DatabaseField]
        public virtual bool NodeIsACLOwner
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
        public virtual int NodeLinkedNodeID
        {
            get
            {
                return GetValue("NodeLinkedNodeID", 0);
            }
            internal set
            {
                SetIntegerValue("NodeLinkedNodeID", value, false);
            }
        }


        /// <summary>
        /// Site ID of a document to which this linked document is related.
        /// </summary>
        [DatabaseField]
        public virtual int NodeLinkedNodeSiteID
        {
            get
            {
                return GetValue("NodeLinkedNodeSiteID", 0);
            }
            internal set
            {
                SetIntegerValue("NodeLinkedNodeSiteID", value, false);
            }
        }


        /// <summary>
        /// ID of a community group which owns the node.
        /// </summary>
        [DatabaseField]
        public virtual int NodeGroupID
        {
            get
            {
                return GetValue("NodeGroupID", 0);
            }
            internal set
            {
                SetIntegerValue("NodeGroupID", value, false);
            }
        }

        #endregion


        #region "Document properties"

        /// <summary>
        /// Indicates if redirection to first child document should be performed when accessed
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentMenuRedirectToFirstChild
        {
            get
            {
                return GetValue("DocumentMenuRedirectToFirstChild", false);
            }
            set
            {
                SetValue("DocumentMenuRedirectToFirstChild", value);
            }
        }


        /// <summary>
        /// URL to which the document is redirected when accessed
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuRedirectUrl
        {
            get
            {
                return GetValue("DocumentMenuRedirectUrl", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuRedirectUrl", value);
            }
        }


        /// <summary>
        /// JavaScript code that is executed upon click on the document in the menus.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuJavascript
        {
            get
            {
                return GetValue("DocumentMenuJavascript", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuJavascript", value);
            }
        }


        /// <summary>
        /// Document name
        /// </summary>
        [DatabaseField]
        public virtual string DocumentName
        {
            get
            {
                return GetValue("DocumentName", String.Empty);
            }
            set
            {
                SetValue("DocumentName", value);
            }
        }


        /// <summary>
        /// Document name path
        /// </summary>
        [DatabaseField]
        public virtual string DocumentNamePath
        {
            get
            {
                return GetValue("DocumentNamePath", String.Empty);
            }
            internal set
            {
                SetValue("DocumentNamePath", value);
            }
        }


        /// <summary>
        /// Document type, contains the document extension.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentType
        {
            get
            {
                return GetValue("DocumentType", String.Empty);
            }
            internal set
            {
                SetValue("DocumentType", value);
            }
        }


        /// <summary>
        /// Document URL path.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentUrlPath
        {
            get
            {
                return GetValue("DocumentUrlPath", String.Empty);
            }
            set
            {
                SetValue("DocumentUrlPath", value);
            }
        }


        /// <summary>
        /// Document culture.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentCulture
        {
            get
            {
                return GetValue("DocumentCulture", String.Empty);
            }
            set
            {
                SetValue("DocumentCulture", value);
            }
        }


        /// <summary>
        /// Document menu caption.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuCaption
        {
            get
            {
                return GetValue("DocumentMenuCaption", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuCaption", value);
            }
        }


        /// <summary>
        /// Document menu style.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuStyle
        {
            get
            {
                return GetValue("DocumentMenuStyle", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuStyle", value);
            }
        }


        /// <summary>
        /// Document menu item image.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemImage
        {
            get
            {
                return GetValue("DocumentMenuItemImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemImage", value);
            }
        }


        /// <summary>
        /// Document menu item left image.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemLeftImage
        {
            get
            {
                return GetValue("DocumentMenuItemLeftImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemLeftImage", value);
            }
        }


        /// <summary>
        /// Document menu item right image.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemRightImage
        {
            get
            {
                return GetValue("DocumentMenuItemRightImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemRightImage", value);
            }
        }


        /// <summary>
        /// Document menu class.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuClass
        {
            get
            {
                return GetValue("DocumentMenuClass", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuClass", value);
            }
        }


        /// <summary>
        /// Document menu highlighted style.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuStyleHighlighted
        {
            get
            {
                return GetValue("DocumentMenuStyleHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuStyleHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu highlighted class.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuClassHighlighted
        {
            get
            {
                return GetValue("DocumentMenuClassHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuClassHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item highlighted image.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemImageHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item left image highlighted.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemLeftImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemLeftImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemLeftImageHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item right image highlighted.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentMenuItemRightImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemRightImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemRightImageHighlighted", value);
            }
        }


        /// <summary>
        /// Indicates whether item is inactive in document menu.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentMenuItemInactive
        {
            get
            {
                return GetValue("DocumentMenuItemInactive", false);
            }
            set
            {
                SetValue("DocumentMenuItemInactive", value);
            }
        }


        /// <summary>
        /// Document page template for specific culture version.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentPageTemplateID
        {
            get
            {
                return GetValue("DocumentPageTemplateID", 0);
            }
            set
            {
                SetIntegerValue("DocumentPageTemplateID", value, false);
            }
        }


        /// <summary>
        /// Document ID.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentID
        {
            get
            {
                return GetValue("DocumentID", 0);
            }
            internal set
            {
                SetValue("DocumentID", value);
            }
        }


        /// <summary>
        /// Automatically use document name path for the UrlPath.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentUseNamePathForUrlPath
        {
            get
            {
                return GetValue("DocumentUseNamePathForUrlPath", false);
            }
            set
            {
                SetValue("DocumentUseNamePathForUrlPath", value);
            }
        }


        /// <summary>
        /// Use custom document extensions.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentUseCustomExtensions
        {
            get
            {
                return GetValue("DocumentUseCustomExtensions", false);
            }
            set
            {
                SetValue("DocumentUseCustomExtensions", value);
            }
        }


        /// <summary>
        /// Document stylesheet ID.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentStylesheetID
        {
            get
            {
                return GetValue("DocumentStylesheetID", 0);
            }
            set
            {
                SetValue("DocumentStylesheetID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates if document inherits stylesheet from the parent.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentInheritsStylesheet
        {
            get
            {
                return GetValue("DocumentInheritsStylesheet", true);
            }
            set
            {
                SetValue("DocumentInheritsStylesheet", value);
            }
        }


        /// <summary>
        /// Document foreign key value
        /// </summary>
        [DatabaseField]
        internal virtual int DocumentForeignKeyValue
        {
            get
            {
                return GetValue("DocumentForeignKeyValue", 0);
            }
            set
            {
                SetIntegerValue("DocumentForeignKeyValue", value, false);
            }
        }


        /// <summary>
        /// Document workflow step name.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual string WorkflowStepName
        {
            get
            {
                return WorkflowStep != null ? WorkflowStep.StepName : String.Empty;
            }
        }


        /// <summary>
        /// Document workflow step type.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual WorkflowStepTypeEnum WorkflowStepType
        {
            get
            {
                return WorkflowStep != null ? WorkflowStep.StepType : WorkflowStepTypeEnum.Undefined;
            }
        }


        /// <summary>
        /// Document workflow step.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual WorkflowStepInfo WorkflowStep
        {
            get
            {
                return (WorkflowStepInfo)InfoHelper.EnsureInfo(ref mWorkflowStep, GetWorkflowStep);
            }
        }


        /// <summary>
        /// Document workflow step timeout date (for steps with timeout).
        /// </summary>
        [DatabaseMapping(false)]
        public virtual DateTime WorkflowStepTimeout
        {
            get
            {
                if (mWorkflowStepTimeout == DateTime.MinValue)
                {
                    if (DocumentWorkflowStepID > 0)
                    {
                        string taskName = WorkflowHelper.GetScheduledTaskName(DocumentGUID);
                        TaskInfo existingTask = TaskInfoProvider.GetTaskInfo(taskName, NodeSiteID);
                        if (existingTask != null)
                        {
                            mWorkflowStepTimeout = existingTask.TaskNextRunTime;
                        }
                    }

                    // Set dummy date
                    if (mWorkflowStepTimeout == DateTime.MinValue)
                    {
                        mWorkflowStepTimeout = DateTimeHelper.ZERO_TIME;
                    }
                }

                return mWorkflowStepTimeout;
            }
        }


        /// <summary>
        /// Indicates whether the document is published.
        /// </summary>
        /// <remarks>
        /// Publish state is determined based on workflow state and publish from/to values.
        /// </remarks>
        [DatabaseMapping(false)]
        public virtual bool IsPublished
        {
            get
            {
                return DocumentHelper.GetPublished(this);
            }
        }


        /// <summary>
        /// Indicates whether document can be considered as published from perspective of workflow state. Publish from/to values are ignored.
        /// </summary>
        [DatabaseField]
        protected bool DocumentCanBePublished
        {
            get
            {
                return CultureData.DocumentCanBePublished;
            }
        }


        /// <summary>
        /// Indicates whether the document is archived.
        /// </summary>
        [DatabaseMapping("((DocumentWorkflowStepID IS NULL) OR (DocumentIsArchived = 1))")]
        public virtual bool IsArchived
        {
            get
            {
                if (DocumentWorkflowStepID == 0)
                {
                    return false;
                }

                // Check if the document is archived
                return DocumentIsArchived;
            }
        }


        /// <summary>
        /// Indicates whether the node is in published step disregarding publish from/to values.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool IsInPublishStep
        {
            get
            {
                // Check published / checked out version
                bool checkedOutVersionHistoryExists = (DocumentCheckedOutVersionHistoryID > 0);
                bool publishedVersionHistoryExists = (DocumentPublishedVersionHistoryID > 0);

                if (checkedOutVersionHistoryExists != publishedVersionHistoryExists)
                {
                    return false;
                }

                // Check node workflow step
                return ((WorkflowStepType == WorkflowStepTypeEnum.DocumentPublished) || (WorkflowStepType == WorkflowStepTypeEnum.Undefined));
            }
        }


        /// <summary>
        /// Indicates whether the document is checked out.
        /// </summary>
        [DatabaseMapping("(DocumentCheckedOutByUserID IS NOT NULL)")]
        public new bool IsCheckedOut
        {
            get
            {
                return (DocumentCheckedOutByUserID != 0);
            }
        }


        /// <summary>
        /// Indicates whether there is a published version for current document.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual bool PublishedVersionExists
        {
            get
            {
                // Archived document is not published
                if (IsArchived)
                {
                    return false;
                }

                bool checkedOutVersionHistoryExists = (DocumentCheckedOutVersionHistoryID > 0);
                bool publishedVersionHistoryExists = (DocumentPublishedVersionHistoryID > 0);

                if (publishedVersionHistoryExists || !checkedOutVersionHistoryExists)
                {
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates from when the document should be published.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentPublishFrom
        {
            get
            {
                return GetValue("DocumentPublishFrom", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentPublishFrom", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Indicates to when the document should be published.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentPublishTo
        {
            get
            {
                return GetValue("DocumentPublishTo", DateTime.MaxValue);
            }
            set
            {
                SetValue("DocumentPublishTo", value, DateTime.MaxValue);
            }
        }


        /// <summary>
        /// Document extensions.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentExtensions
        {
            get
            {
                return GetValue("DocumentExtensions", String.Empty);
            }
            set
            {
                SetValue("DocumentExtensions", value);
            }
        }


        /// <summary>
        /// Document conversion name - reflects the "TrackConversionName" data column.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentTrackConversionName
        {
            get
            {
                return GetValue("DocumentTrackConversionName", String.Empty);
            }
            set
            {
                SetValue("DocumentTrackConversionName", value);
            }
        }


        /// <summary>
        /// Document conversion value.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentConversionValue
        {
            get
            {
                return GetValue("DocumentConversionValue", String.Empty);
            }
            set
            {
                SetValue("DocumentConversionValue", value);
            }
        }


        /// <summary>
        /// Document tags.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentTags
        {
            get
            {
                return GetValue("DocumentTags", String.Empty);
            }
            set
            {
                SetValue("DocumentTags", value);
            }
        }


        /// <summary>
        /// Document tag group ID.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentTagGroupID
        {
            get
            {
                return GetValue("DocumentTagGroupID", 0);
            }
            set
            {
                SetIntegerValue("DocumentTagGroupID", value, false);
            }
        }


        /// <summary>
        /// Document wild card rule.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentWildcardRule
        {
            get
            {
                return GetValue("DocumentWildcardRule", String.Empty);
            }
            set
            {
                SetValue("DocumentWildcardRule", value);
            }
        }


        /// <summary>
        /// Sum of all ratings.
        /// </summary>
        [DatabaseField]
        public virtual double DocumentRatingValue
        {
            get
            {
                return GetValue("DocumentRatingValue", 0.0);
            }
            set
            {
                SetValue("DocumentRatingValue", value);
            }
        }


        /// <summary>
        /// Number of ratings.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentRatings
        {
            get
            {
                return GetValue("DocumentRatings", 0);
            }
            set
            {
                SetValue("DocumentRatings", value);
            }
        }


        /// <summary>
        /// Document priority.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentPriority
        {
            get
            {
                return GetValue("DocumentPriority", 0);
            }
            set
            {
                SetValue("DocumentPriority", value);
            }
        }


        /// <summary>
        /// Document published version history ID (latest published document version).
        /// </summary>
        [DatabaseField]
        public virtual int DocumentPublishedVersionHistoryID
        {
            get
            {
                return GetValue("DocumentPublishedVersionHistoryID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentPublishedVersionHistoryID", value, false);
            }
        }


        /// <summary>
        /// Document checked out version history ID (latest document version).
        /// </summary>
        [DatabaseField]
        public virtual int DocumentCheckedOutVersionHistoryID
        {
            get
            {
                return GetValue("DocumentCheckedOutVersionHistoryID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentCheckedOutVersionHistoryID", value, false);
            }
        }


        /// <summary>
        /// ID of a user who has checked the document out.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentCheckedOutByUserID
        {
            get
            {
                return GetValue("DocumentCheckedOutByUserID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentCheckedOutByUserID", value, false);
            }
        }


        /// <summary>
        /// Document workflow step ID.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentWorkflowStepID
        {
            get
            {
                return GetValue("DocumentWorkflowStepID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentWorkflowStepID", value, false);
            }
        }


        /// <summary>
        /// Indicates whether document is archived.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentIsArchived
        {
            get
            {
                return GetValue("DocumentIsArchived", false);
            }
            internal set
            {
                SetValue("DocumentIsArchived", value);
            }
        }


        /// <summary>
        /// Returns string representing workflow action status.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentWorkflowActionStatus
        {
            get
            {
                return GetValue<string>("DocumentWorkflowActionStatus", null);
            }
            internal set
            {
                SetValue("DocumentWorkflowActionStatus", value);
            }
        }


        /// <summary>
        /// Indicates whether the document will be excluded from search.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentSearchExcluded
        {
            get
            {
                return GetValue("DocumentSearchExcluded", false);
            }
            set
            {
                SetValue("DocumentSearchExcluded", value);
            }
        }


        /// <summary>
        /// Document hash.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentHash
        {
            get
            {
                return GetValue<string>("DocumentHash", null);
            }
            set
            {
                SetValue("DocumentHash", value);
            }
        }


        /// <summary>
        /// Indicates whether any activity is tracked for this document.
        /// </summary>
        [DatabaseField(ValueType = typeof(bool))]
        public virtual bool? DocumentLogVisitActivity
        {
            get
            {
                object value = GetValue("DocumentLogVisitActivity");
                if ((value == DBNull.Value) || (value == null))
                {
                    return null;
                }
                return ValidationHelper.GetBoolean(value, false);
            }
            set
            {
                SetValue("DocumentLogVisitActivity", value);
            }
        }


        /// <summary>
        /// GUID to identify the document within site.
        /// </summary>
        [DatabaseField]
        public virtual Guid DocumentGUID
        {
            get
            {
                return GetValue("DocumentGUID", Guid.Empty);
            }
            set
            {
                SetValue("DocumentGUID", value);
            }
        }


        /// <summary>
        /// Workflow cycle GUID to obtain preview link for document.
        /// </summary>
        [DatabaseField]
        public virtual Guid DocumentWorkflowCycleGUID
        {
            get
            {
                return GetValue("DocumentWorkflowCycleGUID", Guid.Empty);
            }
            set
            {
                SetValue("DocumentWorkflowCycleGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the sitemap settings in format frequency;priority.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentSitemapSettings
        {
            get
            {
                return GetValue("DocumentSitemapSettings", String.Empty);
            }
            set
            {
                SetValue("DocumentSitemapSettings", value);
            }
        }


        /// <summary>
        /// Indicates whether the document is in the process of translation (submitted to a translation service).
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentIsWaitingForTranslation
        {
            get
            {
                return GetValue("DocumentIsWaitingForTranslation", false);
            }
            set
            {
                SetValue("DocumentIsWaitingForTranslation", value);
            }
        }


        /// <summary>
        /// ID of a user who has created the document.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentCreatedByUserID
        {
            get
            {
                return GetValue("DocumentCreatedByUserID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentCreatedByUserID", value, false);
            }
        }


        /// <summary>
        /// Date and time when the document was created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentCreatedWhen
        {
            get
            {
                return GetValue("DocumentCreatedWhen", DateTime.MinValue);
            }
            internal set
            {
                SetValue("DocumentCreatedWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document group web parts (widgets).
        /// </summary>
        [DatabaseField]
        public virtual string DocumentGroupWebParts
        {
            get
            {
                return GetValue("DocumentGroupWebParts", String.Empty);
            }
            internal set
            {
                SetValue("DocumentGroupWebParts", value);
            }
        }


        /// <summary>
        /// Indicates if document is checked out/in automatically
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentCheckedOutAutomatically
        {
            get
            {
                return GetValue("DocumentCheckedOutAutomatically", true);
            }
            internal set
            {
                SetValue("DocumentCheckedOutAutomatically", value, !value);
            }
        }


        /// <summary>
        /// Date and time when was the document checked out.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentCheckedOutWhen
        {
            get
            {
                return GetValue("DocumentCheckedOutWhen", DateTime.MinValue);
            }
            internal set
            {
                SetValue("DocumentCheckedOutWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Date and time when was the document last published.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentLastPublished
        {
            get
            {
                return GetValue("DocumentLastPublished", DateTime.MinValue);
            }
            internal set
            {
                SetValue("DocumentLastPublished", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document last version name.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentLastVersionName
        {
            get
            {
                return GetValue("DocumentLastVersionName", String.Empty);
            }
            internal set
            {
                SetValue("DocumentLastVersionName", value);
            }
        }


        /// <summary>
        /// Document last version number.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentLastVersionNumber
        {
            get
            {
                return GetValue("DocumentLastVersionNumber", String.Empty);
            }
            internal set
            {
                SetValue("DocumentLastVersionNumber", value);
            }
        }


        /// <summary>
        /// Indicates if document is hidden in navigation
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentMenuItemHideInNavigation
        {
            get
            {
                return GetValue("DocumentMenuItemHideInNavigation", false);
            }
            internal set
            {
                SetValue("DocumentMenuItemHideInNavigation", value);
            }
        }


        /// <summary>
        /// ID of a user who modified the document.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentModifiedByUserID
        {
            get
            {
                return GetValue("DocumentModifiedByUserID", 0);
            }
            internal set
            {
                SetIntegerValue("DocumentModifiedByUserID", value, false);
            }
        }


        /// <summary>
        /// Date and time when was the document modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DocumentModifiedWhen
        {
            get
            {
                return GetValue("DocumentModifiedWhen", DateTime.MinValue);
            }
            internal set
            {
                SetValue("DocumentModifiedWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document page description.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentPageDescription
        {
            get
            {
                return GetValue("DocumentPageDescription", String.Empty);
            }
            internal set
            {
                SetValue("DocumentPageDescription", value);
            }
        }


        /// <summary>
        /// Document page title.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentPageTitle
        {
            get
            {
                return GetValue("DocumentPageTitle", String.Empty);
            }
            internal set
            {
                SetValue("DocumentPageTitle", value);
            }
        }


        /// <summary>
        /// Document page key words.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentPageKeyWords
        {
            get
            {
                return GetValue("DocumentPageKeyWords", String.Empty);
            }
            internal set
            {
                SetValue("DocumentPageKeyWords", value);
            }
        }


        /// <summary>
        /// Indicates if the document is visible in the site map.
        /// </summary>
        [DatabaseField]
        public virtual bool DocumentShowInSiteMap
        {
            get
            {
                return GetValue("DocumentShowInSiteMap", true);
            }
            internal set
            {
                SetValue("DocumentShowInSiteMap", value);
            }
        }


        /// <summary>
        /// Document web parts.
        /// </summary>
        [DatabaseField]
        public virtual string DocumentWebParts
        {
            get
            {
                return GetValue("DocumentWebParts", String.Empty);
            }
            internal set
            {
                SetValue("DocumentWebParts", value);
            }
        }


        /// <summary>
        /// Document web parts.
        /// </summary>
        [DatabaseField]
        public virtual int DocumentNodeID
        {
            get
            {
                return GetValue("DocumentNodeID", 0);
            }
            internal set
            {
                SetValue("DocumentNodeID", value);
            }
        }


        /// <summary>
        /// Connected objects
        /// </summary>
        internal DocumentConnectedObjects Connected
        {
            get
            {
                return mConnected ?? (mConnected = new DocumentConnectedObjects(this));
            }
        }

        #endregion


        #region "New methods"

        /// <summary>
        /// Creates a new object from the given DataRow
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            string className = null;

            var data = settings.Data;
            if (data != null)
            {
                // Try to get class name from given data
                className = new DocumentClassNameRetriever(data, false).Retrieve();
            }

            if (String.IsNullOrEmpty(className))
            {
                // Transform object type back to class name and create object by class name
                className = TreeNodeProvider.GetClassName(settings.ObjectType);
            }

            return New<TreeNode>(className, settings.Data);
        }


        /// <summary>
        /// Creates new instance of an empty un-typed TreeNode.
        /// </summary>
        public static TreeNode New()
        {
            return New(null);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit TreeNode and fill it with given data.
        /// </summary>
        /// <param name="dataRow">Data row containing both tree node and coupled data</param>
        public static TreeNode New(DataRow dataRow)
        {
            return New(null, dataRow);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit TreeNode.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        public static TreeNode New(string className, TreeProvider treeProvider = null)
        {
            return New(className, null, treeProvider);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit TreeNode and fill it with given data.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled data</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        public static TreeNode New(string className, DataRow dataRow, TreeProvider treeProvider = null)
        {
            return New<TreeNode>(className, dataRow, treeProvider);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit specified node type and fill it with given data.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        public static NodeType New<NodeType>(string className)
            where NodeType : TreeNode, new()
        {
            return New<NodeType>(className, (DataRow)null);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit specified node type and fill it with given data.
        /// </summary>
        /// <param name="dataRow">Data row containing both tree node and coupled data</param>
        public static NodeType New<NodeType>(DataRow dataRow = null)
            where NodeType : TreeNode, new()
        {
            return New<NodeType>(null, dataRow);
        }


        /// <summary>
        /// Creates new instance of given class which must inherit specified node type and fill it with given data.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row containing both tree node and coupled data</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        public static NodeType New<NodeType>(string className, DataRow dataRow, TreeProvider treeProvider = null)
            where NodeType : TreeNode, new()
        {
            if (dataRow != null)
            {
                className = GetClassNameFromData(className, new DataRowContainer(dataRow));
            }

            var result = NewInstance<NodeType>(className);
            result.Initialize(className, dataRow, treeProvider);

            return result;
        }


        /// <summary>
        /// Creates new instance of given class which must inherit specified node type and fill it with given data.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="data">Data container containing both tree node and coupled data</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        public static NodeType New<NodeType>(string className, IDataContainer data, TreeProvider treeProvider = null)
            where NodeType : TreeNode, new()
        {
            if (data != null)
            {
                className = GetClassNameFromData(className, data);
            }

            var result = NewInstance<NodeType>(className);
            result.Initialize(className, data, treeProvider);

            return result;
        }


        private static string GetClassNameFromData(string className, IDataContainer data)
        {
            // Get class name from data - has priority over the incoming class name to keep data consistent with resulting TreeNode
            var dataClassName = new DocumentClassNameRetriever(data, false).Retrieve();
            if (dataClassName != null)
            {
                className = dataClassName;
            }

            return className;
        }


        /// <summary>
        /// Creates a new instance of the given type
        /// </summary>
        /// <param name="className">Class name</param>
        private static NodeType NewInstance<NodeType>(string className)
            where NodeType : TreeNode, new()
        {
            NodeType result;

            if (typeof(NodeType) == typeof(TreeNode))
            {
                result = DocumentGenerator.NewInstance<NodeType>(className);
            }
            else
            {
                result = new NodeType();

                // Validate the class name with the resulting TreeNode if explicitly set
                if ((className != null) && !string.Equals(className, result.ClassName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("Input class name '" + className + " is not consistent with the requested type '" + typeof(NodeType).FullName + "' which has class name '" + result.ClassName + "'. To fix this, use correct class name or null as the input parameter.");
                }
            }

            return result;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor, allowed only if Initialize is called immediately after it.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use method TreeNode.New.")]
        public TreeNode()
            : this(null)
        {
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        protected TreeNode(string className)
            : base(TYPEINFO)
        {
            if (!string.IsNullOrEmpty(className))
            {
                InitializeClassName(className);
            }
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing node from data row.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dr">Data row containing all tree node, document and coupled table</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        protected void Initialize(string className, DataRow dr, TreeProvider treeProvider)
        {
            if (!string.IsNullOrEmpty(className))
            {
                InitializeClassName(className);
            }

            TreeProvider = treeProvider;

            LoadData(new LoadDataSettings(dr));
        }


        /// <summary>
        /// Initializes the object created with default constructor. Use it to load existing node from data row.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="data">Data container containing all tree node, document and coupled table</param>
        /// <param name="treeProvider">Tree provider used to access data</param>
        protected void Initialize(string className, IDataContainer data, TreeProvider treeProvider)
        {
            if (!string.IsNullOrEmpty(className))
            {
                InitializeClassName(className);
            }

            TreeProvider = treeProvider;

            LoadData(new LoadDataSettings(data));
        }


        /// <summary>
        /// Sets the document default values.
        /// </summary>
        protected virtual void EnsureDefaultValues()
        {
            if (String.IsNullOrEmpty(DocumentName) && !IsRootNode())
            {
                // Get document type node name source
                string nodeNameSource = (DataClassInfo != null) ? DataClassInfo.ClassNodeNameSource : null;

                // Set document name
                DocumentName = !string.IsNullOrEmpty(nodeNameSource) ? ValidationHelper.GetString(GetValue(nodeNameSource), null) : "New page";
            }

            if (String.IsNullOrEmpty(DocumentCulture))
            {
                DocumentCulture = CultureHelper.GetPreferredCulture();
            }

            if (NodeIsContentOnly)
            {
                SetContentOnlyDefaultValues();
            }
        }


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override void LoadData(LoadDataSettings settings)
        {
            if ((settings == null) || (settings.Data == null))
            {
                return;
            }

            var data = settings.Data;

            // Load class name
            var className = new DocumentClassNameRetriever(data, false).Retrieve();
            if (!string.IsNullOrEmpty(className))
            {
                InitializeClassName(className);
            }

            base.LoadData(settings);
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public virtual void Dispose()
        {
            // Nothing in base class
        }

        #endregion


        #region "IHierarchicalDataContainer methods"

        /// <summary>
        /// Returns the type of given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override Type GetPropertyType(string propertyName)
        {
            // Try to get from base class
            Type result = base.GetPropertyType(propertyName);

            // Search by property name + ID (such as SiteDefaultStylesheet[ID])
            propertyName = propertyName + "ID";
            if (ContainsColumn(propertyName))
            {
                string bindingType = GetObjectTypeForColumn(propertyName);
                if (bindingType != null)
                {
                    // Get object
                    BaseInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                    if (bindingObj != null)
                    {
                        // Get the object type
                        result = bindingObj.GetType();
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Registers properties of this object.
        /// </summary>
        protected override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("DocumentName", m => m.GetDocumentName());

            // XML structures
            RegisterProperty("DocumentContent", m => m.DocumentContent);
            RegisterProperty("DocumentCustomData", m => m.DocumentCustomData);
            RegisterProperty("NodeCustomData", m => m.NodeCustomData);

            RegisterProperty("RelatedDocuments", m => m.RelatedDocuments);

            RegisterProperty<TreeNode>("Parent", m => m.Parent);
            RegisterProperty<UserInfo>("Owner", m => m.Owner);

            // Related objects
            RegisterProperty("Tags", m => m.Tags);
            RegisterProperty("Categories", m => m.Categories);

            RegisterProperty("AllAttachments", m => m.AllAttachments);
            RegisterProperty("Attachments", m => m.Attachments);
            RegisterProperty("GroupedAttachments", m => m.GroupedAttachments);

            RegisterProperty("Aliases", m => m.Aliases);

            // Workflow properties
            RegisterProperty<EmptyCollection<VersionHistoryInfo>>("VersionHistory", m => m.VersionHistory);
            RegisterProperty<EmptyCollection<WorkflowHistoryInfo>>("WorkflowHistory", m => m.WorkflowHistory);
            RegisterProperty<EmptyCollection<AttachmentHistoryInfo>>("AttachmentHistory", m => m.AttachmentHistory);
            RegisterProperty<WorkflowStepInfo>("WorkflowStep", m => m.WorkflowStep);
            RegisterProperty<DateTime>("WorkflowStepTimeout", m => m.WorkflowStepTimeout);

            RegisterProperty("MessageBoards", m => m.MessageBoards);
            RegisterProperty("Forums", m => m.Forums);

            RegisterProperty("Personalizations", m => m.Personalizations);

            // Related documents
            RegisterProperty("ChildObjects", m => m.GetObjectChildren());

            RegisterProperty("Children", m => m.Children);
            RegisterProperty("AllChildren", m => m.AllChildren);
            RegisterProperty("Links", m => m.Links);
            RegisterProperty("CultureVersions", m => m.CultureVersions);
            RegisterProperty("DocumentsOnPath", m => m.DocumentsOnPath);

            // Other document properties
            RegisterProperty<bool>("IsLink", m => m.IsLink);
            RegisterProperty<bool>("IsLastVersion", m => m.IsLastVersion);
            RegisterProperty("SiteID", m => m.NodeSiteID);
            RegisterProperty("SiteName", m => m.NodeSiteName);

            // Document URLs
            RegisterProperty("RelativeURL", m => m.RelativeURL);
            RegisterProperty("AbsoluteURL", m => m.AbsoluteURL);
            RegisterProperty("PermanentURL", m => m.PermanentURL);

            // E-commerce properties
            RegisterProperty<bool>("HasSKU", m => m.HasSKU);
        }


        /// <summary>
        /// Obtains value of specified property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value)
        {
            // Try to get from registered properties
            bool result = base.TryGetProperty(columnName, out value);
            if (result)
            {
                return true;
            }

            // Try to get from simple values
            result = TryGetValue(columnName, out value);
            if (result)
            {
                return true;
            }

            // Automatically evaluated bindings
            string autoColumn = columnName + "ID";
            if (ContainsColumn(autoColumn))
            {
                result = GetReferencedObject(autoColumn, out value);
                if (result)
                {
                    return true;
                }
            }

            // Check the connected document collections
            value = ConnectedDocuments[columnName];
            result = (value != null);
            if (result)
            {
                return true;
            }

            // Check the connected object collections
            value = ConnectedObjects[columnName];
            result = (value != null);
            if (result)
            {
                return true;
            }

            // Check the grouped attachments collections
            value = GroupedAttachments[columnName];
            result = (value != null);

            return result;
        }


        /// <summary>
        /// Obtains the object to which the particular column refers.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Result object</param>
        /// <returns>Returns true if the operation was successful (the object was present)</returns>
        protected bool GetReferencedObject(string columnName, out object value)
        {
            value = null;

            // Get the binding type
            string bindingType = GetObjectTypeForColumn(columnName);
            if (bindingType != null)
            {
                int bindingId = ValidationHelper.GetInteger(GetValue(columnName), 0);
                if (bindingId > 0)
                {
                    // Get parent type
                    GeneralizedInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                    if (bindingObj != null)
                    {
                        // Get parent object
                        value = bindingObj.GetObject(bindingId);
                    }
                }

                return true;
            }

            return false;
        }

        #endregion


        #region "IAdvancedDataContainer methods"

        /// <summary>
        /// Obtains value of given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Result value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            value = null;
            bool result = false;
            switch (columnName.ToLowerInvariant())
            {
                case "documentcontent":
                    // Returns the document content XML
                    value = DocumentContent.GetContentXml();
                    result = true;
                    break;

                case "published":
                    // Returns the published status
                    value = IsPublished;
                    result = true;
                    break;

                case "islastversion":
                    // Returns the status whether the version is last
                    value = IsLastVersion;
                    result = true;
                    break;

                case "sitename":
                    // Returns document site name if site is specified
                    value = NodeSiteName;
                    result = true;
                    break;

                case "classname":
                    // Returns document class name
                    value = NodeClassName;
                    result = true;
                    break;
            }

            // Try to get regular column values
            if (!result)
            {
                result = base.TryGetValue(columnName, out value);
            }

            // Ensure the null value
            value = DataHelper.GetNull(value);
            return result;
        }


        /// <summary>
        /// Sets value of the specified node column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the operation was successful</returns>
        public override bool SetValue(string columnName, object value)
        {
            bool result = base.SetValue(columnName, value);

            // Special columns treatment
            switch (columnName.ToLowerInvariant())
            {
                // Synchronize node ID to other columns
                case "nodeid":
                    {
                        if (!IsLink)
                        {
                            var nodeId = ValidationHelper.GetInteger(value, 0);
                            CultureData.DocumentNodeID = nodeId;
                            NodeData.NodeOriginalNodeID = nodeId;
                        }
                    }
                    break;

                // Synchronize node ID columns
                case "documentnodeid":
                    {
                        var nodeId = ValidationHelper.GetInteger(value, 0);
                        NodeData.NodeOriginalNodeID = nodeId;
                        if (!IsLink)
                        {
                            NodeData.NodeID = nodeId;
                        }
                    }
                    break;

                // DocumentContent - load content XML to the content object
                case "documentcontent":
                    DocumentContent.LoadContentXml(ValidationHelper.GetString(value, String.Empty));
                    break;

                case "documentworkflowstepid":
                    // Invalidate timeout value
                    mWorkflowStepTimeout = DateTime.MinValue;
                    break;

                case "nodeparentid":
                    // Clear parent property
                    if (ItemChanged("NodeParentID"))
                    {
                        mParent = null;
                    }
                    break;

                case "nodeclassid":
                    var className = DataClassInfoProvider.GetClassName(ValidationHelper.GetInteger(value, 0));

                    // Refresh class dependant properties if Class ID changed
                    var hasChanged = !string.Equals(mClassName, className, StringComparison.InvariantCultureIgnoreCase);
                    if (hasChanged)
                    {
                        // Update class name
                        mClassName = className;

                        // Refresh dependant properties
                        mDataClassInfo = null;
                        mCoupledData = null;
                        TypeInfo = TreeNodeProvider.GetTypeInfo(mClassName);
                    }
                    break;
            }

            // Invalidate step - can depend on any document property
            mWorkflowStep = null;

            if (result)
            {
                // Change status to changed document in case the document wasn't deleted
                if (ObjectStatus != ObjectStatusEnum.WasDeleted)
                {
                    ObjectStatus = ObjectStatusEnum.Changed;
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the integer value of the Tree node.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        /// <param name="allowZero">If false, only positive values are valid</param>
        /// <returns>Returns true if the value was changed</returns>
        public virtual bool SetIntegerValue(string columnName, int value, bool allowZero)
        {
            int oldValue = GetValue(columnName, 0);

            if (!allowZero && (value <= 0))
            {
                SetValue(columnName, null);
            }
            else
            {
                SetValue(columnName, value);
            }

            return (oldValue != value);
        }


        /// <summary>
        /// Returns true if the object contains specific column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            // Check base classes
            if (CultureData.ContainsColumn(columnName) || NodeData.ContainsColumn(columnName))
            {
                return true;
            }

            // Check couple class
            if (IsCoupled && CoupledData.ContainsColumn(columnName))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            if (!DocumentActionContext.CurrentResetChanges)
            {
                return;
            }

            base.ResetChanges();
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public override void MakeComplete(bool loadFromDb)
        {
            base.MakeComplete(loadFromDb);

            // Coupled DataRow
            if (IsCoupled && (CoupledData.Generalized.ObjectID <= 0))
            {
                EnsureCoupledDataReference();
                CoupledData.MakeComplete(loadFromDb);
            }
        }


        /// <summary>
        /// Returns true if the object has changed.
        /// </summary>
        public bool DataChanged()
        {
            // Exclude DocumentModifiedWhen
            return DataChanged("DocumentModifiedWhen");
        }

        #endregion


        #region "Partial update methods"

        /// <summary>
        /// Initializes class name
        /// </summary>
        /// <param name="className">Class name</param>
        private void InitializeClassName(string className)
        {
            NodeClassName = className;

            // Reset changes due to updated NodeClassID column
            NodeData.ResetChanges();
        }


        /// <summary>
        /// Refreshes node data class ID value based on data class info
        /// </summary>
        private void RefreshNodeDataClassId()
        {
            // Update class ID
            var classId = (DataClassInfo != null) ? DataClassInfo.ClassID : 0;
            NodeData.NodeClassID = classId;

            // Reset changes due to updated NodeClassID column
            NodeData.ResetChanges();
        }


        /// <summary>
        /// Ensures reference for the coupled data if not set
        /// </summary>
        private void EnsureCoupledDataReference()
        {
            if (CoupledData.Generalized.ObjectID > 0)
            {
                return;
            }

            var typeChanged = NodeData.ItemChanged("NodeClassID");
            if (typeChanged)
            {
                CoupledData.Generalized.ObjectID = 0;
                DocumentForeignKeyValue = 0;
            }
            else if (DocumentForeignKeyValue > 0)
            {
                CoupledData.Generalized.ObjectID = DocumentForeignKeyValue;
            }
            else
            {
                var idColumn = CoupledData.TypeInfo.IDColumn;
                var id = DocumentFieldsInfoProvider.GetDocumentFields(NodeClassName)
                                                   .Column(idColumn)
                                                   .WhereIn(idColumn, GetTranslatedCultureData().Column("DocumentForeignKeyValue")
                                                                                                .WhereEquals("DocumentCulture", DocumentCulture))
                                                   .GetScalarResult(0);

                CoupledData.Generalized.ObjectID = id;
                DocumentForeignKeyValue = id;
            }
        }


        /// <summary>
        /// Inserts the internal coupled data.
        /// </summary>
        private void InsertCoupledData()
        {
            // Insert the document data (Couple class) if present
            if (!IsCoupled)
            {
                return;
            }

            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                CoupledData.Insert();
            }

            // Synchronize the value
            DocumentForeignKeyValue = CoupledData.Generalized.ObjectID;
        }


        /// <summary>
        /// Updates the coupled data of the document node.
        /// </summary>
        protected virtual void UpdateCoupledData()
        {
            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                CoupledData.Update();
            }
        }


        /// <summary>
        /// Inserts the CMS_Tree part of the document node.
        /// </summary>
        /// <param name="parent">Parent node</param>
        protected virtual void InsertTreeNodeData(BaseInfo parent)
        {
            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                // Insert the base tree node
                NodeData.Insert(parent as DocumentNodeDataInfo);
            }

            // Update the Node ID within the instance (Ensures correct settings for the linked documents as well)
            NodeID = NodeData.NodeID;
        }


        /// <summary>
        /// Updates the CMS_Tree part of the document node.
        /// </summary>
        protected virtual void UpdateTreeNodeData()
        {
            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                // Update the Tree record
                NodeData.Update();
            }
        }


        /// <summary>
        /// Inserts the CMS_Document part of the document node.
        /// </summary>
        protected virtual void InsertCultureData()
        {
            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                // Insert the document
                CultureData.Insert();
            }
        }


        /// <summary>
        /// Updates the CMS_Document part of the document node.
        /// </summary>
        protected virtual void UpdateCultureData()
        {
            // Do not reset changes for partial classes
            // Time stamp is handled by TreeNode instance explicitly
            using (new CMSActionContext { ResetChanges = false, UpdateTimeStamp = false })
            {
                // Update the document
                CultureData.Update();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets list of culture codes to which is the document translated to.
        /// </summary>
        public IList<string> GetTranslatedCultures()
        {
            return GetTranslatedCultureData()
                .Column("DocumentCulture")
                .GetListResult<string>();
        }


        /// <summary>
        /// Indicates if document is translated to the given culture.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public bool IsTranslated(string cultureCode)
        {
            var count = GetTranslatedCultureData()
                .WhereEquals("DocumentCulture", cultureCode)
                .Count;

            return count > 0;
        }


        /// <summary>
        /// Gets culture data for all document culture versions
        /// </summary>
        internal ObjectQuery<DocumentCultureDataInfo> GetTranslatedCultureData()
        {
            // The value of DocumentNodeID in the where condition is NodeID or NodeLinkedNodeID.
            // Is not possible to use OriginalNodeID because it is initialized when inserting.
            return DocumentCultureDataInfoProvider.GetDocumentCultures()
                .WhereEquals("DocumentNodeID", GetOriginalNodeIDForInsert());
        }


        /// <summary>
        /// Sets the document name source field to the given value.
        /// </summary>
        /// <param name="value">Document name</param>
        public virtual void SetDocumentNameSource(string value)
        {
            SetDocumentNameSource(value, false);
        }


        /// <summary>
        /// Sets the document name source field to the given value.
        /// </summary>
        /// <param name="value">Document name</param>
        /// <param name="onlyIfEmpty">Indicates if value should only be set if the current value is empty</param>
        public virtual void SetDocumentNameSource(string value, bool onlyIfEmpty)
        {
            // There is no source for node name
            var nodeNameSource = DataClassInfo != null ? DataClassInfo.ClassNodeNameSource : null;
            if (string.IsNullOrEmpty(nodeNameSource))
            {
                return;
            }

            // Check source value
            var nodeNameValue = ValidationHelper.GetString(GetValue(nodeNameSource), null);
            if (onlyIfEmpty && !string.IsNullOrEmpty(nodeNameValue))
            {
                return;
            }

            // Do not synchronize field change
            using (new DocumentActionContext { SynchronizeFieldValues = false })
            {
                // Limit length to the maximal allowed
                SetValue(nodeNameSource, TreePathUtils.EnsureMaxNodeNameLength(value, NodeClassName));
            }
        }


        /// <summary>
        /// Maps the document name based on the document type settings.
        /// </summary>
        public virtual string MapDocumentName()
        {
            if ((DataClassInfo == null) || string.IsNullOrEmpty(DataClassInfo.ClassNodeNameSource))
            {
                return null;
            }

            string nodeNameSource = ValidationHelper.GetString(GetValue(DataClassInfo.ClassNodeNameSource), String.Empty);
            if (string.IsNullOrEmpty(nodeNameSource))
            {
                return null;
            }

            DocumentName = TreePathUtils.EnsureMaxNodeNameLength(nodeNameSource, NodeClassName);

            return DocumentName;
        }


        /// <summary>
        /// Hides the insert method without parameter. Do not use!
        /// </summary>
        public override void Insert()
        {
            throw new NotSupportedException("Unable to insert node without parent specification. Use Insert(TreeNode) instead.");
        }


        /// <summary>
        /// Inserts current node under specified parent node.
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="useDocumentHelper">Use document helper</param>
        /// <remarks>Use this method only for creating a new node that doesn't exist yet. The standard path properties of the node are not used during the insert operation and they are overwritten with new values.</remarks>
        public virtual void Insert(TreeNode parent, bool useDocumentHelper = true)
        {
            if (!isInDocumentHelper && useDocumentHelper)
            {
                // Insert using DocumentHelper
                lock (lockObject)
                {
                    try
                    {
                        isInDocumentHelper = true;

                        DocumentHelper.InsertDocument(this, parent, TreeProvider, false);
                    }
                    finally
                    {
                        isInDocumentHelper = false;
                    }
                }
            }
            else
            {
                // Insert standard way
                InsertInternal(parent);
            }
        }


        /// <summary>
        /// Inserts current node under specified parent node. The node is inserted as root if its class is root class.
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <remarks>Use this method only for creating a new node that doesn't exist yet. The standard path properties of the node are not used during the insert operation and they are overwritten with new values.</remarks>
        protected virtual void InsertInternal(TreeNode parent)
        {
            if (IsRootNode())
            {
                SetAsRoot();
            }
            else if (parent == null)
            {
                throw new Exception("The parent node is not specified.");
            }

            InsertNode(parent);
        }


        /// <summary>
        /// Inserts current node under specified parent node. The current node is inserted as root if parent node is null.
        /// </summary>
        /// <param name="parent">Parent node</param>
        private void InsertNode(TreeNode parent)
        {
            EnsureDefaultValues();

            var isRoot = parent == null;

            // Get site name
            var siteName = isRoot ? GetSiteName() : parent.GetSiteName();

            // Ensure node name
            if (!isRoot && string.IsNullOrWhiteSpace(NodeName))
            {
                NodeName = DocumentName;
                if (string.IsNullOrWhiteSpace(NodeName))
                {
                    throw new Exception("The DocumentName value is not specified.");
                }
            }

            // Check document culture
            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                throw new Exception("The DocumentCulture value is not specified.");
            }

            // Check site culture
            if (!CultureSiteInfoProvider.IsCultureAllowed(DocumentCulture, siteName))
            {
                throw new DocumentCultureNotAllowedException("Document culture '" + DocumentCulture + "' is not allowed on site '" + siteName + "'.");
            }

            using (var h = DocumentEvents.Insert.StartEvent(this, parent, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Clear flag to ensure consistency when document is copied or restored without children or linked documents
                    NodeHasChildren = false;
                    NodeHasLinks = false;

                    NodeID = 0;

                    if (!isRoot)
                    {
                        NodeParentID = parent.NodeID;
                        NodeSiteID = parent.NodeSiteID;

                        if (TreeProvider.GetUseParentNodeGroupID(siteName) && (NodeGroupID <= 0))
                        {
                            NodeGroupID = parent.NodeGroupID;
                        }

                        NodeName = TreeProvider.CheckUniqueNames ? TreePathUtils.GetUniqueNodeName(NodeName, parent.NodeID, 0, NodeClassName) : TreePathUtils.EnsureMaxNodeNameLength(NodeName, NodeClassName);

                        SetNodeLevel(parent.NodeData);
                        EnsureDefaultDataForInsert(parent);
                    }

                    // Use node name as a source for document name if not set
                    var name = String.IsNullOrWhiteSpace(DocumentName) ? NodeName : DocumentName;

                    // Update document name field
                    SetDocumentNameSource(DocumentName, true);

                    // Ensure maximal length
                    DocumentName = TreePathUtils.EnsureMaxNodeNameLength(name, NodeClassName);

                    if (TreeProvider.UpdateDocumentContent)
                    {
                        UpdateDocumentContent();
                    }

                    var ensureAlias = string.IsNullOrEmpty(NodeAlias);
                    var pathsUpdater = new DocumentPathsUpdater(this);

                    if (!isRoot)
                    {
                        pathsUpdater.UpdateDocumentPathsForInsert(parent, ensureAlias);
                    }

                    // Insert the node within transaction
                    using (var tr = new CMSTransactionScope())
                    {
                        var nodeData = isRoot ? NodeData : parent.NodeData;

                        InsertTreeNodeData(nodeData);
                        InsertCoupledData();
                        InsertCultureData();

                        if (!isRoot && ensureAlias && pathsUpdater.UpdateDocumentNodeAliasPathBasedOnIDForInsert(parent.NodeData))
                        {
                            UpdateTreeNodeData();
                        }

                        tr.Commit();
                    }

                    // Ensure initial order
                    EnsureInitialOrder(siteName);

                    // Raise notification event
                    SendNotifications("CREATEDOC");

                    new DocumentEventLogger(this).Log("CREATEDOC", ResHelper.GetString("TaskTitle.CreateDocument"));

                    ObjectStatus = ObjectStatusEnum.Unchanged;

                    // Reset the changes
                    ResetChanges();
                }

                // Drop the cache dependencies
                if (TouchCacheDependencies)
                {
                    ClearCache(siteName);
                }

                // Clear license limits tables
                LicenseHelper.ClearLicenseLimitation();

                // Clear the resolved class names (potentially changed)
                DocumentTypeHelper.ClearClassNames(true);

                // Finalize the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Sets current node properties to be root node.
        /// </summary>
        private void SetAsRoot()
        {
            NodeAliasPath = "/";
            NodeName = String.Empty;
            NodeAlias = String.Empty;
            NodeParentID = 0;
            NodeLevel = 0;
            NodeIsACLOwner = true;

            DocumentName = String.Empty;
            DocumentNamePath = "/";
            DocumentInheritsStylesheet = false;
            DocumentShowInSiteMap = false;
            DocumentMenuItemHideInNavigation = false;

            UpdateTimeStamps();
            UpdateUserStamps(true, true);
        }


        /// <summary>
        /// Inserts current node as new culture version of the same document.
        /// </summary>
        /// <param name="cultureCode">Culture code of new culture version (If not specified, node.DocumentCulture property is used.)</param>
        /// <param name="useDocumentHelper">If true, the document helper is used for the operation to handle workflow</param>
        public virtual void InsertAsNewCultureVersion(string cultureCode, bool useDocumentHelper = true)
        {
            if (!isInDocumentHelper && useDocumentHelper)
            {
                // Insert using DocumentHelper
                lock (lockObject)
                {
                    try
                    {
                        isInDocumentHelper = true;

                        DocumentHelper.InsertNewCultureVersion(this, TreeProvider, cultureCode, true, false);
                    }
                    finally
                    {
                        isInDocumentHelper = false;
                    }
                }
            }
            else
            {
                // Insert standard way
                InsertAsNewCultureVersionInternal(cultureCode);
            }
        }


        /// <summary>
        /// Inserts current node as a new culture version of the specified document.
        /// </summary>
        /// <param name="cultureCode">Culture code of new culture version (If not specified, node.DocumentCulture property is used.)</param>
        protected virtual void InsertAsNewCultureVersionInternal(string cultureCode)
        {
            // Ensure given document culture
            if (!string.IsNullOrEmpty(cultureCode))
            {
                DocumentCulture = cultureCode;
            }

            EnsureDefaultValues();

            // Check if the node is root
            bool isRoot = IsRootNode();

            // Check if NodeID set (if not, update cannot process)
            if (NodeID <= 0)
            {
                throw new Exception("The page node doesn't exist, you need to insert the page before inserting new culture versions.");
            }

            // Check if parent node didn't changed
            var originalParentId = GetOriginalValue("NodeParentID").ToInteger(0);
            if ((originalParentId > 0) && (originalParentId != NodeParentID))
            {
                throw new Exception("The new culture version cannot be under different parent node than original node");
            }

            // Check if node name is set
            if (string.IsNullOrWhiteSpace(NodeName))
            {
                NodeName = DocumentName;
                if (string.IsNullOrWhiteSpace(NodeName) && !isRoot)
                {
                    throw new Exception("The DocumentName value was not specified. Data cannot be saved.");
                }
            }

            // Check if document culture set
            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                throw new Exception("The DocumentCulture value was not specified. Data cannot be saved.");
            }

            var siteName = GetSiteName();

            // Check site culture
            if (!CultureSiteInfoProvider.IsCultureAllowed(DocumentCulture, siteName))
            {
                throw new DocumentCultureNotAllowedException("Document culture '" + DocumentCulture + "' is not allowed on site '" + siteName + "'.");
            }

            var pathsUpdater = new DocumentPathsUpdater(this);

            if (!isRoot)
            {
                UpdateNodeName();
                pathsUpdater.UpdateDocumentNodeAlias();
            }
            else
            {
                // Ensure consistent properties
                EnsureRootProperties();
            }

            // Handle the event
            using (var h = DocumentEvents.InsertNewCulture.StartEvent(this, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    EnsureDefaultDataForInsertNewCulture();
                    ResetTranslationFlag();
                    if (TreeProvider.UpdateDocumentContent)
                    {
                        UpdateDocumentContent();
                    }

                    if (!isRoot)
                    {
                        var parentData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(NodeParentID);
                        if (parentData == null)
                        {
                            throw new NullReferenceException("The parent node is not specified.");
                        }

                        // Update document name field
                        SetDocumentNameSource(DocumentName, true);

                        // Ensure maximal length
                        DocumentName = TreePathUtils.EnsureMaxNodeNameLength(DocumentName, NodeClassName);

                        SetNodeLevel(parentData);

                        pathsUpdater.UpdateDocumentPathsForNewCultureVersion(parentData);
                    }

                    // Insert the node within transaction
                    using (var tr = new CMSTransactionScope())
                    {
                        // Insert the document data (Couple class) if present
                        InsertCoupledData();

                        // Insert the document class
                        InsertCultureData();

                        // Update node data
                        UpdateTreeNodeData();

                        pathsUpdater.UpdateDescendantsPathsForNewCultureVersion();

                        // Commit transaction if necessary
                        tr.Commit();
                    }

                    // Raise notification event
                    SendNotifications("CREATEDOC");

                    new DocumentEventLogger(this).Log("NEWCULTUREDOC", ResHelper.GetString("TaskTitle.CreateCultureDocument"), false);

                    ObjectStatus = ObjectStatusEnum.Unchanged;

                    // Reset the changes
                    ResetChanges();
                }

                // Drop the cache dependencies
                if (TouchCacheDependencies)
                {
                    ClearCache(siteName);
                }

                // Clear license limits tables
                LicenseHelper.ClearLicenseLimitation();

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Clears 'Document is waiting for translation' flag.
        /// </summary>
        public void ResetTranslationFlag()
        {
            if (DocumentActionContext.CurrentResetIsWaitingForTranslationFlag)
            {
                DocumentIsWaitingForTranslation = false;
            }
        }


        /// <summary>
        /// Inserts current node under specified parent node as a document link.
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <remarks>Use this method only for creating a new node linked to existing document (the node must be existing document). The standard path properties of the node are not used during the insert operation and they are overwritten with new values.</remarks>
        /// <param name="newDocumentsOwner">Owner of the new documents</param>
        /// <param name="newDocumentsGroup">Group of the new documents</param>
        /// <param name="useDocumentHelper">If true, the document helper is used</param>
        public virtual void InsertAsLink(TreeNode parent, int newDocumentsOwner = 0, int newDocumentsGroup = 0, bool useDocumentHelper = true)
        {
            if (!isInDocumentHelper && useDocumentHelper)
            {
                // Insert using DocumentHelper
                lock (lockObject)
                {
                    try
                    {
                        isInDocumentHelper = true;

                        DocumentHelper.InsertDocumentAsLink(this, parent, TreeProvider, false, false, newDocumentsOwner, newDocumentsGroup);
                    }
                    finally
                    {
                        isInDocumentHelper = false;
                    }
                }
            }
            else
            {
                // Insert standard way
                InsertAsLinkInternal(parent, newDocumentsOwner, newDocumentsGroup);
            }
        }


        /// <summary>
        /// Inserts current node under specified parent node as a document link.
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="newDocumentsOwner">Owner of the new documents</param>
        /// <param name="newDocumentsGroup">Group of the new documents</param>
        /// <remarks>Use this method only for creating a new node linked to existing document (the node must be existing document). The standard path properties of the node are not used during the insert operation and they are overwritten with new values.</remarks>
        protected virtual void InsertAsLinkInternal(TreeNode parent, int newDocumentsOwner, int newDocumentsGroup)
        {
            if (parent == null)
            {
                throw new NullReferenceException("The parent node is not specified.");
            }

            // Get parent site name
            var siteName = parent.GetSiteName();

            // Check if node name or document name is set
            if (string.IsNullOrWhiteSpace(NodeName) && string.IsNullOrWhiteSpace(DocumentName))
            {
                throw new Exception("The DocumentName value is not specified.");
            }

            // Check if node alias set
            if (string.IsNullOrWhiteSpace(NodeAlias))
            {
                throw new Exception("The NodeAlias value is not specified");
            }

            // Parent is the same instance, clone parent to not to influence the properties
            if (parent == this)
            {
                parent = parent.Clone();
            }

            // Perform changes on a clone since the data is cached
            var originalNodeId = NodeData.NodeID;
            NodeData = NodeData.Clone();
            NodeData.NodeID = 0;

            // Ensure node name
            if (string.IsNullOrWhiteSpace(NodeName))
            {
                NodeName = DocumentName;
            }

            // Handle the event
            using (var h = DocumentEvents.InsertLink.StartEvent(this, parent, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Set the linked node ID if not already set (if set, keep the original link) MUST BE BEFORE THE NodeSiteID IS CHANGED
                    if (NodeLinkedNodeID <= 0)
                    {
                        NodeLinkedNodeID = originalNodeId;
                        NodeLinkedNodeSiteID = NodeSiteID;
                    }

                    // Check site culture - at least one of the linked document cultures should be allowed on the target.
                    var anyAllowedCulture = GetTranslatedCultures().Any(culture => CultureSiteInfoProvider.IsCultureAllowed(culture, siteName));
                    if (!anyAllowedCulture)
                    {
                        throw new DocumentCultureNotAllowedException("Linked document does not have any culture which is allowed on site '" + siteName + "'.");
                    }

                    // Set inherited information from parent
                    NodeParentID = parent.NodeID;
                    NodeSiteID = parent.NodeSiteID;

                    TreeProvider.SetInheritedACL(this, parent.NodeACLID);

                    // Link doesn't have any children
                    NodeHasChildren = false;

                    // Linked node doesn't have any links (only original should have set this flag)
                    NodeHasLinks = false;

                    // Set NodeGroupID according to the parent value
                    if (TreeProvider.GetUseParentNodeGroupID(siteName))
                    {
                        NodeGroupID = parent.NodeGroupID;
                    }

                    // Create new GUID
                    if (TreeProvider.GenerateNewGuid)
                    {
                        NodeGUID = Guid.NewGuid();
                    }

                    // Update owner
                    if (newDocumentsOwner > 0)
                    {
                        NodeOwner = newDocumentsOwner;
                    }
                    else
                    {
                        if (TreeProvider.UserInfo.UserID != 0)
                        {
                            if ((NodeOwner <= 0) || TreeProvider.UpdateUser)
                            {
                                NodeOwner = TreeProvider.UserInfo.UserID;
                            }
                        }
                    }

                    // Update group
                    if (newDocumentsGroup > 0)
                    {
                        NodeGroupID = newDocumentsGroup;
                    }

                    new DocumentPathsUpdater(this).UpdateDocumentPathsForLink(parent.NodeData);

                    SetNodeLevel(parent.NodeData);

                    if (TreeProvider.UpdateDocumentContent)
                    {
                        UpdateDocumentContent();
                    }

                    // Insert the node within transaction
                    using (var tr = new CMSTransactionScope())
                    {
                        // Insert the link record
                        InsertTreeNodeData(parent.NodeData);

                        // Commit transaction if necessary
                        tr.Commit();
                    }

                    // If the link was not created from original document, get the language data
                    if (DocumentID <= 0)
                    {
                        LoadCultureDataFromOriginal();
                    }

                    // Ensure initial order
                    EnsureInitialOrder(siteName);

                    // Raise notification event
                    SendNotifications("CREATEDOC");

                    new DocumentEventLogger(this).Log("LINKDOC", ResHelper.GetString("TaskTitle.CreateLinkedDocument"));

                    ObjectStatus = ObjectStatusEnum.Unchanged;

                    // Reset the changes
                    ResetChanges();
                }

                // Drop the cache dependencies
                if (TouchCacheDependencies)
                {
                    ClearCache(siteName);
                }

                // Clear license limits tables
                LicenseHelper.ClearLicenseLimitation();

                // Finish the event
                h.FinishEvent();
            }
        }


        private void LoadCultureDataFromOriginal()
        {
            var originalQuery =
                DocumentHelper.GetDocuments()
                    .WhereEquals("NodeID", NodeLinkedNodeID)
                    .CombineWithAnyCulture();

            if (!String.IsNullOrEmpty(DocumentCulture))
            {
                originalQuery.Culture(DocumentCulture);
            }

            var original = originalQuery.FirstOrDefault();
            if (original != null)
            {
                CultureData = original.CultureData.Clone();
                if (original.CoupledData != null)
                {
                    CoupledData = original.CoupledData.Clone();
                }
            }
        }


        /// <summary>
        /// Updates node data in the database.
        /// </summary>
        public override void Update()
        {
            Update(true);
        }


        /// <summary>
        /// Updates node data in the database.
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used for the operation to handle workflow</param>
        public virtual void Update(bool useDocumentHelper)
        {
            if (IsLastVersion && !isInDocumentHelper && useDocumentHelper)
            {
                // Update using DocumentHelper
                lock (lockObject)
                {
                    try
                    {
                        isInDocumentHelper = true;

                        DocumentHelper.UpdateDocument(this, TreeProvider);
                    }
                    finally
                    {
                        isInDocumentHelper = false;
                    }
                }
            }
            else
            {
                // Update standard way
                UpdateInternal();
            }
        }


        /// <summary>
        /// Updates node data in the database.
        /// </summary>
        protected virtual void UpdateInternal()
        {
            // Check if the node is root
            bool isRoot = IsRootNode();

            // Check if NodeID set (if not, update cannot process)
            if ((NodeID <= 0) || (DocumentID <= 0))
            {
                NodeName = DocumentName;
                if (string.IsNullOrWhiteSpace(NodeName) && !isRoot)
                {
                    throw new Exception("The page record doesn't exist, you need to insert the page before updating.");
                }
            }

            // Check if document culture set
            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                throw new Exception("The DocumentCulture value was not specified. Data cannot be saved.");
            }

            // Changing link to a different original document directly is not supported
            if (TreeProvider.CheckLinkConsistency)
            {
                var originalLinkedNodeId = GetOriginalValue("NodeLinkedNodeID").ToInteger(0);
                if ((originalLinkedNodeId > 0) && (originalLinkedNodeId != NodeLinkedNodeID))
                {
                    throw new NotSupportedException("Original document of a linked document cannot be changed.");
                }
            }

            // Get node site name
            var siteName = GetSiteName();
            var pathsUpdater = new DocumentPathsUpdater(this);
            var originalParentId = GetOriginalValue("NodeParentID").ToInteger(0);

            if (!isRoot)
            {
                UpdateNodeName();
                CheckIfNodeNameIsSet(originalParentId);
                pathsUpdater.UpdateDocumentNodeAlias();
            }
            else
            {
                // Ensure consistent properties
                EnsureRootProperties();
            }

            // Handle the event
            using (var h = DocumentEvents.Update.StartEvent(this, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Do not use ItemChanged check for parent change (there can be a false positive when importing package from version prior to 9 (0 value VS null value)
                    var parentChanged = originalParentId != NodeParentID;

                    // Ensure maximal length
                    DocumentName = TreePathUtils.EnsureMaxNodeNameLength(DocumentName, NodeClassName);

                    // Check site culture for the parent / new parent to verify that the document can be present on the site in the given culture
                    var parentSiteName = siteName;
                    if (parentChanged)
                    {
                        var parent = Parent;
                        if (parent != null)
                        {
                            parentSiteName = parent.NodeSiteName;
                        }
                    }

                    if (!CultureSiteInfoProvider.IsCultureAllowed(DocumentCulture, parentSiteName))
                    {
                        throw new DocumentCultureNotAllowedException("Document culture '" + DocumentCulture + "' is not allowed on site '" + parentSiteName + "'.");
                    }

                    EnsureDefaultDataForUpdate();
                    if (TreeProvider.UpdateDocumentContent)
                    {
                        UpdateDocumentContent();
                    }

                    if (isRoot)
                    {
                        EnsureRootProperties();
                    }
                    else
                    {
                        var parentData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(NodeParentID);
                        if (parentData == null)
                        {
                            throw new NullReferenceException("Parent node does not exist.");
                        }

                        SetNodeLevel(parentData);
                        NodeSiteID = parentData.NodeSiteID;

                        siteName = GetSiteName();

                        pathsUpdater.UpdateDocumentPathsForUpdate(parentData);

                        EnsureWildcardValues();
                    }

                    // Check if the site changed
                    var siteChanged = ItemChanged("NodeSiteID");
                    if (siteChanged && TreeProvider.GenerateNewGuid)
                    {
                        // Ensure unique GUIDs
                        NodeGUID = Guid.NewGuid();
                        DocumentGUID = Guid.NewGuid();
                    }

                    using (var hInner = DocumentEvents.UpdateInner.StartEvent(this, TreeProvider))
                    {
                        hInner.DontSupportCancel();

                        // Insert the node within transaction - keep it short as possible
                        using (var tr = new CMSTransactionScope())
                        {
                            // Update the base tree node
                            UpdateTreeNodeData();

                            // Update the document data (Couple class) if present
                            if (IsCoupled)
                            {
                                EnsureCoupledDataReference();
                                if (CoupledData.Generalized.ObjectID > 0)
                                {
                                    UpdateCoupledData();
                                }
                                else
                                {
                                    InsertCoupledData();
                                }
                            }

                            // Update the document class
                            UpdateCultureData();

                            pathsUpdater.UpdateOtherCultureVersionsAndDescendantsPathsForUpdate();

                            // Commit transaction if necessary
                            tr.Commit();
                        }

                        hInner.FinishEvent();
                    }

                    // Ensures correct site of the links if site changes
                    if (siteChanged)
                    {
                        EnsureLinksSite();
                    }


                    if (parentChanged)
                    {
                        // Make sure document is placed on the right position in the new parent
                        EnsureInitialOrder(NodeSiteName);

                        // Move related objects to the target site
                        if (siteChanged)
                        {
                            TreeProvider.ChangeRelatedObjectsSite(this, NodeSiteID);
                        }

                        // Handle document permissions
                        TreeProvider.HandleMoveNodePermissions(this);
                    }

                    var originalNodeAliasPath = GetOriginalValue("NodeAliasPath").ToString(String.Empty);
                    bool nodeAliasPathChanged = !string.Equals(NodeAliasPath, originalNodeAliasPath, StringComparison.InvariantCulture);
                    if (TouchCacheDependencies && nodeAliasPathChanged)
                    {
                        // Drop the original cache dependencies
                        CacheHelper.TouchKeys(DocumentDependencyCacheKeysBuilder.GetPathDependencyCacheKeys(siteName, originalNodeAliasPath, DocumentCulture));
                    }

                    // Send notifications
                    SendNotifications("UPDATEDOC");

                    new DocumentEventLogger(this).Log("UPDATEDOC", ResHelper.GetString("TaskTitle.UpdateDocument"));

                    ObjectStatus = ObjectStatusEnum.Unchanged;

                    // Reset node changes
                    ResetChanges();

                    // Drop the cache dependencies
                    if (TouchCacheDependencies)
                    {
                        ClearCache(siteName);
                    }
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        private void CheckIfNodeNameIsSet(int originalParentId)
        {
            // Check if node name is set (only root can have an empty string as the node name)
            if ((originalParentId > 0) && string.IsNullOrWhiteSpace(NodeName))
            {
                throw new Exception("The NodeName value was not specified. Data cannot be saved.");
            }
        }


        /// <summary>
        /// Submits the changes in the object to the database.
        /// </summary>
        /// <param name="withCollections">If true, also submits the changes in the children of the object</param>
        public override void SubmitChanges(bool withCollections)
        {
            SubmitChanges(withCollections, Parent);
        }


        /// <summary>
        /// Submits the changes in the object to the database.
        /// </summary>
        /// <param name="withCollections">If true, also submits the changes in the children of the object</param>
        /// <param name="parentDocument">Parent document, needed for new documents</param>
        public virtual void SubmitChanges(bool withCollections, TreeNode parentDocument)
        {
            using (var tr = new CMSTransactionScope())
            {
                switch (ObjectStatus)
                {
                    case ObjectStatusEnum.ToBeDeleted:
                        // Delete the object
                        Delete();
                        break;

                    case ObjectStatusEnum.New:
                        // Insert new document
                        if (IsLink)
                        {
                            InsertAsLink(parentDocument);
                        }
                        else
                        {
                            Insert(parentDocument);
                        }
                        break;

                    case ObjectStatusEnum.Changed:
                        // Save the object to the database
                        if (IsLink)
                        {
                            if (IsTranslated(DocumentCulture))
                            {
                                Update();
                            }
                            else
                            {
                                InsertAsNewCultureVersionWithMoveSupport();
                            }
                        }
                        else
                        {
                            if (DocumentID > 0)
                            {
                                Update();
                            }
                            else
                            {
                                InsertAsNewCultureVersionWithMoveSupport();
                            }
                        }
                        break;

                    case ObjectStatusEnum.Unchanged:
                        // Do nothing
                        break;

                    default:
                        throw new NotImplementedException("Unknown status.");
                }

                // Submit the changes in the children
                if (withCollections)
                {
                    // Submit the children
                    if (Children != null)
                    {
                        Children.SubmitChanges();
                    }

                    // Submit the attachments
                    if (Attachments != null)
                    {
                        Attachments.SubmitChanges();
                    }
                }

                // Commit the transaction
                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes all the culture versions of the specified node including the child nodes.
        /// </summary>
        public virtual void DeleteAllCultures()
        {
            // Delete the child nodes first to avoid their cut offs
            DeleteChildNodes();

            // Delete all culture versions, the default culture as last one
            foreach (var culture in TreeProvider.EnumerateCultureVersions(this, null, true))
            {
                culture.Delete();
            }

            // Clear license limits tables
            LicenseHelper.ClearLicenseLimitation();
        }


        /// <summary>
        /// Destroys the document with its version history. Destroys only this culture version of the document.
        /// </summary>
        public override bool Destroy()
        {
            return Delete(false, true);
        }


        /// <summary>
        /// Deletes the document to the recycle bin. Deletes only this culture version of the document to the recycle bin, including the bound product.
        /// </summary>
        public override bool Delete()
        {
            return Delete(true);
        }


        /// <summary>
        /// Deletes the document to the recycle bin. Deletes only this culture version of the document to the recycle bin, including the bound product.
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used</param>
        public virtual bool Delete(bool useDocumentHelper)
        {
            if (!isInDocumentHelper && useDocumentHelper)
            {
                // Delete using DocumentHelper
                lock (lockObject)
                {
                    bool result;

                    try
                    {
                        isInDocumentHelper = true;

                        result = Delete(false, false);
                    }
                    finally
                    {
                        isInDocumentHelper = false;
                    }

                    return result;
                }
            }
            else
            {
                // Delete standard way
                var settings = new DeleteDocumentSettings(this, false, false, TreeProvider);
                return DeleteInternal(settings);
            }
        }


        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <param name="deleteAllCultures">Indicates if all culture versions of the specified document should be deleted</param>
        /// <param name="destroyHistory">Indicates if document history should be deleted as well</param>
        public virtual bool Delete(bool deleteAllCultures, bool destroyHistory)
        {
            return DocumentHelper.DeleteDocument(this, TreeProvider, deleteAllCultures, destroyHistory);
        }


        /// <summary>
        /// Deletes single culture version of a node from the database. If node is last node culture version, deletes also the child nodes of all culture versions.
        /// </summary>
        /// <param name="settings">Delete document settings. Please note settings for alternating document, version history and culture versions are not applied since this functionality is not supproted in this context.</param>
        /// <remarks>If deleted node is the root node it will not be deleted. Only child nodes will be deleted and no exception will be thrown.</remarks>
        /// <returns>Returns true if last culture (Tree record) has been deleted</returns>
        internal virtual bool DeleteInternal(DeleteDocumentSettings settings)
        {
            LicenseHelper.ClearLicenseLimitation();

            var siteName = GetSiteName();
            var isRoot = IsRootNode();
            var isLink = IsLink;
            var lastCulture = isLink || (GetTranslatedCultureData().Count == 1);
            var allowRootDeletion = settings.AllowRootDeletion;

            // Handle the event
            using (var h = DocumentEvents.Delete.StartEvent(this, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Remove dependencies for each culture version of standard document
                    if (!isLink && (!isRoot || !lastCulture || allowRootDeletion))
                    {
                        RemoveDocumentDependencies(settings);
                    }

                    // Delete the node within transaction
                    using (var tr = new CMSTransactionScope())
                    {
                        // If last culture, delete the child nodes of all culture versions
                        if (lastCulture)
                        {
                            if (!isLink)
                            {
                                // Delete the linked documents. Links always lead to the original document, so there is no need to delete links for link.
                                TreeProvider.DeleteLinks(this);
                            }

                            if (settings.DeleteChildNodes)
                            {
                                DeleteChildNodes();
                            }
                        }

                        // Do not delete last culture version of a root document
                        if (!isRoot || !lastCulture || allowRootDeletion)
                        {
                            // Delete the data if not link
                            if (!isLink)
                            {
                                if (!lastCulture)
                                {
                                    new DocumentPathsUpdater(this).UpdateDescendantsPathsForDelete();
                                }

                                // Delete the document data (Coupled class) if present
                                if (IsCoupled)
                                {
                                    CoupledData.Delete();
                                }

                                // Delete the document
                                CultureData.Delete();
                            }

                            if (lastCulture)
                            {
                                // Delete the base tree node
                                NodeData.Delete();
                            }
                        }

                        // Commit transaction if necessary
                        tr.Commit();
                    }

                    if (!isRoot || !lastCulture || allowRootDeletion)
                    {
                        // Send notifications
                        SendNotifications("DELETEDOC");

                        new DocumentEventLogger(this).Log(settings.DestroyHistory ? "DESTROYDOC" : "DELETEDOC", settings.DestroyHistory ? ResHelper.GetString("TaskTitle.DestroyDocument") : ResHelper.GetString("TaskTitle.DeleteDocument"), false);

                        ObjectStatus = ObjectStatusEnum.WasDeleted;
                    }
                }

                // Drop the cache dependencies
                if (TouchCacheDependencies)
                {
                    ClearCache(siteName);
                }

                LicenseHelper.ClearLicenseLimitation();

                // Finish the event
                h.FinishEvent();
            }

            return lastCulture;
        }


        /// <summary>
        /// Removes document dependencies.
        /// </summary>
        /// <param name="settings">Delete document settings</param>
        protected virtual void RemoveDocumentDependencies(DeleteDocumentSettings settings)
        {
            // Remove document tags
            DocumentTagInfoProvider.RemoveTags(DocumentID);

            // Remove document from all categories
            DocumentCategoryInfoProvider.RemoveDocumentFromCategories(DocumentID);

            // Remove scheduled tasks
            TaskInfoProvider.DeleteObjectsTasks(OBJECT_TYPE, new List<int> { DocumentID });

            // Remove personalization dependencies
            RemovePersonalizationDependencies();

            // Delete event attendees
            if (string.Equals(NodeClassName, "cms.bookingevent", StringComparison.InvariantCultureIgnoreCase))
            {
                DeleteEventAttendees();
            }
        }


        /// <summary>
        /// Changes the document node to the link to another document.
        /// </summary>
        /// <param name="linkedNodeId">Node to link</param>
        internal virtual void ChangeToLink(int linkedNodeId)
        {
            // Get the original document
            TreeProvider.PreferredCultureCode = DocumentCulture;
            var original = TreeProvider.SelectSingleNode(linkedNodeId, TreeProvider.ALL_CULTURES);
            if (original == null)
            {
                throw new NullReferenceException("Could not find the original document.");
            }

            // For standard document check matching class names
            bool linked = IsLink;
            if (!linked && !string.Equals(original.NodeClassName, NodeClassName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Node class name does not match the target class name.");
            }

            // Changing other document properties is not a supported scenario
            if (DataChanged(null))
            {
                throw new NotSupportedException("Properties of a document cannot be changed when converting to a linked document.");
            }

            using (var h = DocumentEvents.ChangeToLink.StartEvent(this, TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Insert the node within transaction
                    using (var tr = new CMSTransactionScope())
                    {
                        // Remove document data since link is just node record
                        if (!linked)
                        {
                            foreach (var culture in TreeProvider.EnumerateCultureVersions(this))
                            {
                                if (culture == null)
                                {
                                    continue;
                                }

                                var settings = new DeleteDocumentSettings(culture, false, false, TreeProvider);
                                culture.RemoveDocumentDependencies(settings);

                                // Delete coupled data
                                if (culture.IsCoupled)
                                {
                                    culture.CoupledData.Delete();
                                }

                                // Delete document data
                                culture.CultureData.Delete();
                            }
                        }

                        // Change document to link
                        NodeLinkedNodeID = original.NodeID;
                        NodeLinkedNodeSiteID = original.NodeSiteID;

                        // Propagate data
                        NodeSKUID = original.NodeSKUID;
                        NodeName = original.NodeName;

                        // Update the data
                        UpdateTreeNodeData();

                        // Load new data for the changed document (classes match)
                        if (!linked)
                        {
                            // Transfer the document and coupled data from linked node
                            foreach (string column in CultureData.ColumnNames)
                            {
                                SetValue(column, original.GetValue(column));
                            }

                            if (IsCoupled)
                            {
                                foreach (string column in CoupledData.ColumnNames)
                                {
                                    SetValue(column, original.GetValue(column));
                                }
                            }
                        }

                        // Commit transaction if necessary
                        tr.Commit();
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Ensures changing the underlying database structure of a given document to its document type. 
        /// Changes only the published data of the document type (CMS_Document and coupled data). Fills missing values with the data from given document node.
        /// </summary>
        /// <param name="node">Document node to change with new data</param>
        /// <param name="originalClassName">Original class name of the document</param>
        public static void ChangeNodeDocumentType(TreeNode node, string originalClassName)
        {
            // Validate new class
            string className = node.NodeClassName;
            var dci = node.DataClassInfo;
            if (dci == null)
            {
                return;
            }

            using (var h = DocumentEvents.ChangeDocumentType.StartEvent(node, node.TreeProvider))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    using (var tr = new CMSTransactionScope())
                    {
                        TreeProvider tree = new TreeProvider();
                        TreeNode lastNode = null;

                        // Get the original node
                        var siteName = node.GetSiteName();
                        var nodes = tree.SelectNodes(siteName, node.NodeAliasPath, TreeProvider.ALL_CULTURES, false, originalClassName);
                        if (!DataHelper.DataSourceIsEmpty(nodes))
                        {
                            foreach (var dbNode in nodes)
                            {
                                // If already correct document type, do not convert
                                if (string.Equals(dbNode.NodeClassName, className, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    return;
                                }

                                // Get original coupled class
                                var originalCoupledData = dbNode.CoupledData;
                                if (originalCoupledData != null)
                                {
                                    // Ensure the correct ID (coupled class is not loaded)
                                    originalCoupledData.SetValue(originalCoupledData.TypeInfo.IDColumn, dbNode.DocumentForeignKeyValue);
                                }

                                int newForeignKeyValue = 0;

                                if (dci.ClassIsCoupledClass)
                                {
                                    // Create new couple class
                                    var newCoupledData = DocumentFieldsInfo.New(className, originalCoupledData);

                                    // Ensure empty values in the new coupled DataClass
                                    foreach (string col in newCoupledData.ColumnNames)
                                    {
                                        if ((newCoupledData.GetValue(col) == null) && node.CoupledData.ContainsColumn(col))
                                        {
                                            newCoupledData.SetValue(col, node.CoupledData.GetValue(col));
                                        }
                                    }
                                    newCoupledData.Insert();

                                    dbNode.CoupledData = newCoupledData;
                                    newForeignKeyValue = newCoupledData.Generalized.ObjectID;
                                }
                                else
                                {
                                    dbNode.CoupledData = null;
                                }

                                dbNode.DocumentForeignKeyValue = newForeignKeyValue;

                                // Change the foreign key value of the given node to match the new coupled class
                                if (string.Equals(dbNode.DocumentCulture, node.DocumentCulture, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    node.DocumentForeignKeyValue = newForeignKeyValue;
                                    if (node.DocumentForeignKeyValue > 0)
                                    {
                                        node.CoupledData.SetValue(node.CoupledData.TypeInfo.IDColumn, newForeignKeyValue);
                                    }
                                }

                                // Delete original couple class
                                if (originalCoupledData != null)
                                {
                                    originalCoupledData.Delete();
                                }

                                // Save the document class to update the link to coupled data
                                dbNode.UpdateCultureData();

                                lastNode = dbNode;
                            }

                            // Update the class ID in the main node
                            if (lastNode != null)
                            {
                                lastNode.SetValue("NodeClassID", dci.ClassID);
                                lastNode.UpdateTreeNodeData();
                            }
                        }

                        // Update all linked documents with new class ID
                        DocumentNodeDataInfoProvider.BulkUpdateData(
                            new WhereCondition()
                                .WhereEquals("NodeID", node.OriginalNodeID)
                                .Or()
                                .WhereEquals("NodeLinkedNodeID", node.OriginalNodeID),
                            new Dictionary<string, object> {
                                { "NodeClassID", dci.ClassID }
                            }
                        );

                        // Clear the previous document history
                        VersionManager vm = VersionManager.GetInstance(node.TreeProvider);
                        vm.ClearDocumentHistory(node.DocumentID);

                        tr.Commit();
                    }

                    // Reset node changes
                    node.ResetChanges();

                    // Clear the cache
                    node.ClearOutputCache(true, true);
                    node.ClearCache();
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Returns a clone of the node.
        /// </summary>
        public virtual NewNodeType Clone<NewNodeType>() where NewNodeType : TreeNode, new()
        {
            return New<NewNodeType>(NodeClassName, this, TreeProvider);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            var path = NodeAliasPath;
            if (String.IsNullOrEmpty(path))
            {
                // Ensure alias path if empty
                path = NodeData.GetAliasPath();
            }

            return
                new WhereCondition()
                    .WhereEquals("NodeSiteID", NodeSiteID)
                    .Where(w => w
                        .Where(w1 => w1
                            // GUID lookup
                            .WhereEquals("DocumentGUID", DocumentGUID)
                            // Add condition for node GUID to get correct node when link or original document is retrieved
                            .WhereEquals("NodeGUID", NodeGUID)
                        )
                        .Or()
                        .Where(w2 => w2
                            // "Code name" lookup
                            .WhereEquals("NodeAliasPath", path)
                            .WhereEquals("DocumentCulture", DocumentCulture)
                        )
                    );
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected override BaseInfo GetExisting()
        {
            var existing = base.GetExisting() as TreeNode;

            // Propagate tree provider instance together with its configuration
            if (existing != null)
            {
                existing.TreeProvider = TreeProvider;
            }

            return existing;
        }


        /// <summary>
        /// Copies the node data to the destination node according to the settings.
        /// </summary>
        /// <param name="destNode">Destination node</param>
        /// <param name="settings">Copy settings</param> 
        public virtual void CopyDataTo(TreeNode destNode, CopyNodeDataSettings settings)
        {
            // Abort the copying if the destination node is not specified
            if (destNode == null)
            {
                return;
            }

            // Get the class record
            FormInfo fi = null;

            // Copy the Tree data (whole tree is not versioned)
            if (settings.CopyTreeData && settings.CopyNonVersionedData)
            {
                // Linked node ID first
                destNode.NodeLinkedNodeID = NodeLinkedNodeID;
                destNode.NodeLinkedNodeSiteID = NodeLinkedNodeSiteID;

                var nodeColumns = ObjectTypeManager.GetColumnNames(DocumentNodeDataInfo.OBJECT_TYPE);
                foreach (string col in nodeColumns)
                {
                    // Do not copy system data if not enabled
                    bool copy = true;
                    bool isSystem = VersionManager.IsSystemNodeColumn(col);
                    if (!settings.CopySystemTreeData && isSystem)
                    {
                        copy = false;
                    }

                    // Copy the data
                    if (copy && destNode.ContainsColumn(col) && !settings.ExcludeColumns.Contains(col))
                    {
                        destNode.SetValue(col, GetValue(col));
                    }
                }

                // Update original values and reset changes, treat copied data as original data
                if (settings.ResetChanges)
                {
                    destNode.NodeData.ResetChanges();
                }
            }

            // Copy the Document data
            if (settings.CopyDocumentData)
            {
                var docColumns = ObjectTypeManager.GetColumnNames(DocumentCultureDataInfo.OBJECT_TYPE);
                foreach (string col in docColumns)
                {
                    // Check excluded columns (not data columns)
                    bool isVersioned = VersionManager.IsVersionedDocumentColumn(col);
                    bool copy = (settings.CopyVersionedData && isVersioned) || (settings.CopyNonVersionedData && !isVersioned);

                    // Do not copy system data if not enabled
                    bool isSystem = VersionManager.IsSystemDocumentColumn(col);
                    if (!settings.CopySystemDocumentData && isSystem)
                    {
                        copy = false;
                    }

                    // Copy the data
                    if (copy && destNode.ContainsColumn(col) && !settings.ExcludeColumns.Contains(col))
                    {
                        destNode.SetValue(col, GetValue(col));
                    }
                }

                // Update original values and reset changes, treat copied data as original data
                if (settings.ResetChanges)
                {
                    destNode.CultureData.ResetChanges();
                }
            }

            // Copy the Coupled data if present (whole coupled class is versioned)
            if (settings.CopyCoupledData && IsCoupled)
            {
                if (NodeClassName == null)
                {
                    throw new Exception("Class name is not specified.");
                }


                int colIndex = 0;
                var coupledColumns = ObjectTypeManager.GetColumnNames(DocumentFieldsInfoProvider.GetObjectType(NodeClassName));
                foreach (string col in coupledColumns)
                {
                    bool isVersioned = ((colIndex > 0) && VersionManager.IsVersionedCoupledColumn(NodeClassName, col));

                    // Copy the data
                    if ((!isVersioned && settings.CopyNonVersionedData) || (isVersioned && settings.CopyVersionedData))
                    {
                        if (destNode.ContainsColumn(col) && !settings.ExcludeColumns.Contains(col))
                        {
                            bool setValue = true;
                            var val = GetValue(col);
                            if (val == null)
                            {
                                if (fi == null)
                                {
                                    fi = FormHelper.GetFormInfo(NodeClassName, false);
                                }

                                // Get field info
                                FormFieldInfo ffi = fi.GetFormField(col);

                                // Set empty value only if allowed
                                setValue = (ffi == null) || ffi.AllowEmpty;
                            }

                            if (setValue)
                            {
                                destNode.SetValue(col, val);
                            }
                        }
                    }

                    colIndex += 1;
                }

                // Update original values and reset changes, treat copied data as original data
                if (settings.ResetChanges)
                {
                    destNode.CoupledData.ResetChanges();
                }
            }
        }


        /// <summary>
        /// Loads the inherited values to the node.
        /// </summary>
        /// <param name="columns">Columns to load</param>
        /// <param name="allCultures">Indicates if all culture versions of the parent documents will be taken in account</param>
        public virtual void LoadInheritedValues(string[] columns, bool allCultures = true)
        {
            // Get only columns which don't have a value
            List<string> inherited = new List<string>();
            foreach (string col in columns)
            {
                if (GetValue(col) == null)
                {
                    inherited.Add(col);
                }
            }

            // No columns need to load value
            if (inherited.Count == 0)
            {
                return;
            }

            // Get inherited data
            var data = GetUpperTree(inherited.Join(","), allCultures);

            // Load the columns data
            foreach (string col in inherited)
            {
                object value = TreePathUtils.GetNodeInheritedValueInternal(data, col, DocumentCulture);

                // Inherited value don't have to be present
                if (value != null)
                {
                    SetValue(col, value);
                }
            }
        }


        /// <summary>
        /// Returns the inherited value for the document.
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="allCultures">All cultures</param>
        public virtual object GetInheritedValue(string column, bool allCultures = true)
        {
            // Get inherited values
            var data = GetUpperTree(column, allCultures);
            return TreePathUtils.GetNodeInheritedValueInternal(data, column, DocumentCulture);
        }


        /// <summary>
        /// Returns the DataSet of node data. For linked documents data of original document is included.
        /// </summary>
        protected internal virtual DataSet GetOriginalDataSet()
        {
            // Get the empty DataSet
            var data = DocumentHelper.GetTreeNodeDataSet(NodeClassName, IsCoupled, HasSKU);
            var table = data.Tables[0];

            AddDataToDataTable(table, true);

            return data;
        }


        /// <summary>
        /// Returns the DataSet of the node data.
        /// </summary>
        public virtual DataSet GetDataSet()
        {
            // Get the empty DataSet
            var data = DocumentHelper.GetTreeNodeDataSet(NodeClassName, IsCoupled, HasSKU);
            var table = data.Tables[0];

            AddDataToDataTable(table, false);

            return data;
        }


        /// <summary>
        /// Sets the value for the column in the DataRow.
        /// </summary>
        protected void SetDataRowColumn(DataRow row, string columnName, object rawValue)
        {
            var value = DataHelper.GetDBNull(rawValue);
            if (value != DBNull.Value)
            {
                var columnType = Generalized.GetColumnType(columnName);
                row[columnName] = DataHelper.ConvertValue(value, columnType);
            }
            else
            {
                row[columnName] = value;
            }
        }


        /// <summary>
        /// Gets the unique string key for the object.
        /// </summary>
        protected override string GetObjectKey()
        {
            return String.Format("{0}_{1}", TypeInfo.ObjectType, DocumentID);
        }


        /// <summary>
        /// Returns document preview link.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="ensureQueryHash">Indicates if query string hash should be added. For content-only nodes, hash is added always.</param>
        public virtual string GetPreviewLink(string userName, bool ensureQueryHash = false)
        {
            if (DocumentWorkflowCycleGUID != Guid.Empty)
            {
                var presentationUrl = Site.SitePresentationURL;
                var presentationUrlAvailable = !String.IsNullOrEmpty(presentationUrl);

                string url;
                if (NodeIsContentOnly)
                {
                    var urlPattern = DataClassInfo.ClassURLPattern;
                    if (String.IsNullOrEmpty(urlPattern))
                    {
                        // Page has no preview link without URL pattern
                        return null;
                    }

                    // Get standard URL (content only page does not have permanent URL)
                    url = DocumentURLProvider.GetUrl(this);

                    if (!presentationUrlAvailable)
                    {
                        // If not MVC site do not create virtual context and handle URL as portal engine page
                        return url;
                    }

                    ensureQueryHash = true;
                }
                else
                {
                    url = DocumentURLProvider.GetPermanentDocUrl(NodeGUID, VirtualContext.PARAM_PREVIEW_LINK, NodeSiteName, PageInfoProvider.PREFIX_CMS_GETDOC, ".aspx");
                }

                if (ensureQueryHash)
                {
                    url = URLHelper.RemoveApplicationPath(url.TrimStart('~')).ToLowerInvariant();
                    url = "~/" + VirtualContext.AddPreviewHash(url).TrimStart('/');
                }

                // Prepare context parameters
                NameValueCollection param = new NameValueCollection();
                param.Add(VirtualContext.PARAM_PREVIEW_LINK, userName);
                param.Add("culture", DocumentCulture);
                param.Add(VirtualContext.PARAM_WF_GUID, DocumentWorkflowCycleGUID.ToString());

                url = VirtualContext.GetVirtualContextPath(url, param);

                // Append site presentation URL if present
                if (presentationUrlAvailable)
                {
                    return URLHelper.CombinePath(url, '/', presentationUrl, null);
                }

                return url;
            }

            return null;
        }


        /// <summary>
        /// Returns friendly document name.
        /// </summary>
        public virtual string GetDocumentName()
        {
            return string.IsNullOrEmpty(DocumentName) ? ResHelper.GetString("general.root") : DocumentName;
        }

        #endregion


        #region "Workflow methods"

        /// <summary>
        /// Checks the document in.
        /// </summary>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        public virtual void CheckIn(string versionNumber = null, string versionComment = null)
        {
            VersionManager.CheckIn(this, versionNumber, versionComment);
        }


        /// <summary>
        /// Checks the document out.
        /// </summary>
        public virtual WorkflowStepInfo CheckOut()
        {
            return VersionManager.CheckOut(this);
        }


        /// <summary>
        /// Rolls the checkout operation of the document back.
        /// </summary>
        public virtual void UndoCheckOut()
        {
            VersionManager.UndoCheckOut(this);
        }


        /// <summary>
        /// Creates new document version. (Moves document to edit step.)
        /// </summary>
        public virtual void CreateNewVersion()
        {
            VersionManager.CreateNewVersion(this);
        }


        /// <summary>
        /// Moves the document to a previous step in the workflow (rejects the document).
        /// </summary>
        /// <param name="comment">Comment</param>
        public virtual WorkflowStepInfo MoveToPreviousStep(string comment = null)
        {
            return WorkflowManager.MoveToPreviousStep(this, comment);
        }


        /// <summary>
        /// Moves the document to a next step in the workflow (sends the document to approval, approves or publishes the document based on the current workflow status).
        /// </summary>
        /// <param name="comment">Comment</param>
        public virtual WorkflowStepInfo MoveToNextStep(string comment = null)
        {
            return WorkflowManager.MoveToNextStep(this, comment);
        }


        /// <summary>
        /// Moves the document to first step in the workflow (removes the workflow information if document is not under a workflow anymore).
        /// </summary>
        /// <param name="comment">Comment</param>
        public virtual WorkflowStepInfo MoveToFirstStep(string comment = null)
        {
            return WorkflowManager.MoveToFirstStep(this, comment);
        }


        /// <summary>
        /// Moves the document directly to published workflow step (moves document directly to publish step, skips all following steps).
        /// </summary>
        /// <param name="comment">Comment</param>
        public virtual WorkflowStepInfo MoveToPublishedStep(string comment = null)
        {
            return WorkflowManager.MoveToPublishedStep(this, comment);
        }


        /// <summary>
        /// Archives the document.
        /// </summary>
        /// <param name="comment">Comment</param>
        public virtual WorkflowStepInfo Archive(string comment = null)
        {
            return WorkflowManager.ArchiveDocument(this, comment);
        }


        /// <summary>
        /// Publishes the document (moves document to publish step through all following steps). If there is not only one path to published step, document stays in the last step it reached.
        /// </summary>
        /// <param name="comment">Comment</param>
        /// <returns>TRUE if document is moved to publish step.</returns>
        public virtual WorkflowStepInfo Publish(string comment = null)
        {
            return WorkflowManager.PublishDocument(this, comment);
        }


        /// <summary>
        /// Gets node current workflow (depends on document current workflow step or if document is under a workflow scope).
        /// </summary>
        public virtual WorkflowInfo GetWorkflow()
        {
            var step = WorkflowStep;
            return step != null ? WorkflowInfoProvider.GetWorkflowInfo(step.StepWorkflowID) : null;
        }


        /// <summary>
        /// Gets the document workflow step
        /// </summary>
        private BaseInfo GetWorkflowStep()
        {
            return WorkflowManager.GetStepInfo(this);
        }

        #endregion


        #region "Internal update methods"

        /// <summary>
        /// Returns the upper tree data for given document.
        /// </summary>
        /// <param name="columns">Columns to get</param>
        /// <param name="allCultures">All cultures</param>
        protected virtual DataSet GetUpperTree(string columns, bool allCultures = true)
        {
            string culture = allCultures ? TreeProvider.ALL_CULTURES : DocumentCulture;
            return TreePathUtils.GetNodeUpperTreeInternal(NodeSiteID, NodeAliasPath, columns, culture);
        }


        /// <summary>
        /// Ensures target site of the linked documents if site changes
        /// </summary>
        private void EnsureLinksSite()
        {
            var where = new WhereCondition()
                .WhereIn("NodeLinkedNodeID", GetNodeIDsQueryByPathAndSite());

            // Update site of the linked documents
            DocumentNodeDataInfoProvider.BulkUpdateData(
                where,
                new Dictionary<string, object> {
                    { "NodeLinkedNodeSiteID", NodeSiteID }
                }
            );
        }


        /// <summary>
        /// Gets query for node IDs based on site and alias path
        /// </summary>
        internal ObjectQuery<DocumentNodeDataInfo> GetNodeIDsQueryByPathAndSite()
        {
            var nodeIdsQuery = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                           .Columns("NodeID")
                                                           .Where(new WhereCondition()
                                                               .WhereEquals("NodeAliasPath", NodeAliasPath).Or()
                                                               .WhereStartsWith("NodeAliasPath", NodeAliasPath.TrimEnd('/') + "/"))
                                                           .OnSite(NodeSiteID);
            return nodeIdsQuery;
        }




        /// <summary>
        /// Updates the content of the document.
        /// </summary>
        protected virtual void UpdateDocumentContent()
        {
            SetValue("DocumentContent", DocumentContent.GetContentXml());
        }


        /// <summary>
        /// Deletes the child nodes of all culture versions under the specified parent node.
        /// </summary>
        protected virtual void DeleteChildNodes()
        {
            foreach (var child in TreeProvider.EnumerateChildren(this))
            {
                // Skip already deleted children
                if (child == null)
                {
                    continue;
                }

                child.DeleteAllCultures();
            }

            LicenseHelper.ClearLicenseLimitation();
        }


        /// <summary>
        /// Raises the notifications events and sends the notifications.
        /// </summary>
        public void SendNotifications(string eventType)
        {
            // Is document being deleted?
            bool deleteEvent = string.Equals(eventType, "deletedoc", StringComparison.OrdinalIgnoreCase);

            // Check if notifications are available and document is published
            if (!TreeProvider.EnableNotifications || (!IsPublished && (!deleteEvent || !PublishedVersionExists)) || !LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.Notifications))
            {
                return;
            }

            var sourceMacros = new Dictionary<string, object>
            {
                { "documentlink", AbsoluteURL }
            };

            ModuleCommands.NotificationsRaiseEvent("Content", eventType, 0, NodeAliasPath, null, NodeSiteID, "(SubscriptionEventData2 LIKE '%" + SqlHelper.EscapeLikeText(SqlHelper.EscapeQuotes(NodeClassName)) + "%') OR ISNULL(SubscriptionEventData2, '') = ''", this, sourceMacros);
        }


        /// <summary>
        /// Registers the grouped attachments for this document.
        /// </summary>
        /// <param name="repository">Object repository</param>
        protected virtual void RegisterGroupedAttachments(DocumentAttachmentRepository repository)
        {
            // Do not register attachments for virtual node
            if (NodeClassName == null)
            {
                return;
            }

            // Attachments
            int versionHistoryId = DocumentCheckedOutVersionHistoryID;

            // Get attachment fields
            FormInfo fi = FormHelper.GetFormInfo(NodeClassName, false);
            var fields = fi.GetFields(FormFieldControlTypeEnum.DocumentAttachmentsControl);

            foreach (var field in fields)
            {
                if (IsLastVersion && (versionHistoryId > 0))
                {
                    // Attachments
                    var settings = new InfoCollectionSettings(field.Name, AttachmentHistoryInfo.OBJECT_TYPE)
                    {
                        WhereCondition = new WhereCondition("AttachmentHistoryID IN (SELECT AttachmentHistoryID FROM CMS_VersionAttachment WHERE VersionHistoryID = " + versionHistoryId + ") AND AttachmentGroupGUID = N'" + field.Guid + "'"),
                        OrderBy = "AttachmentOrder"
                    };

                    repository.AddCollection(settings);
                }
                else
                {
                    // Attachments
                    var settings = new InfoCollectionSettings(field.Name, AttachmentInfo.OBJECT_TYPE)
                    {
                        WhereCondition = new WhereCondition("AttachmentDocumentID = " + DocumentID + " AND AttachmentGroupGUID = N'" + field.Guid + "'"),
                        OrderBy = "AttachmentOrder"
                    };

                    repository.AddCollection(settings);
                }
            }
        }

        #endregion


        #region "Overridden info methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Delete();
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SetDocumentInternal(true);
        }


        /// <summary>
        /// Updates the document using appropriate provider.
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used for the operation to handle workflow</param>
        internal void SetDocumentInternal(bool useDocumentHelper)
        {
            if (IsLink)
            {
                if (NodeID > 0)
                {
                    if (IsTranslated(DocumentCulture))
                    {
                        Update(useDocumentHelper);
                    }
                    else
                    {
                        InsertAsNewCultureVersionWithMoveSupport(useDocumentHelper);
                    }
                }
                else
                {
                    InsertAsLink(Parent, useDocumentHelper: useDocumentHelper);
                }
            }
            else
            {
                if (DocumentID > 0)
                {
                    Update(useDocumentHelper);
                }
                else
                {
                    if (NodeID > 0)
                    {
                        InsertAsNewCultureVersionWithMoveSupport(useDocumentHelper);
                    }
                    else
                    {
                        Insert(Parent, useDocumentHelper);
                    }
                }
            }
        }


        /// <summary>
        /// Inserts the document as new culture version.
        /// If parent node changed, also moves the document to a new location. This is done in two steps, insert new culture version and then move.
        /// </summary>
        private void InsertAsNewCultureVersionWithMoveSupport(bool useDocumentHelper = true)
        {
            // Check if the document is also to be moved
            var originalParentId = GetOriginalValue("NodeParentID").ToInteger(0);
            var parentId = NodeParentID;

            if ((originalParentId > 0) && (originalParentId != NodeParentID))
            {
                NodeParentID = originalParentId;

                // First insert as new culture version
                InsertAsNewCultureVersion(DocumentCulture, useDocumentHelper);

                // After that move to a new location
                NodeParentID = parentId;

                Update(useDocumentHelper);
            }
            else
            {
                // Only insert as new culture version
                InsertAsNewCultureVersion(DocumentCulture, useDocumentHelper);
            }
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return TreeNodeProvider.GetQueryInternal(NodeClassName)
                                   .LatestVersion(IsLastVersion);
        }


        /// <summary>
        /// Gets the child object where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="objectType">Object type of the child object</param>
        protected override WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
        {
            // Ensure base where condition
            where = where ?? new WhereCondition();

            switch (objectType)
            {
                case PredefinedObjectType.DOCUMENTMVTVARIANT:
                    // Get the MVT variants only for widgets
                    where.WhereNotNull("MVTVariantDocumentID");
                    break;

                case PredefinedObjectType.DOCUMENTMVTCOMBINATION:
                    // Get the MVT combinations only for widgets
                    where.WhereNotNull("MVTCombinationDocumentID");
                    break;

                case PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT:
                    // Get the Content personalization variants only for widgets
                    where.WhereNotNull("VariantDocumentID");
                    break;
            }

            return base.GetChildWhereCondition(where, objectType);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="currentSiteName">Name of the current context site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsWithHandler(PermissionsEnum permission, string currentSiteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            var e = new DocumentSecurityEventArgs
            {
                Node = this,
                User = userInfo,
                SiteName = currentSiteName,
                Permission = permission
            };

            // Handle the event
            using (var h = DocumentEvents.CheckPermissions.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Combine with default result
                    e.Result = e.Result.CombineWith(CheckPermissionsInternal(permission, currentSiteName, userInfo, exceptionOnFailure));
                }

                // Finish the event
                h.FinishEvent();
            }

            // Convert result to bool
            var boolResult = e.Result.ToBoolean();

            if (exceptionOnFailure)
            {
                PermissionCheckException(permission, currentSiteName, boolResult);
            }

            return boolResult;
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            UserInfo user = (UserInfo)userInfo;
            NodePermissionsEnum nodePermission = DocumentSecurityHelper.GetNodePermissionEnum(permission);

            if (nodePermission == NodePermissionsEnum.Create)
            {
                return DocumentSecurityHelper.IsAuthorizedToCreateNewDocument(NodeParentID, NodeClassName, DocumentCulture, user);
            }

            AuthorizationResultEnum result = DocumentSecurityHelper.IsAuthorizedPerDocument(this, nodePermission, true, DocumentCulture, user, siteName);

            return result.ToBoolean();
        }


        /// <summary>
        /// Gets the list of local column names for particular object.
        /// </summary>
        protected override List<string> GetLocalColumnNames()
        {
            // Get base names
            var names = new List<string>();

            if (!HideTypeSpecificColumns)
            {
                names.AddRange(GetTypeSpecificColumnNames());
            }

            // Get extra column
            names.Add("ClassName");
            names.Add("ClassDisplayName");

            return names;
        }


        /// <summary>
        /// Gets the column names that are specific to the type
        /// </summary>
        protected internal virtual IEnumerable<string> GetTypeSpecificColumnNames()
        {
            // Add couple class
            return IsCoupled ? CoupledData.ColumnNames : new List<string>();
        }


        /// <summary>
        /// Returns object name combining object type name and object display name.
        /// </summary>
        protected override string GetObjectName()
        {
            var culture = CultureInfoProvider.GetCultureInfo(DocumentCulture);
            var cultureName = culture != null ? culture.CultureName : DocumentCulture;
            return String.Format("{0} {1} ({2})", TypeInfo.GetNiceObjectTypeName(), DocumentNamePath, cultureName);
        }


        /// <summary>
        /// Returns the name of the object within its parent hierarchy.
        /// </summary>
        /// <param name="includeParent">If true, the parent object name is included to the object name</param>
        /// <param name="includeSite">If true, the site information is included if available</param>
        /// <param name="includeGroup">If true, the group information is included if available</param>
        protected override string GetFullObjectName(bool includeParent, bool includeSite, bool includeGroup)
        {
            // Do not include hierarchy information
            return base.GetFullObjectName(false, includeSite, includeGroup);
        }


        #region "Order methods"

        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return new WhereCondition().WhereEquals("NodeParentID", NodeParentID);
        }


        /// <summary>
        /// Gets order identity where condition to identify the correct node data which are currently ordered
        /// </summary>
        /// <remarks>
        /// Includes only NodeID to correct identify the node data and doesn't matter which culture uses.
        /// </remarks>
        protected override WhereCondition GetOrderIdentityWhereCondition()
        {
            return new WhereCondition().WhereEquals("NodeID", NodeID);
        }


        /// <summary>
        /// Returns ID of the item being ordered (i.e. NodeID).
        /// </summary>
        protected override int GetObjectOrderID()
        {
            return NodeID;
        }


        /// <summary>
        /// Gets the parametrized query to get siblings of the document. Uses best matching culture and filters out all culture versions of the current document.
        /// </summary>
        /// <remarks>
        /// The culture is taken preferably from the current node instance or default culture of the site or any other existing culture version. 
        /// </remarks>
        /// <param name="parameters">Parameters for the data retrieval</param>
        protected override IDataQuery GetSiblingsQueryInternal(Action<DataQuerySettings> parameters)
        {
            var q = DocumentHelper.GetDocuments(null)
                                 .All()
                                 .LatestVersion(IsLastVersion)
                                 .Where(GetSiblingsWhereCondition())
                                 .AllCultures(false)
                                 .Culture(DocumentCulture, CultureHelper.GetDefaultCultureCode(NodeSiteName))
                                 .CombineWithAnyCulture()
                                 .ApplySettings(parameters);

            return q;
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            // Invalidate sorted child documents
            NodeData.TypeInfo.ChildrenInvalidated(NodeParentID);

            // Drop the cache dependencies
            if (TouchCacheDependencies)
            {
                CacheHelper.TouchKeys(DocumentDependencyCacheKeysBuilder.GetChangeOrderDependencyCacheKeys(NodeParentID));
            }
        }


        /// <summary>
        /// Method called after the InitObjectOrder method is called. Override this to do further actions after order initialization. Does nothing by default.
        /// </summary>
        protected override void InitObjectsOrderPostprocessing()
        {
            // Invalidate sorted child documents
            NodeData.TypeInfo.ChildrenInvalidated(NodeParentID);

            // Drop the cache dependencies
            if (TouchCacheDependencies)
            {
                CacheHelper.TouchKeys(DocumentDependencyCacheKeysBuilder.GetChangeOrderDependencyCacheKeys(NodeParentID));
            }
        }


        /// <summary>
        /// Creates QueryDataParameters with special macros for object order management.
        /// </summary>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        /// <param name="nameColumn">Name of the column by which the order should be initialized (if not set, displayname column is used)</param>
        /// <param name="asc">If true the order will be ascending (default is true)</param>
        protected override QueryDataParameters GetOrderQueryParameters(string orderColumn, string nameColumn = null, bool asc = true)
        {
            // This cannot be automatically generated from type info, Order is not defined in CMS_Document table, but in CMS_Tree
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.AddMacro("##IDColumn##", "NodeID");
            parameters.AddMacro("##ORDERCOLUMN##", "NodeOrder");
            parameters.AddMacro("##TABLE##", "CMS_Tree");

            string tableDef;
            string selectDef;
            string orderByDef;
            if (string.IsNullOrEmpty(nameColumn))
            {
                tableDef = String.Format("NodeID int,\nNodeOrder int,\nDocumentName nvarchar({0}),\nNodeAlias nvarchar({1})", TreePathUtils.MaxNameLength, TreePathUtils.MaxAliasLength);
                selectDef = "NodeID, NodeOrder, DocumentName, NodeAlias";
                orderByDef = "NodeOrder, DocumentName" + (asc ? SqlHelper.ORDERBY_ASC : SqlHelper.ORDERBY_DESC) + ", NodeAlias" + (asc ? SqlHelper.ORDERBY_ASC : SqlHelper.ORDERBY_DESC);
            }
            else
            {
                tableDef = "NodeID int,\nNodeOrder int,\n[" + nameColumn + "] nvarchar(250)";
                selectDef = "NodeID, NodeOrder, [" + nameColumn + "]";
                orderByDef = "[" + nameColumn + "]" + (asc ? SqlHelper.ORDERBY_ASC : SqlHelper.ORDERBY_DESC) + ", NodeOrder";
            }

            parameters.AddMacro("##TABLEDEF##", tableDef);
            parameters.AddMacro("##SELECTDEF##", selectDef);
            parameters.AddMacro("##ORDERBYDEF##", orderByDef);
            // Sort documents using current culture language versions (view cannot be used because of the bulk update)
            parameters.AddMacro("##TABLESELECT##", "(SELECT * FROM (SELECT " + selectDef + ", NodeParentID, ROW_NUMBER() OVER (PARTITION BY NodeID ORDER BY CASE WHEN DocumentCulture = '" + LocalizationContext.PreferredCultureCode + "' THEN 1 ELSE 2 END) AS Priority FROM CMS_Document LEFT JOIN CMS_Tree ON CMS_Document.DocumentNodeID = CMS_Tree.NodeOriginalNodeID WHERE " + GetSiblingsWhereCondition() + ") AS LANG_VERSIONS WHERE Priority = 1) AS DOCS");

            return parameters;
        }

        #endregion

        #endregion


        #region "Cache methods"

        /// <summary>
        /// Clears the document cache.
        /// </summary>
        public new void ClearCache()
        {
            ClearCache(NodeSiteName);
        }


        /// <summary>
        /// Clears the document cache.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected void ClearCache(string siteName)
        {
            CacheHelper.TouchKeys(DocumentDependencyCacheKeysBuilder.GetNodeDependencyCacheKeys(this, siteName));
        }


        /// <summary>
        /// Clears the output cache for current page.
        /// </summary>
        /// <param name="allCultures">Clear all cultures cache</param>
        public void ClearOutputCache(bool allCultures)
        {
            ClearOutputCache(allCultures, false);
        }


        /// <summary>
        /// Clears the output cache for current page.
        /// </summary>
        /// <param name="allCultures">Clear all cultures cache</param>
        /// <param name="childNodes">If true, the output cache of the child nodes is cleared as well</param>
        public void ClearOutputCache(bool allCultures, bool childNodes)
        {
            CacheHelper.ClearOutputCache(NodeSiteID, NodeAliasPath, DocumentCulture, allCultures, childNodes);
        }

        #endregion


        #region "Search methods"

        /// <summary>
        /// Returns an unique id of current object for search index (documentid;nodeid).
        /// </summary>
        public override string GetSearchID()
        {
            return DocumentID + ";" + NodeID;
        }


        /// <summary>
        /// Returns search fields collection. When existing collection is passed as argument, fields will be added to that collection.
        /// When collection is not passed, new collection will be created and return. 
        /// Collection will contain field values only when collection with StoreValues property set to true is passed to the method.
        /// When method creates new collection, it is created with StoreValues property set to false.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(ISearchIndexInfo index, ISearchFields searchFields = null)
        {
            var provider = new DocumentSearchFieldsProvider(this, index, searchFields);
            return provider.Get();
        }


        /// <summary>
        /// Returns document with dependence on current object type and index.
        /// </summary>
        /// <param name="index">Search index info</param>        
        public override SearchDocument GetSearchDocument(ISearchIndexInfo index)
        {
            var documentCreator = new SearchDocumentCreator(this, index);
            return documentCreator.Create();
        }


        #endregion


        #region "ITreeNodeMethods Members"

        /// <summary>
        /// Returns true if the document type stands for the product section.
        /// </summary>
        public bool IsProductSection()
        {
            return TreeNodeMethods.IsProductSection(this);
        }


        /// <summary>
        /// Returns true if the document represents a product.
        /// </summary>
        public bool IsProduct()
        {
            return TreeNodeMethods.IsProduct(this);
        }


        /// <summary>
        /// Sets the default document page template ID.
        /// </summary>
        /// <param name="templateId">Page template ID to set</param>
        public void SetDefaultPageTemplateID(int templateId)
        {
            if (templateId > 0)
            {
                // Get the template info
                PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                if ((pti != null) && !pti.IsReusable && (pti.PageTemplateNodeGUID != NodeGUID))
                {
                    // Update the page template
                    pti.PageTemplateNodeGUID = NodeGUID;

                    PageTemplateInfoProvider.SetPageTemplateInfo(pti);
                }
            }

            NodeTemplateForAllCultures = true;

            NodeTemplateID = templateId;
            DocumentPageTemplateID = templateId;
        }


        /// <summary>
        /// Gets the page template id used by this document.
        /// </summary>
        public string GetUsedPageTemplateIdColumn()
        {
            return TreeNodeMethods.GetUsedPageTemplateIdColumn(this);
        }


        /// <summary>
        /// Gets the page template id used by this document.
        /// </summary>
        public int GetUsedPageTemplateId()
        {
            if (NodeTemplateForAllCultures)
            {
                int templateId = NodeTemplateID;
                if (templateId <= 0)
                {
                    templateId = DocumentPageTemplateID;
                }

                return templateId;
            }

            return DocumentPageTemplateID;
        }


        /// <summary>
        /// Returns true if the document is a root node.
        /// </summary>
        protected bool IsRootNode()
        {
            return TreeNodeMethods.IsRoot(this);
        }

        #endregion


        #region "Versioning methods"

        /// <summary>
        /// Ensures data consistency.
        /// </summary>
        public virtual void EnsureConsistency()
        {
            VersionManager.EnsureConsistencyInternal(this);
        }

        #endregion


        #region "Attachment methods"

        /// <summary>
        /// Retrieves the attachment associated with the specified field of type File.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The attachment associated with the specified field, if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null.</exception>
        /// <remarks>
        /// This method returns null if the specified field does not exist or there is no associated attachment.
        /// </remarks>
        protected Attachment GetFieldAttachment(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            Guid attachmentGuid = ValidationHelper.GetGuid(GetValue(fieldName), Guid.Empty);
            if (attachmentGuid == Guid.Empty)
            {
                return null;
            }

            var attachment = AllAttachments.SingleOrDefault(x => x.GetGuidValue("AttachmentGUID", Guid.Empty) == attachmentGuid);
            if (attachment == null)
            {
                return null;
            }

            return new Attachment(attachment);
        }


        /// <summary>
        /// Retrieves an enumerable collection of attachments associated with the specified field of type Attachments.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>An enumerable collection of attachments associated with the specified field of type Attachments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null.</exception>
        /// <remarks>
        /// This method returns an empty collection if the specified field does not exist or there are no associated attachments.
        /// </remarks>
        protected IEnumerable<Attachment> GetFieldAttachments(string fieldName)
        {
            var collection = GroupedAttachments[fieldName];
            if (collection == null)
            {
                return Enumerable.Empty<Attachment>();
            }

            return collection.Select(x => new Attachment(x));
        }

        #endregion


        #region "Relationship methods"

        /// <summary>
        /// Retrieves a query that selects documents related to the current document with the relationship name given by specified field.
        /// Selects only documents of the same culture as the parent node.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>A query that selects documents related to the current document with the relationship name given by specified field.</returns>
        /// <remarks>
        /// This method returns an empty collection if the specified field does not exist or there are no associated related pages.
        /// </remarks>
        protected MultiDocumentQuery GetRelatedDocuments(string fieldName)
        {
            var form = FormHelper.GetFormInfo(NodeClassName, false);

            // Get field info
            var field = form.GetFormField(fieldName);
            if (field == null)
            {
                return new MultiDocumentQuery().NoResults();
            }

            var relationshipName = RelationshipNameInfoProvider.GetAdHocRelationshipNameCodeName(NodeClassName, field);
            var relationshipNameObject = RelationshipNameInfoProvider.GetRelationshipNameInfo(relationshipName);
            bool combineWithDefaultCulture = TreeProvider.GetCombineWithDefaultCulture(Site.SiteName);

            var q = DocumentHelper.GetDocuments()
                .Culture(DocumentCulture)
                // "CombineWithDefaultCulture" must be specified explicitly; otherwise it takes the settings value from the global setting because no site is specified in the query
                .CombineWithDefaultCulture(combineWithDefaultCulture)
                .Published(!IsLastVersion)
                .PublishedVersion(!IsLastVersion)
                .WithCoupledColumns()
                .InRelationWith(NodeGUID, relationshipName, RelationshipSideEnum.Left);

            // Ensure correct order for ad-hoc relationships 
            q = RelationshipInfoProvider.ApplyRelationshipOrderData(q, NodeID, relationshipNameObject.RelationshipNameId);

            return q;
        }


        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures the document wildcard values
        /// </summary>
        private void EnsureWildcardValues()
        {
            if (!ItemChanged("DocumentURLPath"))
            {
                return;
            }

            var wildcardUrlPath = DocumentUrlPath;
            int wildcardCount;

            DocumentWildcardRule = DocumentAliasInfoProvider.CreateWildcardRule(wildcardUrlPath, out wildcardCount);

            // Count priority and set it
            int slashCount = wildcardUrlPath.Split('/').Length - 1;
            DocumentPriority = slashCount - wildcardCount;
        }


        /// <summary>
        /// Ensures default data for updating node.
        /// </summary>
        private void EnsureDefaultDataForUpdate()
        {
            SetValue("NodeInheritPageLevels", NodeInheritPageLevels);

            UpdateTimeStamps(false);
            UpdateUserStamps();

            EnsureDefaultData();

            EnsureDocumentGUID();
        }


        /// <summary>
        /// Ensures the default data for inserting new culture version.
        /// </summary>
        private void EnsureDefaultDataForInsertNewCulture()
        {
            // Reset DocumentWorkflowCycleGUID and DocumentGUID for new culture version
            if (TreeProvider.GenerateNewGuid)
            {
                DocumentGUID = Guid.NewGuid();
                DocumentWorkflowCycleGUID = Guid.NewGuid();
            }

            // Update when document was created
            UpdateTimeStamps();

            // Update users
            UpdateUserStamps(false, true);

            // Bind to the tree node
            DocumentNodeID = GetOriginalNodeIDForInsert();

            EnsureDefaultData();
        }


        /// <summary>
        /// Ensures the default data for the inserting new document
        /// </summary>
        /// <param name="parent">Parent node</param>
        private void EnsureDefaultDataForInsert(TreeNode parent)
        {
            EnsureNodeGUID();
            EnsureDocumentGUID();
            EnsureWorkflowCycleGUID();

            UpdateTimeStamps();
            UpdateUserStamps(true, true);

            TreeProvider.SetInheritedACL(this, parent.NodeACLID);

            EnsureDefaultData();
        }


        /// <summary>
        /// Ensures default data.
        /// </summary>
        private void EnsureDefaultData()
        {
            // If ShowInSiteMap is null, set default value
            if (GetValue("DocumentShowInSiteMap") == null)
            {
                DocumentShowInSiteMap = true;
            }
            // If HideInNavigation is null, set default value
            if (GetValue("DocumentMenuItemHideInNavigation") == null)
            {
                DocumentMenuItemHideInNavigation = false;
            }
            // If DocumentIsArchived is null, set default value (false)
            if (GetValue("DocumentIsArchived") == null)
            {
                DocumentIsArchived = false;
            }
        }


        private void EnsureDocumentGUID()
        {
            if (DocumentGUID == Guid.Empty)
            {
                DocumentGUID = Guid.NewGuid();
            }
        }


        private void EnsureNodeGUID()
        {
            if (NodeGUID == Guid.Empty)
            {
                NodeGUID = Guid.NewGuid();
            }
        }


        private void EnsureWorkflowCycleGUID()
        {
            if (DocumentWorkflowCycleGUID == Guid.Empty)
            {
                DocumentWorkflowCycleGUID = Guid.NewGuid();
            }
        }


        private void SetNodeLevel(DocumentNodeDataInfo parent)
        {
            NodeLevel = parent.NodeLevel + 1;
        }


        /// <summary>
        /// Updates Created and Modified timestamps
        /// </summary>
        /// <param name="updateCreatedWhen">Indicates if CreatedWhen should be updated</param>
        private void UpdateTimeStamps(bool updateCreatedWhen = true)
        {
            if (updateCreatedWhen && ((GetValue("DocumentCreatedWhen") == null) || TreeProvider.UpdateTimeStamps))
            {
                DocumentCreatedWhen = DateTime.Now;
            }

            if ((GetValue("DocumentModifiedWhen") == null) || TreeProvider.UpdateTimeStamps)
            {
                DocumentModifiedWhen = DateTime.Now;
            }
        }


        /// <summary>
        /// Updates CreatedBy and ModifiedBy user stamps
        /// </summary>
        /// <param name="updateOwner">Indicates if NodeOwner should be updated</param>
        /// <param name="updateCreatedBy">Indicates if CreatedBy should be updated</param>
        private void UpdateUserStamps(bool updateOwner = false, bool updateCreatedBy = false)
        {
            var userId = TreeProvider.UserInfo.UserID;
            if (userId == 0)
            {
                return;
            }

            if (updateCreatedBy && ((GetValue("DocumentCreatedByUserID") == null) || TreeProvider.UpdateUser))
            {
                DocumentCreatedByUserID = userId;
            }

            if ((GetValue("DocumentModifiedByUserID") == null) || TreeProvider.UpdateUser)
            {
                DocumentModifiedByUserID = userId;
            }

            if (updateOwner && ((NodeOwner <= 0) || TreeProvider.UpdateUser))
            {
                NodeOwner = TreeProvider.UserInfo.UserID;
            }
        }


        /// <summary>
        /// Sets default values for content only documents. The content item can not use any template or inherit template from the parent.
        /// </summary>
        private void SetContentOnlyDefaultValues()
        {
            // "/" means inherit NONE
            NodeInheritPageLevels = "/";
            DocumentPageTemplateID = 0;
            NodeTemplateID = 0;
            NodeInheritPageTemplate = false;
            DocumentStylesheetID = 0;
        }


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
        /// Removes user personalization for specified document.
        /// </summary>
        private void RemovePersonalizationDependencies()
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@ID", DocumentID);

            ConnectionHelper.ExecuteQuery("cms.personalization.removedocumentdependencies", parameters);
        }


        /// <summary>
        /// Deletes all event attendees of specified document.
        /// </summary>
        private void DeleteEventAttendees()
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@NodeId", NodeID);

            // Delete attendees
            ConnectionHelper.ExecuteQuery("cms.eventattendee.removeeventattendees", parameters);
        }


        /// <summary>
        /// Ensures initial node order based on settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        private void EnsureInitialOrder(string siteName)
        {
            if (TreeProvider.UseAutomaticOrdering)
            {
                TreeProvider.SetNodeOrder(this, TreePathUtils.NewDocumentOrder(siteName));
            }
        }


        /// <summary>
        /// Updates node name value based on document name changes
        /// </summary>
        private void UpdateNodeName()
        {
            var nodeName = NodeName;

            // Do not process if node name update is not allowed and node name is set already
            if (!String.IsNullOrEmpty(nodeName) && !TreeProvider.UpdateNodeName)
            {
                return;
            }

            var docName = DocumentName;

            // Update node name for default culture version
            if (IsDefaultCulture)
            {
                if (string.IsNullOrWhiteSpace(docName) && !IsRootNode())
                {
                    throw new Exception("The DocumentName value was not specified. Data cannot be saved.");
                }

                // Update node name
                nodeName = docName;
            }

            if (TreeProvider.CheckUniqueNames && !IsLink)
            {
                // Ensure unique names
                nodeName = TreePathUtils.GetUniqueNodeName(nodeName, NodeParentID, NodeID, NodeClassName);
            }
            else
            {
                nodeName = TreePathUtils.EnsureMaxNodeNameLength(nodeName, NodeClassName);
            }

            // Node name changed from the original or previous value
            if (!string.Equals(nodeName, GetOriginalValue("NodeName").ToString(String.Empty), StringComparison.InvariantCultureIgnoreCase) || !string.Equals(nodeName, NodeName, StringComparison.InvariantCultureIgnoreCase))
            {
                NodeName = nodeName;
            }
        }


        /// <summary>
        /// Indicates if node document type has node alias source field defined.
        /// </summary>
        internal bool ClassNodeAliasSourceDefined()
        {
            return (DataClassInfo != null) && !String.IsNullOrWhiteSpace(DataClassInfo.ClassNodeAliasSource);
        }


        /// <summary>
        /// Gets document parent in best matching culture version.
        /// </summary>
        private TreeNode GetParentNode()
        {
            var parentNode = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(NodeParentID);
            if (parentNode == null)
            {
                return null;
            }

            return TreeProvider.SelectNodes(parentNode.NodeClassName)
                               .All()
                               .LatestVersion(IsLastVersion)
                               .WhereEquals("NodeID", NodeParentID)
                               // Prefer document culture, than site default culture and any other culture version as a last option
                               .AllCultures(false)
                               .Culture(DocumentCulture, CultureHelper.GetDefaultCultureCode(NodeSiteName))
                               .CombineWithAnyCulture()
                               .FirstOrDefault();
        }


        /// <summary>
        /// Ensures consistent properties for root document
        /// </summary>
        private void EnsureRootProperties()
        {
            NodeAlias = string.Empty;
            NodeAliasPath = "/";
            DocumentName = string.Empty;
            DocumentNamePath = "/";
            DocumentUrlPath = string.Empty;
            NodeLevel = 0;
        }


        private void AddClassNameColumn(DataTable table)
        {
            if (!table.Columns.Contains("ClassName"))
            {
                table.Columns.Add(new DataColumn("ClassName", typeof(string)));
                table.Rows[0]["ClassName"] = NodeClassName;
            }
        }


        private void AddDataToDataTable(DataTable table, bool forceOriginalData)
        {
            var row = table.NewRow();
            table.Rows.Add(row);

            var nodeData = NodeData;
            if (forceOriginalData && IsLink)
            {
                nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(NodeLinkedNodeID);
            }

            foreach (var columnName in nodeData.ColumnNames)
            {
                SetDataRowColumn(row, columnName, nodeData.GetValue(columnName));
            }

            foreach (var columnName in CultureData.ColumnNames)
            {
                SetDataRowColumn(row, columnName, CultureData.GetValue(columnName));
            }

            if (IsCoupled)
            {
                foreach (var columnName in CoupledData.ColumnNames)
                {
                    SetDataRowColumn(row, columnName, CoupledData.GetValue(columnName));
                }
            }

            AddClassNameColumn(table);
        }

        #endregion


        #region "Components handling"

        /// <summary>
        /// Loads internal components from the given data source
        /// </summary>
        /// <param name="data">Source data to load</param>
        protected override void LoadComponentsData(IDataContainer data)
        {
            LoadCultureData(data);
            LoadNodeData(data);

            // Refresh node data class ID with current value
            RefreshNodeDataClassId();

            if (IsCoupled)
            {
                LoadCoupledData(NodeClassName, data);
            }
        }


        private void LoadCoupledData(string className, IDataContainer data)
        {
            CoupledData = new DocumentFieldsInfo(className);
            CoupledData.InitFromDataContainer(data);
        }


        private void LoadNodeData(IDataContainer data)
        {
            NodeData.InitFromDataContainer(data);
        }


        private void LoadCultureData(IDataContainer data)
        {
            CultureData = new DocumentCultureDataInfo();
            CultureData.InitFromDataContainer(data);
        }


        /// <summary>
        /// Returns <see cref="NodeID"/> for original document or <see cref="NodeLinkedNodeID"/> if the document represents a link.
        /// </summary>
        /// <remarks>This method is intended to be used in context of insertion of a document since the <see cref="OriginalNodeID"/> is not initialized yet.</remarks>
        private int GetOriginalNodeIDForInsert()
        {
            return IsLink ? NodeLinkedNodeID : NodeID;
        }


        /// <summary>
        /// Gets the list of internal components
        /// </summary>
        protected override IEnumerable<IInfo> GetComponents()
        {
            yield return NodeData;
            yield return CultureData;

            if (IsCoupled)
            {
                yield return CoupledData;
            }
        }


        /// <summary>
        /// Sets the list of internal components
        /// </summary>
        /// <param name="components">List of components</param>
        protected override void SetComponents(IEnumerable<IInfo> components)
        {
            var items = components.ToList();

            NodeData = (DocumentNodeDataInfo)items[0];
            CultureData = (DocumentCultureDataInfo)items[1];

            if (IsCoupled && (items.Count > 2))
            {
                CoupledData = (DocumentFieldsInfo)items[2];
            }
            else
            {
                CoupledData = null;
            }
        }

        #endregion
    }
}





