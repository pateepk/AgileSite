using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Xml.Serialization;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    using EditableItemsDictionary = MultiKeyDictionary<string>;

    /// <summary>
    /// Page data storage object
    /// </summary>
    [Serializable]
    public class PageInfo : ReadOnlyAbstractHierarchicalObject<PageInfo>, IPageInfo, ITreeNode, ITreeNodeMethods
    {
        #region "Variables"

        private DataSet mUpperTree;
        private DateTime mDocumentPublishTo = DateTime.MaxValue;
        private DateTime mDocumentPublishFrom = DateTime.MinValue;

        private bool? mRequiresAuthentication;
        private bool? mRequiresSSL;
        private int mRequiresSSLValue = -1;

        private int mNodeCacheMinutes = -1;
        private bool? mNodeAllowCacheInFileSystem;

        private int mDocumentStylesheetID;
        private bool? mDocumentInheritsStylesheet;
        private string mWorkflowStepName;
        private int mNodeSiteID;
        private bool? mNodeInheritPageTemplate;

        private string mNodeDocType;
        private string mNodeHeadTags;
        private string mNodeBodyElementAttributes;
        private string mNodeBodyScripts;

        private string mCurrentNodeDocType;
        private string mCurrentNodeHeadTags;
        private string mCurrentNodeBodyElementAttributes;
        private string mCurrentNodeBodyScripts;

        private string mDocumentContent;
        private string mDocumentWebParts;
        private string mDocumentGroupWebParts;

        private string mDocumentPageTitle;
        private string mDocumentPageKeyWords;
        private string mDocumentPageDescription;
        private bool? mNodeIsContentOnly;

        private WorkflowStepTypeEnum? mWorkflowStepType;

        private bool? mDocumentLogActivity;
        private PageInfoResult mPageResult;


        /// <summary>
        /// Inherited page info object.
        /// </summary>
        protected PageInfo mInheritedPageInfo;

        /// <summary>
        /// Document class name.
        /// </summary>
        protected string mClassName;

        /// <summary>
        /// Document site name.
        /// </summary>
        protected string mSiteName;

        /// <summary>
        /// Page template info object.
        /// </summary>
        protected PageTemplateInfo mPageTemplateInfo;

        /// <summary>
        /// If true, the page template info was already loaded
        /// </summary>
        protected bool mPageTemplateInfoLoaded;

        /// <summary>
        /// Editable items container.
        /// </summary>
        protected EditableItems mEditableItems;

        /// <summary>
        /// Document widgets template instance.
        /// </summary>
        protected PageTemplateInstance mDocumentTemplateInstance;

        /// <summary>
        /// Group widgets template instance.
        /// </summary>
        protected PageTemplateInstance mGroupTemplateInstance;

        /// <summary>
        /// Complete page template instance (PageTemplateInfo.TemplateInstance + DocumentTemplateInstance + GroupTemplateInstance + MVTVariants + ContentPersonalizationVariants).
        /// </summary>
        protected PageTemplateInstance mTemplateInstance;

        #endregion


        #region "Inheriting properties"

        /// <summary>
        /// Node body CSS class.
        /// </summary>
        public string NodeBodyElementAttributes
        {
            get
            {
                return (mNodeBodyElementAttributes = GetHtmlFieldValue(mNodeBodyElementAttributes, mCurrentNodeBodyElementAttributes, page => page.NodeBodyElementAttributes));
            }
            set
            {
                mNodeBodyElementAttributes = value;
            }
        }



        /// <summary>
        /// Top HTML body node for custom HTML code.
        /// </summary>
        public string NodeBodyScripts
        {
            get
            {
                return (mNodeBodyScripts = GetHtmlFieldValue(mNodeBodyScripts, mCurrentNodeBodyScripts, page => page.NodeBodyScripts));
            }
            set
            {
                mNodeBodyScripts = value;
            }
        }


        /// <summary>
        /// Node document type.
        /// </summary>
        public string NodeDocType
        {
            get
            {
                return (mNodeDocType = GetHtmlFieldValue(mNodeDocType, mCurrentNodeDocType, page => page.NodeDocType));
            }
            set
            {
                mNodeDocType = value;
            }
        }


        /// <summary>
        /// Node header tags.
        /// </summary>
        public string NodeHeadTags
        {
            get
            {
                return (mNodeHeadTags = GetHtmlFieldValue(mNodeHeadTags, mCurrentNodeHeadTags, page => page.NodeHeadTags, checkForNull: false));
            }
            set
            {
                mNodeHeadTags = value;
            }
        }


        /// <summary>
        /// Node cache minutes.
        /// </summary>
        public int NodeCacheMinutes
        {
            get
            {
                if (mNodeCacheMinutes < 0)
                {
                    // Get from the inherited page info or from upper tree
                    mNodeCacheMinutes = InheritedPageInfo != null ? InheritedPageInfo.NodeCacheMinutes : ValidationHelper.GetInteger(GetValueFromUpperTree("NodeCacheMinutes"), 0);
                }

                return mNodeCacheMinutes;
            }
            set
            {
                mNodeCacheMinutes = value;
            }
        }


        /// <summary>
        /// Document page title.
        /// </summary>
        public string DocumentPageTitle
        {
            get
            {
                if (mDocumentPageTitle == null)
                {
                    // Get from the inherited page info or from upper tree
                    mDocumentPageTitle = InheritedPageInfo != null ? InheritedPageInfo.DocumentPageTitle : ValidationHelper.GetString(GetValueFromUpperTree("DocumentPageTitle"), string.Empty);
                }

                return mDocumentPageTitle;
            }
            set
            {
                mDocumentPageTitle = value;
            }
        }


        /// <summary>
        /// Document page keywords.
        /// </summary>
        public string DocumentPageKeyWords
        {
            get
            {
                if (mDocumentPageKeyWords == null)
                {
                    // Get from the inherited page info or from upper tree
                    mDocumentPageKeyWords = InheritedPageInfo != null ? InheritedPageInfo.DocumentPageKeyWords : ValidationHelper.GetString(GetValueFromUpperTree("DocumentPageKeyWords"), string.Empty);
                }

                return mDocumentPageKeyWords;
            }
            set
            {
                mDocumentPageKeyWords = value;
            }
        }


        /// <summary>
        /// Document page description.
        /// </summary>
        public string DocumentPageDescription
        {
            get
            {
                if (mDocumentPageDescription == null)
                {
                    // Get from the inherited page info or from upper tree
                    mDocumentPageDescription = InheritedPageInfo != null ? InheritedPageInfo.DocumentPageDescription : ValidationHelper.GetString(GetValueFromUpperTree("DocumentPageDescription"), string.Empty);
                }

                return mDocumentPageDescription;
            }
            set
            {
                mDocumentPageDescription = value;
            }
        }


        /// <summary>
        /// Document CSS stylesheet ID.
        /// </summary>
        public int DocumentStylesheetID
        {
            get
            {
                if (DocumentInheritsStylesheet)
                {
                    // Get from the inherited page info or from upper tree
                    mDocumentStylesheetID = InheritedPageInfo != null ? InheritedPageInfo.DocumentStylesheetID : ValidationHelper.GetInteger(GetValueFromUpperTree("DocumentStylesheetID"), 0);
                }

                return mDocumentStylesheetID;
            }
            set
            {
                mDocumentStylesheetID = value;
            }
        }


        /// <summary>
        /// Indicates if inherits stylesheet from parent
        /// </summary>
        private bool DocumentInheritsStylesheet
        {
            get
            {
                if (!mDocumentInheritsStylesheet.HasValue)
                {
                    mDocumentInheritsStylesheet = ValidationHelper.GetBoolean(GetValue("DocumentInheritsStylesheet"), true);
                }

                return mDocumentInheritsStylesheet.Value;
            }
        }


        /// <summary>
        /// Requires authentication  property.
        /// </summary>
        public virtual bool RequiresAuthentication
        {
            get
            {
                if (mRequiresAuthentication == null)
                {
                    // Get from the inherited page info
                    mRequiresAuthentication = InheritedPageInfo != null ? InheritedPageInfo.RequiresAuthentication : ValidationHelper.GetBoolean(GetValueFromUpperTree("IsSecuredNode"), false);
                }

                return mRequiresAuthentication.Value;
            }
            set
            {
                mRequiresAuthentication = value;
            }
        }


        /// <summary>
        /// Requires authentication  property.
        /// </summary>
        public virtual bool RequiresSSL
        {
            get
            {
                if (mRequiresSSL == null)
                {
                    // Get from the inherited page info
                    mRequiresSSL = InheritedPageInfo != null ? InheritedPageInfo.RequiresSSL : ValidationHelper.GetBoolean(GetValueFromUpperTree("RequiresSSL"), false);
                }

                return mRequiresSSL.Value;
            }
            set
            {
                mRequiresSSL = value;
            }
        }


        /// <summary>
        /// Requires authentication  property.
        /// </summary>
        public virtual int RequiresSSLValue
        {
            get
            {
                if (mRequiresSSLValue == -1)
                {
                    // Get from the inherited page info or from upper tree
                    mRequiresSSLValue = InheritedPageInfo != null ? InheritedPageInfo.RequiresSSLValue : ValidationHelper.GetInteger(GetValueFromUpperTree("RequiresSSL"), 0);
                }

                return mRequiresSSLValue;
            }
            set
            {
                mRequiresSSLValue = value;
            }
        }


        /// <summary>
        /// Requires authentication  property.
        /// </summary>
        public virtual bool NodeAllowCacheInFileSystem
        {
            get
            {
                if (mNodeAllowCacheInFileSystem == null)
                {
                    // Get from the inherited page info or from upper tree
                    mNodeAllowCacheInFileSystem = InheritedPageInfo != null ? InheritedPageInfo.NodeAllowCacheInFileSystem : ValidationHelper.GetBoolean(GetValueFromUpperTree("NodeAllowCacheInFileSystem"), true);
                }

                return (bool)mNodeAllowCacheInFileSystem;
            }
            set
            {
                mNodeAllowCacheInFileSystem = value;
            }
        }


        /// <summary>
        /// Indicates if document enables logging activities.
        /// </summary>
        public virtual bool DocumentLogActivity
        {
            get
            {
                if (mDocumentLogActivity == null)
                {
                    // Get from the inherited page info or from upper tree
                    mDocumentLogActivity = InheritedPageInfo != null ? InheritedPageInfo.DocumentLogActivity : ValidationHelper.GetBoolean(GetValueFromUpperTree("DocumentLogVisitActivity"), true);
                }

                return (bool)mDocumentLogActivity;
            }
            set
            {
                mDocumentLogActivity = value;
            }
        }

        #endregion


        #region "Node properties"

        /// <summary>
        /// Node ID.
        /// </summary>
        public int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Node alias path.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Node name.
        /// </summary>
        public string NodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Node alias.
        /// </summary>
        public string NodeAlias
        {
            get;
            set;
        }


        /// <summary>
        /// Node class ID.
        /// </summary>
        public int NodeClassID
        {
            get;
            set;
        }


        /// <summary>
        /// Node parent ID.
        /// </summary>
        public int NodeParentID
        {
            get;
            set;
        }


        /// <summary>
        /// Node group ID.
        /// </summary>
        public int NodeGroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Node level.
        /// </summary>
        public int NodeLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Node ACLID.
        /// </summary>
        public int NodeACLID
        {
            get;
            set;
        }


        /// <summary>
        /// Node site ID.
        /// </summary>
        public int NodeSiteID
        {
            get
            {
                return mNodeSiteID;
            }
            set
            {
                mNodeSiteID = value;
                mSiteName = null;
            }
        }


        /// <summary>
        /// Node GUID.
        /// </summary>
        public Guid NodeGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Node order.
        /// </summary>
        public int NodeOrder
        {
            get;
            set;
        }


        /// <summary>
        /// Node inherit page levels. This property is for internal use only, use method GetUsedInheritPageLevels or SetUsedInheritPageLevels instead.
        /// </summary>
        private string NodeInheritPageLevels
        {
            get;
            set;
        }


        /// <summary>
        /// Linked node ID.
        /// </summary>
        public int NodeLinkedNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Linked node site ID.
        /// </summary>
        public int NodeLinkedNodeSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the document is link to another document.
        /// </summary>
        public bool IsLink
        {
            get
            {
                return NodeLinkedNodeID > 0;
            }
        }


        /// <summary>
        /// Original node site ID. Returns NodeSiteID for standard document, LinkedNodeSiteID for linked document.
        /// </summary>
        [XmlIgnore]
        public int OriginalNodeSiteID
        {
            get
            {
                return (NodeLinkedNodeSiteID > 0) ? NodeLinkedNodeSiteID : NodeSiteID;
            }
        }


        /// <summary>
        /// Document owner.
        /// </summary>
        public int NodeOwner
        {
            get;
            set;
        }


        /// <summary>
        /// Document product ID.
        /// </summary>
        public int NodeSKUID
        {
            get;
            set;
        }


        /// <summary>
        /// Node page template - Used for all culture versions if NodeTemplateForAllCultures is set. This property should not be used from external code, use method GetUsedPageTemplateID or SetPageTemplateID instead.
        /// </summary>
        private int NodeTemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the document uses the same template (NodeTemplateID) for all culture versions
        /// </summary>
        public virtual bool NodeTemplateForAllCultures
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the document inherits the page template from parent
        /// </summary>
        public virtual bool NodeInheritPageTemplate
        {
            get
            {
                // Get the value
                if (mNodeInheritPageTemplate == null)
                {
                    // Evaluate automatically
                    return (GetUsedPageTemplateId() <= 0);
                }

                return mNodeInheritPageTemplate.Value;
            }
            set
            {
                mNodeInheritPageTemplate = value;
            }
        }

        #endregion


        #region "Document properties"

        /// <summary>
        /// Indicates whether document should be excluded from search
        /// </summary>
        public bool DocumentSearchExcluded
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu hide in navigation.
        /// </summary>
        public bool DocumentMenuItemHideInNavigation
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu style.
        /// </summary>
        public string DocumentMenuStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu class.
        /// </summary>
        public string DocumentMenuClass
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu item inactive.
        /// </summary>
        public bool DocumentMenuItemInactive
        {
            get;
            set;
        }


        /// <summary>
        /// Document ID.
        /// </summary>
        public int DocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Document name.
        /// </summary>
        public string DocumentName
        {
            get;
            set;
        }


        /// <summary>
        /// Document name path.
        /// </summary>
        public string DocumentNamePath
        {
            get;
            set;
        }


        /// <summary>
        /// Document Redirect URL.
        /// </summary>
        public string DocumentMenuRedirectUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if redirection to first child document should be performed when accessed.
        /// </summary>
        public bool DocumentMenuRedirectToFirstChild
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu JavaScript.
        /// </summary>
        public string DocumentMenuJavascript
        {
            get;
            set;
        }


        /// <summary>
        /// Document published from.
        /// </summary>
        public DateTime DocumentPublishFrom
        {
            get
            {
                return mDocumentPublishFrom;
            }
            set
            {
                mDocumentPublishFrom = value;
            }
        }


        /// <summary>
        /// Document published to.
        /// </summary>
        public DateTime DocumentPublishTo
        {
            get
            {
                return mDocumentPublishTo;
            }
            set
            {
                mDocumentPublishTo = value;
            }
        }


        /// <summary>
        /// Document URL path.
        /// </summary>
        public string DocumentUrlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Document culture.
        /// </summary>
        public string DocumentCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Document menu caption.
        /// </summary>
        public string DocumentMenuCaption
        {
            get;
            set;
        }


        /// <summary>
        /// Document published version history ID.
        /// </summary>
        public int DocumentPublishedVersionHistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// Document checked out version history ID.
        /// </summary>
        public int DocumentCheckedOutVersionHistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// Document workflow step ID.
        /// </summary>
        public int DocumentWorkflowStepID
        {
            get;
            set;
        }


        /// <summary>
        /// Document page template ID. This property is for internal usage only and may not give proper value in case NodeTemplateForAllCultures is true, use method GetUsedPageTemplateID or SetPageTemplateID instead.
        /// </summary>
        private int DocumentPageTemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// Document extensions.
        /// </summary>
        public string DocumentExtensions
        {
            get;
            set;
        }


        /// <summary>
        /// Workflow cycle GUID to obtain preview link for document
        /// </summary>
        public Guid DocumentWorkflowCycleGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Document GUID
        /// </summary>
        public Guid DocumentGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Document conversion name - Reflects the "DocumentTrackConversionName" data column.
        /// </summary>
        public virtual string DocumentTrackConversionName
        {
            get;
            set;
        }


        /// <summary>
        /// Document conversion value - Reflects the "DocumentConversionValue" data column.
        /// </summary>
        public virtual string DocumentConversionValue
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Returns true if the node is published.
        /// </summary>
        [XmlIgnore]
        public virtual bool IsPublished
        {
            get
            {
                // Check if the document is archived, if so, do not consider published
                bool archived = (WorkflowStepType == WorkflowStepTypeEnum.DocumentArchived);
                if (archived)
                {
                    return false;
                }

                // Check published / checked out version
                bool checkedOutVersionHistoryExists = (DocumentCheckedOutVersionHistoryID > 0);
                bool publishedVersionHistoryExists = (DocumentPublishedVersionHistoryID > 0);

                if (checkedOutVersionHistoryExists != publishedVersionHistoryExists)
                {
                    return false;
                }

                // Check publishing times
                DateTime publishFrom = DocumentPublishFrom;
                DateTime publishTo = DocumentPublishTo;

                return ((DateTime.Now >= publishFrom) && (DateTime.Now <= publishTo));
            }
        }


        /// <summary>
        /// Inherited page info.
        /// </summary>
        [XmlIgnore]
        protected virtual PageInfo InheritedPageInfo
        {
            get
            {
                return mInheritedPageInfo;
            }
        }


        /// <summary>
        /// Upper tree DataSet (for inheriting of the values).
        /// </summary>
        [XmlIgnore]
        protected virtual DataSet UpperTree
        {
            get
            {
                if (mUpperTree == null)
                {
                    // Get from the inherited page info
                    mUpperTree = InheritedPageInfo != null ? InheritedPageInfo.UpperTree : GetUpperTree();
                }

                return mUpperTree;
            }
        }


        /// <summary>
        /// Workflow step name.
        /// </summary>
        [XmlIgnore]
        public virtual string WorkflowStepName
        {
            get
            {
                if (mWorkflowStepName != null)
                {
                    return mWorkflowStepName;
                }

                // Get from the inherited page info
                if (InheritedPageInfo != null)
                {
                    mWorkflowStepName = InheritedPageInfo.WorkflowStepName;
                }
                else
                {
                    // Get workflow step
                    int stepId = DocumentWorkflowStepID;
                    if (stepId > 0)
                    {
                        var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(DocumentWorkflowStepID);
                        mWorkflowStepName = (step != null) ? step.StepName : string.Empty;
                    }

                    if (mWorkflowStepName == null)
                    {
                        mWorkflowStepName = string.Empty;
                    }
                }

                return mWorkflowStepName;
            }
        }


        /// <summary>
        /// Workflow step type.
        /// </summary>
        [XmlIgnore]
        public virtual WorkflowStepTypeEnum WorkflowStepType
        {
            get
            {
                if (mWorkflowStepType != null)
                {
                    return mWorkflowStepType.Value;
                }

                // Get from the inherited page info
                if (InheritedPageInfo != null)
                {
                    mWorkflowStepType = InheritedPageInfo.WorkflowStepType;
                }
                else
                {
                    // Get workflow step
                    int stepId = DocumentWorkflowStepID;
                    if (stepId > 0)
                    {
                        // Get step type
                        var step = WorkflowStepInfoProvider.GetWorkflowStepInfo(DocumentWorkflowStepID);
                        mWorkflowStepType = (step != null) ? step.StepType : WorkflowStepTypeEnum.Undefined;
                    }

                    if (mWorkflowStepType == null)
                    {
                        mWorkflowStepType = WorkflowStepTypeEnum.Undefined;
                    }
                }

                return mWorkflowStepType.Value;
            }
        }


        /// <summary>
        /// Class name of the current document.
        /// </summary>
        public virtual string ClassName
        {
            get
            {
                return mClassName ?? (mClassName = DataClassInfoProvider.GetClassName(NodeClassID));
            }
            set
            {
                mClassName = value;
            }
        }


        /// <summary>
        /// Site name of the current document.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return mSiteName ?? (mSiteName = SiteInfoProvider.GetSiteName(NodeSiteID));
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Node site name.
        /// </summary>
        public string NodeSiteName
        {
            get
            {
                return SiteName;
            }
        }


        /// <summary>
        /// Gets or sets the contextual page result data object related to the current url.
        /// PageResult returns different data for different document aliases.
        /// </summary>
        public PageInfoResult PageResult
        {
            get
            {
                PageInfoResult current = null;

                if (mPageResult != null)
                {
                    // Get the page result data for the current url
                    current = mPageResult.GetCurrentResult(DocumentContext.PageResultUrlPath);
                }

                return current ?? new PageInfoResult();
            }
        }

        #endregion


        #region "Advanced properties - nested objects"

        /// <summary>
        /// Page template info used for standard and design view of the document.
        /// </summary>
        [XmlIgnore]
        public virtual PageTemplateInfo DesignPageTemplateInfo
        {
            get
            {
                // Ensure loading of the page template info
                return EnsurePageTemplateInfo();
            }
            set
            {
                mPageTemplateInfo = value;
            }
        }


        /// <summary>
        /// Page template info used by current view mode.
        /// </summary>
        [XmlIgnore]
        public virtual PageTemplateInfo UsedPageTemplateInfo
        {
            get
            {
                return DesignPageTemplateInfo;
            }
            set
            {
                DesignPageTemplateInfo = value;
            }
        }


        /// <summary>
        /// Editable items contained within the page.
        /// </summary>
        [XmlIgnore]
        public virtual EditableItems EditableItems
        {
            get
            {
                if (mEditableItems == null)
                {
                    // Load the items
                    var items = new EditableItems();
                    items.LoadContentXml(mDocumentContent);

                    mEditableItems = items;
                }

                return mEditableItems;
            }
        }


        /// <summary>
        /// Child page info (for the page path of the Portal engine controls).
        /// </summary>
        [XmlIgnore]
        public virtual PageInfo ChildPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Parent page info (for the page path), this property is meant for internal purposes of Portal engine.
        /// </summary>
        [XmlIgnore]
        public virtual PageInfo ParentPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Editable regions contained within the document.
        /// </summary>
        [XmlIgnore]
        public virtual EditableItemsDictionary EditableRegions
        {
            get
            {
                return EditableItems.EditableRegions;
            }
        }


        /// <summary>
        /// Editable WebParts contained within the document.
        /// </summary>
        [XmlIgnore]
        public virtual EditableItemsDictionary EditableWebParts
        {
            get
            {
                return EditableItems.EditableWebParts;
            }
        }


        /// <summary>
        /// Returns document content XML.
        /// </summary>
        [XmlIgnore]
        public virtual string DocumentContent
        {
            get
            {
                return GetContentXml();
            }
        }


        /// <summary>
        /// Document widgets template instance.
        /// </summary>
        [XmlIgnore]
        public PageTemplateInstance DocumentTemplateInstance
        {
            get
            {
                return EnsureDocumentTemplateInstance();
            }
            set
            {
                mDocumentTemplateInstance = value;
            }
        }


        /// <summary>
        /// Group widgets template instance.
        /// </summary>
        [XmlIgnore]
        public PageTemplateInstance GroupTemplateInstance
        {
            get
            {
                if (mGroupTemplateInstance == null)
                {
                    // Prepare the template instance
                    mGroupTemplateInstance = new PageTemplateInstance(mDocumentGroupWebParts);
                }
                return mGroupTemplateInstance;
            }
            set
            {
                mGroupTemplateInstance = value;
            }
        }


        /// <summary>
        /// Complete page template instance (PageTemplateInfo.TemplateInstance + DocumentTemplateInstance + GroupTemplateInstance).
        /// </summary>
        [XmlIgnore]
        public PageTemplateInstance TemplateInstance
        {
            get
            {
                return EnsurePageTemplateInstance();
            }
            set
            {
                mTemplateInstance = value;
            }
        }


        /// <summary>
        /// Indicates if document context available.
        /// </summary>
        [XmlIgnore]
        public bool IsDocument
        {
            get
            {
                return NodeID > 0;
            }
        }

        #endregion


        #region "Constructors and cloning"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PageInfo()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Container with the data (CMS_Tree_Joined + CMS_PageTemplate)</param>
        public PageInfo(IDataContainer container)
        {
            // If no data given, create blank class info
            if (container == null)
            {
                return;
            }

            LoadVersion(container, false);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dr">Data row with the data (CMS_Tree_Joined + CMS_PageTemplate)</param>
        public PageInfo(DataRow dr)
            : this(new DataRowContainer(dr))
        {
        }


        /// <summary>
        /// Creates an inherited page info.
        /// </summary>
        public PageInfo CreateInherited()
        {
            // Create new object
            PageInfo newInfo = Clone(false, false);
            newInfo.mInheritedPageInfo = this;

            return newInfo;
        }


        /// <summary>
        /// Clones the PageInfo object.
        /// </summary>
        public PageInfo Clone()
        {
            return Clone(false, true);
        }


        /// <summary>
        /// Clones the PageInfo object.
        /// </summary>
        /// <param name="cloneTemplate">If true, page template object is cloned</param>
        /// <param name="cloneEditableItems">Clone editable items</param>
        public PageInfo Clone(bool cloneTemplate, bool cloneEditableItems)
        {
            var clone = new PageInfo();

            // Document data
            clone.DocumentCheckedOutVersionHistoryID = DocumentCheckedOutVersionHistoryID;
            clone.DocumentPublishedVersionHistoryID = DocumentPublishedVersionHistoryID;
            clone.mDocumentContent = mDocumentContent;
            clone.DocumentCulture = DocumentCulture;
            clone.DocumentID = DocumentID;

            clone.DocumentMenuCaption = DocumentMenuCaption;
            clone.DocumentMenuClass = DocumentMenuClass;
            clone.DocumentMenuItemHideInNavigation = DocumentMenuItemHideInNavigation;
            clone.DocumentMenuItemInactive = DocumentMenuItemInactive;
            clone.DocumentMenuJavascript = DocumentMenuJavascript;
            clone.DocumentMenuRedirectUrl = DocumentMenuRedirectUrl;
            clone.DocumentMenuRedirectToFirstChild = DocumentMenuRedirectToFirstChild;
            clone.DocumentMenuStyle = DocumentMenuStyle;

            clone.DocumentName = DocumentName;
            clone.DocumentNamePath = DocumentNamePath;

            clone.mDocumentPageDescription = mDocumentPageDescription;
            clone.mDocumentPageKeyWords = mDocumentPageKeyWords;
            clone.DocumentPageTemplateID = DocumentPageTemplateID;
            clone.mDocumentPageTitle = mDocumentPageTitle;
            clone.mDocumentPublishFrom = mDocumentPublishFrom;
            clone.mDocumentPublishTo = mDocumentPublishTo;
            clone.mDocumentStylesheetID = mDocumentStylesheetID;
            clone.mDocumentInheritsStylesheet = mDocumentInheritsStylesheet;
            clone.DocumentUrlPath = DocumentUrlPath;
            clone.DocumentWorkflowStepID = DocumentWorkflowStepID;
            clone.DocumentExtensions = DocumentExtensions;

            clone.DocumentConversionValue = DocumentConversionValue;
            clone.DocumentTrackConversionName = DocumentTrackConversionName;

            clone.DocumentWorkflowCycleGUID = DocumentWorkflowCycleGUID;
            clone.DocumentGUID = DocumentGUID;

            clone.DocumentSearchExcluded = DocumentSearchExcluded;
            clone.mDocumentLogActivity = mDocumentLogActivity;

            // Node data
            clone.NodeACLID = NodeACLID;
            clone.NodeAlias = NodeAlias;
            clone.NodeAliasPath = NodeAliasPath;

            clone.mNodeCacheMinutes = mNodeCacheMinutes;
            clone.mNodeAllowCacheInFileSystem = mNodeAllowCacheInFileSystem;

            clone.NodeClassID = NodeClassID;
            clone.NodeTemplateID = NodeTemplateID;
            clone.NodeTemplateForAllCultures = NodeTemplateForAllCultures;
            clone.mNodeInheritPageTemplate = mNodeInheritPageTemplate;

            clone.mNodeBodyElementAttributes = mNodeBodyElementAttributes;
            clone.mNodeBodyScripts = mNodeBodyScripts;
            clone.mNodeDocType = mNodeDocType;
            clone.mNodeHeadTags = mNodeHeadTags;

            clone.mCurrentNodeBodyElementAttributes = mCurrentNodeBodyElementAttributes;
            clone.mCurrentNodeBodyScripts = mCurrentNodeBodyScripts;
            clone.mCurrentNodeDocType = mCurrentNodeDocType;
            clone.mCurrentNodeHeadTags = mCurrentNodeHeadTags;

            clone.NodeGUID = NodeGUID;
            clone.NodeID = NodeID;
            clone.NodeInheritPageLevels = NodeInheritPageLevels;
            clone.NodeLevel = NodeLevel;
            clone.NodeLinkedNodeID = NodeLinkedNodeID;
            clone.NodeLinkedNodeSiteID = NodeLinkedNodeSiteID;
            clone.NodeOwner = NodeOwner;
            clone.NodeName = NodeName;
            clone.NodeOrder = NodeOrder;
            clone.NodeParentID = NodeParentID;
            clone.NodeGroupID = NodeGroupID;
            clone.mNodeSiteID = mNodeSiteID;
            clone.NodeSKUID = NodeSKUID;

            // Authentication information
            clone.mRequiresAuthentication = mRequiresAuthentication;
            clone.mRequiresSSL = mRequiresSSL;

            // Other properties
            clone.mInheritedPageInfo = mInheritedPageInfo;
            clone.ChildPageInfo = ChildPageInfo;
            clone.ParentPageInfo = ParentPageInfo;
            clone.mClassName = mClassName;
            clone.mWorkflowStepName = mWorkflowStepName;
            clone.mWorkflowStepType = mWorkflowStepType;

            clone.mUpperTree = mUpperTree;

            clone.mDocumentWebParts = mDocumentWebParts;
            clone.mDocumentGroupWebParts = mDocumentGroupWebParts;

            // Page template
            if (cloneTemplate)
            {
                if (mPageTemplateInfo != null)
                {
                    clone.mPageTemplateInfo = mPageTemplateInfo.Clone();
                    clone.mPageTemplateInfo.ParentPageInfo = clone;
                }
            }
            else
            {
                clone.mPageTemplateInfo = mPageTemplateInfo;
            }

            // Editable items
            if (cloneEditableItems && (mEditableItems != null))
            {
                clone.mEditableItems = mEditableItems.Clone();
            }
            else
            {
                clone.mEditableItems = mEditableItems;
            }

            return clone;
        }

        #endregion


        #region "Page template methods"

        /// <summary>
        /// Ensures the loading of the combined page template instance
        /// </summary>
        private PageTemplateInstance EnsurePageTemplateInstance()
        {
            if ((mTemplateInstance == null) && (UsedPageTemplateInfo != null))
            {
                lock (this)
                {
                    if (mTemplateInstance == null)
                    {
                        // Clone base instance (from page template) together with macro table 
                        // to be able to identify properties containing macros
                        var ti = UsedPageTemplateInfo.TemplateInstance.Clone(true);

                        // Raise the page template combine event
                        using (var h = PageInfoEvents.CombinePageTemplateInstance.StartEvent(this, ti))
                        {
                            if (h.CanContinue())
                            {
                                ti = h.EventArguments.PageTemplateInstance;

                                // Combine with editor instance (from document)
                                ti.CombineWith(DocumentTemplateInstance, WidgetZoneTypeEnum.Editor);

                                // Combine with group instance (from document)
                                ti.CombineWith(GroupTemplateInstance, WidgetZoneTypeEnum.Group);
                            }

                            h.FinishEvent();

                            // Use the potentially new template instance from handler
                            ti = h.EventArguments.PageTemplateInstance;
                        }

                        mTemplateInstance = ti;
                    }
                }
            }

            return mTemplateInstance;
        }


        /// <summary>
        /// Ensures the loading of the document template instance
        /// </summary>
        private PageTemplateInstance EnsureDocumentTemplateInstance()
        {
            if (mDocumentTemplateInstance == null)
            {
                lock (this)
                {
                    if (mDocumentTemplateInstance == null)
                    {
                        string dWebParts = null;
                        if (PortalContext.ViewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditLive))
                        {
                            // Try to get the editor widgets from the temporary interlayer
                            dWebParts = PortalContext.GetEditorWidgets(DocumentID);
                        }

                        if (dWebParts != null)
                        {
                            // Prepare the template instance using the temporary interlayer
                            mDocumentTemplateInstance = new PageTemplateInstance(dWebParts);
                        }
                        else
                        {
                            // Prepare the template instance
                            mDocumentTemplateInstance = new PageTemplateInstance(mDocumentWebParts);
                        }

                        if (mTemplateInstance != null)
                        {
                            mDocumentTemplateInstance.ParentPageTemplate = mTemplateInstance.ParentPageTemplate;
                        }
                        else
                        {
                            // Base instance (from page template)
                            mDocumentTemplateInstance.ParentPageTemplate = EnsurePageTemplateInfo();
                        }

                        // If the cached template has not loaded MVT/Content personalization variants yet, then load them now
                        if (PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled)
                        {
                            // Load document widget variants
                            foreach (WebPartZoneInstance zone in mDocumentTemplateInstance.WebPartZones)
                            {
                                // Load the widget variants only for the editor zones
                                if (zone.WidgetZoneType == WidgetZoneTypeEnum.Editor)
                                {
                                    foreach (WebPartInstance widget in zone.WebParts)
                                    {
                                        // Load widget variants
                                        widget.LoadVariants(false, VariantModeEnum.None, DocumentID);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return mDocumentTemplateInstance;
        }


        /// <summary>
        /// Ensures that the Page template info object is loaded
        /// </summary>
        private PageTemplateInfo EnsurePageTemplateInfo()
        {
            if ((mPageTemplateInfo == null) && !mPageTemplateInfoLoaded)
            {
                lock (this)
                {
                    if (!mPageTemplateInfoLoaded)
                    {
                        // Get from the inherited page info
                        if (InheritedPageInfo != null)
                        {
                            mPageTemplateInfo = InheritedPageInfo.UsedPageTemplateInfo;
                        }
                        else
                        {
                            if (NodeInheritPageTemplate)
                            {
                                // Inherit page template from parent
                                mPageTemplateInfo = GetInheritedTemplateInfo(DocumentCulture, SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSCombineWithDefaultCulture"));
                            }
                            else
                            {
                                // Use current page info template
                                int templateId = GetUsedPageTemplateId();

                                mPageTemplateInfo = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                            }

                            if (mPageTemplateInfo == null)
                            {
                                // Empty page template
                                mPageTemplateInfo = new PageTemplateInfo();
                            }

                            mPageTemplateInfo.ParentPageInfo = this;
                        }

                        mPageTemplateInfoLoaded = true;
                    }
                }
            }

            return mPageTemplateInfo;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets resolved document page title
        /// </summary>
        public string GetResolvedPageTitle()
        {
            // Get default site Title
            string prefix = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSPageTitlePrefix");
            string format = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSPageTitleFormat");

            string titleOrName = DataHelper.GetNotEmpty(DocumentPageTitle, GetDocumentName());

            // Get resolver
            MacroResolver resolver = DocumentContext.CurrentResolver.CreateChild();

            resolver.SetAnonymousSourceData(this);
            resolver.SetNamedSourceData(new Dictionary<string, object>
            {
                { "prefix", prefix },
                { "pagetitle_orelse_name", titleOrName }
            }, false);

            resolver.Settings.AllowRecursion = true;
            resolver.Settings.AvoidInjection = false;

            // Resolve macros
            return resolver.ResolveMacros(format);
        }


        /// <summary>
        /// Gets the upper tree data for inherited values.
        /// </summary>
        public DataSet GetUpperTree()
        {
            return TreePathUtils.GetNodeUpperTreeInternal(NodeSiteID, NodeAliasPath, null, DocumentCulture);
        }


        /// <summary>
        /// Returns string value of selected column.
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="checkForNull">If is true, check for NULL value, else check for "-1" value</param>
        /// <param name="onlyMaster">Indicates whether only master page value should be returned</param>
        public object GetValueFromUpperTree(string column, bool checkForNull = true, bool onlyMaster = false)
        {
            // Use special value checker
            Func<object, bool> checker = null;
            if (!checkForNull)
            {
                checker = (v => ValidationHelper.GetInteger(v, int.MaxValue) > 0);
            }

            // Use special data evaluator
            Func<DataRow, bool> evaluator = null;
            if (onlyMaster)
            {
                evaluator = (data =>
                {
                    // Root is always master page
                    if (DataHelper.GetStringValue(data, "NodeAliasPath", "/") == "/")
                    {
                        return true;
                    }

                    // Try get current page template and check whether is set as master template
                    bool allCultures = DataHelper.GetBoolValue(data, "NodeTemplateForAllCultures");
                    string templateColumn = (allCultures ? "NodeTemplateID" : "DocumentPageTemplateID");

                    int templateId = DataHelper.GetIntValue(data, templateColumn);
                    if (templateId > 0)
                    {
                        PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                        if ((pti != null) && pti.ShowAsMasterTemplate)
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return TreePathUtils.GetNodeInheritedValueInternal(UpperTree, column, DocumentCulture, checker, evaluator);
        }


        /// <summary>
        /// Load the current version data of the document to the page info.
        /// </summary>
        public void LoadVersion()
        {
            // Only if there is a version available
            if (DocumentCheckedOutVersionHistoryID <= 0)
            {
                return;
            }

            // Get version data
            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(DocumentCheckedOutVersionHistoryID);
            if (version == null)
            {
                return;
            }

            var node = TreeNode.New<TreeNode>(null, version.Data);

            // Load only versioned data
            LoadVersion(node, true);
        }


        /// <summary>
        /// Load the given version of the document data to the page info.
        /// </summary>
        /// <param name="data">Data to load</param>
        /// <param name="loadVersionedDataOnly">Indicates if only versioned data should be loaded</param>
        internal void LoadVersion(IDataContainer data, bool loadVersionedDataOnly)
        {
            if (data == null)
            {
                return;
            }

            LoadNodeData(data, loadVersionedDataOnly);
            LoadDocumentData(data, loadVersionedDataOnly);
            LoadSKUData(data);

            // Load master page data if necessary
            LoadMasterPageData(data, loadVersionedDataOnly);

            // Load inherited values
            LoadInheritedData(data, loadVersionedDataOnly);
        }


        /// <summary>
        /// Load complete given version of the document to the page info.
        /// </summary>
        /// <param name="node">Node to load</param>
        public void LoadVersion(TreeNode node)
        {
            LoadVersion(node, false);
        }


        /// <summary>
        /// Loads node data
        /// </summary>
        /// <param name="data">Data to load</param>
        /// <param name="loadVersionedDataOnly">Indicates if only versioned data should be loaded</param>
        private void LoadNodeData(IDataContainer data, bool loadVersionedDataOnly)
        {
            // Do not load versioned data if not required
            if (loadVersionedDataOnly)
            {
                return;
            }

            NodeID = ValidationHelper.GetInteger(data.GetValue("NodeID"), 0);
            NodeAliasPath = ValidationHelper.GetString(data.GetValue("NodeAliasPath"), string.Empty);
            NodeName = ValidationHelper.GetString(data.GetValue("NodeName"), string.Empty);
            NodeAlias = ValidationHelper.GetString(data.GetValue("NodeAlias"), string.Empty);
            NodeClassID = ValidationHelper.GetInteger(data.GetValue("NodeClassID"), 0);
            mClassName = ValidationHelper.GetString(data.GetValue("ClassName"), null);
            NodeParentID = ValidationHelper.GetInteger(data.GetValue("NodeParentID"), 0);
            NodeLevel = ValidationHelper.GetInteger(data.GetValue("NodeLevel"), 0);
            NodeSiteID = ValidationHelper.GetInteger(data.GetValue("NodeSiteID"), 0);

            var groupId = data.GetValue("NodeGroupID");
            NodeGroupID = groupId == null ? 0 : ValidationHelper.GetInteger(groupId, 0);

            var aclid = data.GetValue("NodeACLID");
            NodeACLID = aclid == null ? 0 : ValidationHelper.GetInteger(aclid, 0);

            var nodeGuid = data.GetValue("NodeGUID");
            NodeGUID = nodeGuid == null ? Guid.Empty : ValidationHelper.GetGuid(nodeGuid, Guid.Empty);

            var nodeOrder = data.GetValue("NodeOrder");
            NodeOrder = nodeOrder == null ? 0 : ValidationHelper.GetInteger(nodeOrder, 0);

            NodeInheritPageLevels = ValidationHelper.GetString(data.GetValue("NodeInheritPageLevels"), string.Empty);

            NodeLinkedNodeID = ValidationHelper.GetInteger(data.GetValue("NodeLinkedNodeID"), 0);
            NodeLinkedNodeSiteID = ValidationHelper.GetInteger(data.GetValue("NodeLinkedNodeSiteID"), 0);

            NodeOwner = ValidationHelper.GetInteger(data.GetValue("NodeOwner"), 0);

            NodeTemplateID = ValidationHelper.GetInteger(data.GetValue("NodeTemplateID"), 0);
            NodeTemplateForAllCultures = ValidationHelper.GetBoolean(data.GetValue("NodeTemplateForAllCultures"), false);

            var inheritPT = data.GetValue("NodeInheritPageTemplate");
            if (DataHelper.GetNull(inheritPT) == null)
            {
                mNodeInheritPageTemplate = null;
            }
            else
            {
                mNodeInheritPageTemplate = ValidationHelper.GetBoolean(inheritPT, false);
            }
        }


        /// <summary>
        /// Loads SKU data
        /// </summary>
        /// <param name="data">Data to load</param>
        private void LoadSKUData(IDataContainer data)
        {
            var nodeSKUId = data.GetValue("NodeSKUID");
            NodeSKUID = nodeSKUId == null ? 0 : ValidationHelper.GetInteger(nodeSKUId, 0);
        }


        /// <summary>
        /// Loads culture specific data
        /// </summary>
        /// <param name="data">Data to load</param>
        /// <param name="loadVersionedDataOnly">Indicates if only versioned data should be loaded</param>
        private void LoadDocumentData(IDataContainer data, bool loadVersionedDataOnly)
        {
            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentID"))
            {
                DocumentID = ValidationHelper.GetInteger(data.GetValue("DocumentID"), 0);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentName"))
            {
                DocumentName = ValidationHelper.GetString(data.GetValue("DocumentName"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentCulture"))
            {
                var culture = data.GetValue("DocumentCulture");
                DocumentCulture = culture == null ? string.Empty : ValidationHelper.GetString(culture, string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentNamePath"))
            {
                var namePath = data.GetValue("DocumentNamePath");
                DocumentNamePath = namePath == null ? string.Empty : ValidationHelper.GetString(namePath, string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentUrlPath"))
            {
                var urlPath = data.GetValue("DocumentUrlPath");
                DocumentUrlPath = urlPath == null ? string.Empty : ValidationHelper.GetString(urlPath, string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuCaption"))
            {
                var caption = data.GetValue("DocumentMenuCaption");
                DocumentMenuCaption = caption == null ? string.Empty : ValidationHelper.GetString(caption, string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuRedirectUrl"))
            {
                DocumentMenuRedirectUrl = ValidationHelper.GetString(data.GetValue("DocumentMenuRedirectUrl"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuRedirectToFirstChild"))
            {
                DocumentMenuRedirectToFirstChild = ValidationHelper.GetBoolean(data.GetValue("DocumentMenuRedirectToFirstChild"), false);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuJavascript"))
            {
                DocumentMenuJavascript = ValidationHelper.GetString(data.GetValue("DocumentMenuJavascript"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuItemInactive"))
            {
                DocumentMenuItemInactive = ValidationHelper.GetBoolean(data.GetValue("DocumentMenuItemInactive"), false);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuItemHideInNavigation"))
            {
                DocumentMenuItemHideInNavigation = ValidationHelper.GetBoolean(data.GetValue("DocumentMenuItemHideInNavigation"), false);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuStyle"))
            {
                DocumentMenuStyle = ValidationHelper.GetString(data.GetValue("DocumentMenuStyle"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentMenuClass"))
            {
                DocumentMenuClass = ValidationHelper.GetString(data.GetValue("DocumentMenuClass"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPublishFrom"))
            {
                mDocumentPublishFrom = ValidationHelper.GetDateTime(data.GetValue("DocumentPublishFrom"), DateTime.MinValue);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPublishTo"))
            {
                mDocumentPublishTo = ValidationHelper.GetDateTime(data.GetValue("DocumentPublishTo"), DateTime.MaxValue);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentWorkflowStepID"))
            {
                DocumentWorkflowStepID = ValidationHelper.GetInteger(data.GetValue("DocumentWorkflowStepID"), 0);
                mWorkflowStepName = null;
                mWorkflowStepType = null;
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentCheckedOutVersionHistoryID"))
            {
                DocumentCheckedOutVersionHistoryID = ValidationHelper.GetInteger(data.GetValue("DocumentCheckedOutVersionHistoryID"), 0);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPublishedVersionHistoryID"))
            {
                DocumentPublishedVersionHistoryID = ValidationHelper.GetInteger(data.GetValue("DocumentPublishedVersionHistoryID"), 0);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPageTemplateID"))
            {
                DocumentPageTemplateID = ValidationHelper.GetInteger(data.GetValue("DocumentPageTemplateID"), 0);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentExtensions"))
            {
                DocumentExtensions = ValidationHelper.GetString(data.GetValue("DocumentExtensions"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentConversionValue"))
            {
                DocumentConversionValue = ValidationHelper.GetString(data.GetValue("DocumentConversionValue"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentTrackConversionName"))
            {
                DocumentTrackConversionName = ValidationHelper.GetString(data.GetValue("DocumentTrackConversionName"), string.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentWorkflowCycleGUID"))
            {
                DocumentWorkflowCycleGUID = ValidationHelper.GetGuid(data.GetValue("DocumentWorkflowCycleGUID"), Guid.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentGUID"))
            {
                DocumentGUID = ValidationHelper.GetGuid(data.GetValue("DocumentGUID"), Guid.Empty);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentSearchExcluded"))
            {
                DocumentSearchExcluded = ValidationHelper.GetBoolean(data.GetValue("DocumentSearchExcluded"), false);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentContent"))
            {
                mDocumentContent = ValidationHelper.GetString(data.GetValue("DocumentContent"), string.Empty);

                // Clear cached instances
                mEditableItems = null;
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentWebParts"))
            {
                mDocumentWebParts = ValidationHelper.GetString(data.GetValue("DocumentWebParts"), string.Empty);

                // Clear cached instances
                mDocumentTemplateInstance = null;
                mTemplateInstance = null;
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentGroupWebParts"))
            {
                mDocumentGroupWebParts = ValidationHelper.GetString(data.GetValue("DocumentGroupWebParts"), string.Empty);

                // Clear cached instances
                mGroupTemplateInstance = null;
                mTemplateInstance = null;
            }

            // Special columns treatment
            DocumentNamePath = TreePathUtils.GetParentPath(DocumentNamePath).TrimEnd('/') + "/" + TreePathUtils.GetSafeDocumentName(DocumentName, SiteName);
        }


        /// <summary>
        /// Loads inherited data
        /// </summary>
        /// <param name="data">Data to load</param>
        /// <param name="loadVersionedDataOnly">Indicates if only versioned data should be loaded</param>
        private void LoadInheritedData(IDataContainer data, bool loadVersionedDataOnly)
        {
            if (!loadVersionedDataOnly)
            {
                mNodeCacheMinutes = ValidationHelper.GetInteger(data.GetValue("NodeCacheMinutes"), -1);

                var isSecured = data.GetValue("IsSecuredNode");
                if (DataHelper.GetNull(isSecured) == null)
                {
                    mRequiresAuthentication = null;
                }
                else
                {
                    mRequiresAuthentication = ValidationHelper.GetBoolean(isSecured, false);
                }

                var requireSSL = data.GetValue("RequiresSSL");
                if (DataHelper.GetNull(requireSSL) == null)
                {
                    mRequiresSSL = null;
                    mRequiresSSLValue = -1;
                }
                else
                {
                    mRequiresSSL = ValidationHelper.GetBoolean(requireSSL, false);
                    mRequiresSSLValue = ValidationHelper.GetInteger(requireSSL, -1);
                }

                var cacheInFS = data.GetValue("NodeAllowCacheInFileSystem");
                if (DataHelper.GetNull(cacheInFS) == null)
                {
                    mNodeAllowCacheInFileSystem = null;
                }
                else
                {
                    mNodeAllowCacheInFileSystem = ValidationHelper.GetBoolean(cacheInFS, true);
                }
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPageTitle"))
            {
                mDocumentPageTitle = ValidationHelper.GetString(data.GetValue("DocumentPageTitle"), null);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPageKeyWords"))
            {
                mDocumentPageKeyWords = ValidationHelper.GetString(data.GetValue("DocumentPageKeyWords"), null);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentPageDescription"))
            {
                mDocumentPageDescription = ValidationHelper.GetString(data.GetValue("DocumentPageDescription"), null);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentStylesheetID"))
            {
                mDocumentStylesheetID = ValidationHelper.GetInteger(data.GetValue("DocumentStylesheetID"), 0);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentInheritsStylesheet"))
            {
                mDocumentInheritsStylesheet = ValidationHelper.GetBoolean(data.GetValue("DocumentInheritsStylesheet"), true);
            }

            if (!loadVersionedDataOnly || VersionManager.IsVersionedDocumentColumn("DocumentLogVisitActivity"))
            {
                var logVisit = data.GetValue("DocumentLogVisitActivity");
                if (DataHelper.GetNull(logVisit) == null)
                {
                    mDocumentLogActivity = null;
                }
                else
                {
                    mDocumentLogActivity = ValidationHelper.GetBoolean(logVisit, false);
                }
            }
        }


        /// <summary>
        /// Loads data from master page level
        /// </summary>
        /// <param name="data">Data to load</param>
        /// <param name="loadVersionedDataOnly">Indicates if only versioned data should be loaded</param>
        private void LoadMasterPageData(IDataContainer data, bool loadVersionedDataOnly)
        {
            if (loadVersionedDataOnly)
            {
                return;
            }

            // Load current values
            mCurrentNodeDocType = ValidationHelper.GetString(data.GetValue("NodeDocType"), String.Empty);
            mCurrentNodeBodyElementAttributes = ValidationHelper.GetString(data.GetValue("NodeBodyElementAttributes"), String.Empty);
            mCurrentNodeBodyScripts = ValidationHelper.GetString(data.GetValue("NodeBodyScripts"), String.Empty);
            mCurrentNodeHeadTags = ValidationHelper.GetString(data.GetValue("NodeHeadTags"), String.Empty);
        }


        /// <summary>
        /// Indicates if current page uses master page template
        /// </summary>
        private bool UsesMasterPageTemplate()
        {
            // Root is always master page
            if (string.Equals(NodeAliasPath, "/", StringComparison.Ordinal))
            {
                return true;
            }

            // Check whether current template is set as master page
            int templateId = GetUsedPageTemplateId();

            // Try get template ID from inherited template
            if (templateId <= 0)
            {
                var inherited = UsedPageTemplateInfo;
                if (inherited != null)
                {
                    templateId = inherited.PageTemplateId;
                }
            }

            // No template used
            if (templateId <= 0)
            {
                return false;
            }

            // Check template if master page template
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
            return (template != null) && template.ShowAsMasterTemplate;
        }


        /// <summary>
        /// Gets the inherited template info from parent page infos
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        protected PageTemplateInfo GetInheritedTemplateInfo(string cultureCode, bool combineWithDefaultCulture)
        {
            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(NodeSiteID);
            if (si == null)
            {
                throw new Exception("[PageInfo.GetInheritedTemplateInfo]: Node site not found.");
            }

            PageInfo pi = this;

            // Go through all parent templates
            while ((pi != null) && !pi.IsRoot())
            {
                string aliasPath = DataHelper.GetParentPath(pi.NodeAliasPath);

                // Get parent page info
                pi = PageInfoProvider.GetPageInfo(si.SiteName, aliasPath, cultureCode, null, combineWithDefaultCulture);
                if (pi != null)
                {
                    // Check the template ID
                    int templateId = pi.GetUsedPageTemplateId();
                    if (templateId > 0)
                    {
                        return pi.UsedPageTemplateInfo;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the page dependency keys for the response of the page.
        /// </summary>
        public string[] GetResponseCacheDependencies()
        {
            int level = NodeLevel;

            string[] result = new string[8 + level];

            int siteId = NodeSiteID;
            int groupId = NodeGroupID;

            int i = 0;

            // Full page cache keys
            string baseKey = CacheHelper.FULLPAGE_KEY;

            // fullpage
            result[i++] = baseKey;

            // fullpage|<sitename>
            result[i++] = baseKey + "|" + SiteName;

            // fullpage|<siteid>|<aliaspath>
            baseKey += "|" + siteId;

            string key = baseKey + "|" + NodeAliasPath.ToLowerInvariant();
            result[i++] = key;

            // fullpage|<siteid>|<aliaspath>|<culture>
            result[i++] = key + "|" + DocumentCulture.ToLowerInvariant();

            // Add child nodes dependencies
            if (level > 0)
            {
                string path = TreePathUtils.GetParentPath(NodeAliasPath);
                while (level > 0)
                {
                    // Crete dependency key
                    result[i++] = baseKey + "|" + path.ToLowerInvariant() + "|childnodes";

                    // Root is maximal level to process => stop cycle
                    if (path == "/")
                    {
                        break;
                    }

                    // Get parent path
                    path = TreePathUtils.GetParentPath(path);
                    level--;
                }
            }

            // Document relation keys
            result[i++] = "nodeid|" + NodeID;
            result[i++] = "documentid|" + DocumentID;

            // Dependency on page template
            PageTemplateInfo pti = UsedPageTemplateInfo;
            if (pti != null)
            {
                result[i++] = "template|" + pti.PageTemplateId;
            }

            // Add community group dependency
            if (groupId > 0)
            {
                result[i] = "community.group|byid|" + groupId;
            }

            return result;
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


        /// <summary>
        /// Registers the Columns of this object
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn<int>("DocumentID", m => m.DocumentID);
            RegisterColumn("DocumentName", m => m.GetDocumentName());
            RegisterColumn("DocumentNamePath", m => m.DocumentNamePath);
            RegisterColumn<DateTime>("DocumentPublishTo", m => m.DocumentPublishTo);
            RegisterColumn<DateTime>("DocumentPublishFrom", m => m.DocumentPublishFrom);
            RegisterColumn("DocumentURLPath", m => m.DocumentUrlPath);
            RegisterColumn("DocumentCulture", m => m.DocumentCulture);
            RegisterColumn<int>("NodeSKUID", m => m.NodeSKUID);
            RegisterColumn("DocumentMenuCaption", m => m.DocumentMenuCaption);
            RegisterColumn("DocumentMenuRedirectURL", m => m.DocumentMenuRedirectUrl);
            RegisterColumn("DocumentMenuRedirectToFirstChild", m => m.DocumentMenuRedirectToFirstChild);
            RegisterColumn("DocumentMenuJavascript", m => m.DocumentMenuJavascript);
            RegisterColumn<bool>("DocumentMenuItemHideInNavigation", m => m.DocumentMenuItemHideInNavigation);
            RegisterColumn("DocumentMenuStyle", m => m.DocumentMenuStyle);
            RegisterColumn("DocumentMenuClass", m => m.DocumentMenuClass);
            RegisterColumn<bool>("DocumentMenuItemInactive", m => m.DocumentMenuItemInactive);
            RegisterColumn<int>("DocumentWorkflowStepID", m => m.DocumentWorkflowStepID);
            RegisterColumn("DocumentContent", m => m.DocumentContent);
            RegisterColumn<int>("DocumentPageTemplateID", m => m.DocumentPageTemplateID);
            RegisterColumn<int>("DocumentCheckedOutVersionHistoryID", m => m.DocumentCheckedOutVersionHistoryID);
            RegisterColumn<int>("DocumentPublishedVersionHistoryID", m => m.DocumentPublishedVersionHistoryID);
            RegisterColumn<bool>("DocumentSearchExcluded", m => m.DocumentSearchExcluded);
            RegisterColumn("NodeAliasPath", m => m.NodeAliasPath);
            RegisterColumn<int>("NodeID", m => m.NodeID);
            RegisterColumn("NodeName", m => m.NodeName);
            RegisterColumn("NodeAlias", m => m.NodeAlias);
            RegisterColumn<int>("NodeClassID", m => m.NodeClassID);
            RegisterColumn<int>("NodeLevel", m => m.NodeLevel);
            RegisterColumn<int>("NodeParentID", m => m.NodeParentID);
            RegisterColumn<int>("NodeGroupID", m => m.NodeGroupID);
            RegisterColumn<int>("NodeACLID", m => m.NodeACLID);
            RegisterColumn<int>("NodeSiteID", m => m.NodeSiteID);
            RegisterColumn<int>("NodeOrder", m => m.NodeOrder);
            RegisterColumn<Guid>("NodeGUID", m => m.NodeGUID);
            RegisterColumn<bool>("RequiresAuthentication", m => m.RequiresAuthentication);
            RegisterColumn<bool>("RequiresSSL", m => m.RequiresSSL);
            RegisterColumn<int>("NodeCacheMinutes", m => m.NodeCacheMinutes);
            RegisterColumn<bool>("NodeAllowCacheInFileSystem", m => m.NodeAllowCacheInFileSystem);
            RegisterColumn("NodeBodyElementAttributes", m => m.NodeBodyElementAttributes);
            RegisterColumn("NodeBodyScripts", m => m.NodeBodyScripts);
            RegisterColumn("NodeDocType", m => m.NodeDocType);
            RegisterColumn("NodeHeadTags", m => m.NodeHeadTags);
            RegisterColumn("NodeInheritPageLevels", m => m.NodeInheritPageLevels);
            RegisterColumn<int>("NodeTemplateID", m => m.NodeTemplateID);
            RegisterColumn<bool>("NodeTemplateForAllCultures", m => m.NodeTemplateForAllCultures);
            RegisterColumn<bool>("NodeInheritPageTemplate", m => m.NodeInheritPageTemplate);
            RegisterColumn("DocumentPageTitle", m => m.DocumentPageTitle);
            RegisterColumn("DocumentPageKeyWords", m => m.DocumentPageKeyWords);
            RegisterColumn("DocumentPageDescription", m => m.DocumentPageDescription);
            RegisterColumn<int>("DocumentStylesheetID", m => m.DocumentStylesheetID);
            RegisterColumn<int>("NodeLinkedNodeID", m => m.NodeLinkedNodeID);
            RegisterColumn<int>("NodeOwner", m => m.NodeOwner);
            RegisterColumn("DocumentExtensions", m => m.DocumentExtensions);
            RegisterColumn("DocumentTrackConversionName", m => m.DocumentTrackConversionName);
            RegisterColumn("DocumentConversionValue", m => m.DocumentConversionValue);
            RegisterColumn<Guid>("DocumentWorkflowCycleGUID", m => m.DocumentWorkflowCycleGUID);
            RegisterColumn<Guid>("DocumentGUID", m => m.DocumentGUID);
            RegisterColumn<bool>("DocumentLogActivity", m => m.DocumentLogActivity);
            RegisterColumn<bool>("NodeIsContentOnly", m => m.NodeIsContentOnly);
        }


        /// <summary>
        /// Gets friendly document name
        /// </summary>
        public virtual string GetDocumentName()
        {
            return String.IsNullOrEmpty(DocumentName) ? ResHelper.Slash : DocumentName;
        }


        /// <summary>
        /// Sets the page result object for the given url. The page result object is then accessible via PageResult property.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="pageResult">The page result object to be stored</param>
        public void SetPageResult(string url, PageInfoResult pageResult)
        {
            DocumentContext.PageResultUrlPath = url;

            if (!String.IsNullOrEmpty(url))
            {
                if (mPageResult == null)
                {
                    mPageResult = new PageInfoResult();
                }

                // Set the page result data for the given url
                mPageResult.SetCurrentResult(url, pageResult);
            }
        }


        /// <summary>
        /// Loads the given page template info into the Page info
        /// </summary>
        /// <param name="pti">Page template info</param>
        internal void LoadPageTemplateInfo(PageTemplateInfo pti)
        {
            mPageTemplateInfo = pti;
            mPageTemplateInfoLoaded = true;

            SetPageTemplateId(pti.PageTemplateId);
        }


        /// <summary>
        /// Obtains value of field used in Node's HTML section.
        /// </summary>
        /// <param name="value">Value of the HTML section backing field.</param>
        /// <param name="templateValue">Value of the template HTML section backing field.</param>
        /// <param name="propertySelector">Property of <see cref="InheritedPageInfo"/> HTML section backing field.</param>
        /// <param name="checkForNull">If <c>true</c>, value from upper tree is checked for <c>null</c> instead of <c>-1</c>.</param>
        /// <returns>New value of field used in Node's HTML.</returns>
        private string GetHtmlFieldValue(string value, string templateValue, Expression<Func<PageInfo, string>> propertySelector, bool checkForNull = true)
        {
            if (UsesMasterPageTemplate())
            {
                // Current node uses master page template, propagate data
                value = templateValue;
            }

            if (value != null)
            {
                return value;
            }

            if (InheritedPageInfo != null)
            {
                // Get inherited value
                return propertySelector.Compile()(InheritedPageInfo);
            }

            //Get value from upper tree
            var propertyMemberExpression = (MemberExpression)propertySelector.Body;
            return ValidationHelper.GetString(GetValueFromUpperTree(propertyMemberExpression.Member.Name, checkForNull, onlyMaster: true), String.Empty);
        }

        #endregion


        #region "Content management methods"

        /// <summary>
        /// Returns the xml code of the document contents (Editable regions, web parts).
        /// </summary>
        public string GetContentXml()
        {
            return EditableItems.GetContentXml();
        }


        /// <summary>
        /// Loads the content XML to the content table.
        /// </summary>
        /// <param name="contentXml">Content XML to load</param>
        public void LoadContentXml(string contentXml)
        {
            EditableItems.LoadContentXml(contentXml);
        }

        #endregion


        #region ITreeNodeMethods Members

        /// <summary>
        /// Returns true if the document type stands for the product section
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
        /// Sets the default document page template ID
        /// </summary>
        /// <param name="templateId">Page template ID to set</param>
        public void SetPageTemplateId(int templateId)
        {
            if (NodeTemplateForAllCultures)
            {
                NodeTemplateID = templateId;
            }

            DocumentPageTemplateID = templateId;
        }


        /// <summary>
        /// Gets the inherit page levels used by this page
        /// </summary>
        public string GetUsedInheritPageLevels()
        {
            return NodeInheritPageLevels;
        }


        /// <summary>
        /// Gets the page template id used by this document
        /// </summary>
        public string GetUsedPageTemplateIdColumn()
        {
            return TreeNodeMethods.GetUsedPageTemplateIdColumn(this);
        }


        /// <summary>
        /// Gets the page template id used by this document
        /// </summary>
        public int GetUsedPageTemplateId()
        {
            // If inheritance explicitly set, return 0
            if ((mNodeInheritPageTemplate != null) && mNodeInheritPageTemplate.Value)
            {
                return 0;
            }

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
        /// Returns true if node is content only.
        /// </summary>
        internal bool NodeIsContentOnly
        {
            get
            {
                if (mNodeIsContentOnly == null)
                {
                    var dataClass = DataClassInfoProvider.GetDataClassInfo(ClassName);
                    mNodeIsContentOnly = dataClass.ClassIsContentOnly;
                }

                return (mNodeIsContentOnly == true);
            }
        }


        /// <summary>
        /// Returns true if the given document is a root node
        /// </summary>
        public bool IsRoot()
        {
            return TreeNodeMethods.IsRoot(this);
        }

        #endregion
    }
}