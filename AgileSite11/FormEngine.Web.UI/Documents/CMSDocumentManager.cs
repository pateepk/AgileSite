using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.Modules;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Document manager.
    /// </summary>
    [ToolboxData("<{0}:CMSDocumentManager runat=server></{0}:CMSDocumentManager>")]
    public class CMSDocumentManager : CMSAbstractManager<DocumentManagerEventArgs, SimpleDocumentManagerEventArgs>, IPostBackEventHandler, ICMSDocumentManager
    {
        #region "Variables"

        /// <summary>
        /// Tree provider to use for DB access.
        /// </summary>
        private TreeProvider mTree;


        /// <summary>
        /// WorkflowManager for managing workflow.
        /// </summary>
        private WorkflowManager mWorkflowManager;


        /// <summary>
        /// VersionManager for managing version.
        /// </summary>
        private VersionManager mVersionManager;


        /// <summary>
        /// Next steps
        /// </summary>
        private List<WorkflowStepInfo> mNextSteps;


        /// <summary>
        /// Document workflow
        /// </summary>
        private BaseInfo mWorkflow;


        /// <summary>
        /// Site name of newly created document
        /// </summary>
        private string mSiteName;


        /// <summary>
        /// Invariant document node.
        /// </summary>
        private TreeNode mInvariantNode;


        /// <summary>
        /// Source document for new language version.
        /// </summary>
        private TreeNode mSourceNode;


        /// <summary>
        /// Node ID
        /// </summary>
        private int mNodeId;
        private TreeNode mNode;


        /// <summary>
        /// Document ID
        /// </summary>
        private int mDocumentId;


        /// <summary>
        /// Community group ID
        /// </summary>
        private int mGroupId;


        /// <summary>
        /// New node class ID
        /// </summary>
        private int mNewNodeClassID;


        /// <summary>
        /// New node class name
        /// </summary>
        private string mNewNodeClassName;


        /// <summary>
        /// Parent document node.
        /// </summary>
        private TreeNode mParentNode;


        /// <summary>
        /// Culture code of document culture
        /// </summary>
        private string mCultureCode;


        /// <summary>
        /// Indicates if content should be refreshed (action is being processed)
        /// </summary>
        private bool? mRefreshActionContent;


        /// <summary>
        /// Node class
        /// </summary>
        private DataClassInfo mNewNodeClass;


        /// <summary>
        /// Node culture of new document.
        /// </summary>
        private string mNewNodeCultureCode;


        /// <summary>
        /// Indicates if save changes script should be registered.
        /// </summary>
        private bool? mRegisterSaveChangesScript;


        /// <summary>
        /// Indicates whether add comment dialog should be available.
        /// </summary>
        private bool? mAddComment;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if default values should be loaded when new document or language version is being created.
        /// </summary>
        public bool LoadDefaultValues
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if manager should register for common events (save etc.).
        /// </summary>
        public bool RegisterEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the support script for save changes should be registered.
        /// </summary>
        public bool RegisterSaveChangesScript
        {
            get
            {
                if (mRegisterSaveChangesScript == null)
                {
                    // Save changes script is not allowed for live site
                    mRegisterSaveChangesScript = !IsLiveSite;
                }

                return mRegisterSaveChangesScript.Value;
            }
            set
            {
                mRegisterSaveChangesScript = value;
            }
        }


        /// <summary>
        /// Indicates whether reload page property should be automatically added to the page for reload page functionality
        /// </summary>
        public bool SetRefreshFlag
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the page should be redirected to the current page which is being edited.
        /// </summary>
        public bool SetRedirectPageFlag
        {
            get;
            set;
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeProvider Tree
        {
            get
            {
                return mTree ?? (mTree = new TreeProvider(CurrentUser));
            }
            set
            {
                mTree = value;
            }
        }


        /// <summary>
        /// Gets Workflow manager instance.
        /// </summary>
        public virtual WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(Tree));
            }
        }


        /// <summary>
        /// Version manager instance.
        /// </summary>
        public virtual VersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = VersionManager.GetInstance(Tree));
            }
        }


        /// <summary>
        /// Indicates if check-in/check-out functionality is automatic.
        /// </summary>
        public bool AutoCheck
        {
            get
            {
                // Check if the document uses workflow
                if (Workflow != null)
                {
                    return !Workflow.UseCheckInCheckOut(Node.NodeSiteName);
                }

                return false;
            }
        }


        /// <summary>
        /// Document workflow.
        /// </summary>
        public WorkflowInfo Workflow
        {
            get
            {
                return (WorkflowInfo)InfoHelper.EnsureInfo(ref mWorkflow, GetWorkflow);
            }
        }


        /// <summary>
        /// Document workflow step.
        /// </summary>
        public WorkflowStepInfo Step
        {
            get
            {
                if (Node != null)
                {
                    // Get workflow step info
                    return Node.WorkflowStep;
                }

                return null;
            }
        }


        /// <summary>
        /// Next steps
        /// </summary>
        public List<WorkflowStepInfo> NextSteps
        {
            get
            {
                return mNextSteps ?? (mNextSteps = WorkflowManager.GetNextSteps(Node, Step));
            }
        }


        /// <summary>
        /// Document information label.
        /// </summary>
        public Label DocumentInfoLabel
        {
            get
            {
                EnsureChildControls();
                return DocumentPanel.Label;
            }
        }


        /// <summary>
        /// Document information text.
        /// </summary>
        public string DocumentInfo
        {
            get
            {
                return DocumentInfoLabel.Text;
            }
            set
            {
                DocumentInfoLabel.Text = value;
            }
        }


        /// <summary>
        /// Document ID of document language version, which should be used as a source data for new culture version.
        /// </summary>
        public int SourceDocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Component name
        /// </summary>
        public override string ComponentName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ComponentName"], base.ComponentName);
            }
            set
            {
                base.ComponentName = value;
                ViewState["ComponentName"] = value;
            }
        }


        /// <summary>
        /// Site name of the newly created document.
        /// </summary>
        public string SiteName
        {
            get
            {
                string siteName = ValidationHelper.GetString(ViewState["SiteName"], mSiteName);
                if (siteName == null)
                {
                    siteName = SiteContext.CurrentSiteName;
                    ViewState["SiteName"] = siteName;
                }
                mSiteName = siteName;

                return mSiteName;
            }
            set
            {
                mSiteName = value;
                ViewState["SiteName"] = value;
            }
        }


        /// <summary>
        /// Gets site ID of the newly created document.
        /// </summary>
        protected int SiteID
        {
            get
            {
                if (!string.IsNullOrEmpty(SiteName))
                {
                    return SiteInfoProvider.GetSiteID(SiteName);
                }

                return 0;
            }
        }


        /// <summary>
        /// Indicates if the UI should be redirected if document not found.
        /// </summary>
        public bool RedirectForNonExistingDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Currently edited document
        /// </summary>
        protected new TreeNode CurrentDocument
        {
            get
            {
                if (IsLiveSite)
                {
                    return mNode;
                }
                else
                {
                    return EditedDocument;
                }
            }
            set
            {
                if (IsLiveSite)
                {
                    mNode = value;
                }
                else
                {
                    // Check edited document
                    EditedDocument = value;
                }
            }
        }


        /// <summary>
        /// Document edited by the current page. If set to NULL, redirects to the information page with information that document has been deleted.
        /// </summary>
        protected static TreeNode EditedDocument
        {
            get
            {
                return DocumentContext.EditedDocument;
            }
            set
            {
                if (value == null)
                {
                    URLHelper.Redirect(AdministrationUrlHelper.GetInformationUrl("editeddocument.notexists"));
                }
                else
                {
                    DocumentContext.EditedDocument = value;
                }
            }
        }


        /// <summary>
        /// Returns currently edited document if it is available in the given context.
        /// </summary>
        public TreeNode Node
        {
            get
            {
                if (CurrentDocument == null)
                {
                    switch (Mode)
                    {
                        case FormModeEnum.Update:
                            // Update
                            if (DocumentID > 0)
                            {
                                CurrentDocument = DocumentHelper.GetDocument(DocumentID, Tree);
                            }
                            else if (NodeID > 0)
                            {
                                if (RedirectForNonExistingDocument)
                                {
                                    // If not exists, redirect to special page (for linked documents)
                                    CurrentDocument = GetDocument();
                                }
                                else
                                {
                                    // Do not redirect to not existing page
                                    if (IsLiveSite)
                                    {
                                        mNode = GetDocument();
                                    }
                                    else
                                    {
                                        // Set document without null reference check
                                        DocumentContext.EditedDocument = GetDocument();
                                    }
                                }
                            }
                            break;

                        case FormModeEnum.Insert:
                            // Insert new document
                            {
                                string className = NewNodeClass.ClassName;
                                CurrentDocument = TreeNode.New(className, Tree);

                                // Load default data
                                if (LoadDefaultValues)
                                {
                                    FormHelper.LoadDefaultValues(className, CurrentDocument, FormResolveTypeEnum.AllFields);
                                }

                                // Set other properties
                                CurrentDocument.NodeParentID = ParentNodeID;
                                CurrentDocument.DocumentCulture = NewNodeCultureCode;
                                CurrentDocument.SetValue("NodeSiteID", SiteID);
                            }
                            break;

                        case FormModeEnum.InsertNewCultureVersion:
                            // Insert new culture version
                            if (SourceNode != null)
                            {
                                CurrentDocument = TreeNode.New(SourceNode.NodeClassName, Tree);

                                // Source node provided
                                if (SourceDocumentID > 0)
                                {
                                    // Copy the data to the new node
                                    CopyNodeDataSettings settings = new CopyNodeDataSettings(true, null)
                                    {
                                        ResetChanges = true
                                    };
                                    DocumentHelper.CopyNodeData(SourceNode, CurrentDocument, settings);
                                }
                                else
                                {
                                    // Copy the data to the new node
                                    CopyNodeDataSettings settings = new CopyNodeDataSettings(true, null)
                                    {
                                        CopyDocumentData = false,
                                        CopyCoupledData = false,
                                        ResetChanges = true
                                    };
                                    DocumentHelper.CopyNodeData(SourceNode, CurrentDocument, settings);

                                    // Copy the Page template ID from source node
                                    if (SourceNode.DocumentPageTemplateID > 0)
                                    {
                                        CurrentDocument.SetValue("DocumentPageTemplateID", SourceNode.DocumentPageTemplateID);
                                    }

                                    // Load default data
                                    if (LoadDefaultValues)
                                    {
                                        FormHelper.LoadDefaultValues(CurrentDocument.NodeClassName, CurrentDocument, FormResolveTypeEnum.AllFields);
                                    }
                                }

                                // Set other properties
                                CurrentDocument.DocumentCulture = NewNodeCultureCode;
                                CurrentDocument.SetValue("DocumentID", 0);
                            }
                            else
                            {
                                throw new NullReferenceException("[CMSDocumentManager.Node]: Source node is not initialized.");
                            }
                            break;
                    }
                }

                return CurrentDocument;
            }
        }


        /// <summary>
        /// Returns current document in any culture.
        /// </summary>
        public TreeNode InvariantNode
        {
            get
            {
                if (mInvariantNode == null)
                {
                    if (Mode != FormModeEnum.Insert)
                    {
                        mInvariantNode = GetDocument(TreeProvider.ALL_CULTURES);
                    }
                }

                return mInvariantNode;
            }
        }


        /// <summary>
        /// Source document for new language version
        /// </summary>
        public TreeNode SourceNode
        {
            get
            {
                if (mSourceNode == null)
                {
                    if (SourceDocumentID > 0)
                    {
                        mSourceNode = DocumentHelper.GetDocument(SourceDocumentID, Tree);
                    }
                    else
                    {
                        // Get the default culture node for the CMS_Tree source data. If not found, get any culture version.
                        mSourceNode = DocumentHelper.GetDocument(NodeID, SettingsKeyInfoProvider.GetValue(SiteName + ".CMSDefaultCultureCode"), Tree) ?? DocumentHelper.GetDocument(NodeID, TreeProvider.ALL_CULTURES, Tree);
                    }
                }

                return mSourceNode;
            }
        }


        /// <summary>
        /// Node ID. Together with DocumentCulture property identifies edited document.
        /// </summary>
        public int NodeID
        {
            get
            {
                // Ensure node ID
                if ((mNodeId == 0) && (CurrentDocument != null))
                {
                    mNodeId = CurrentDocument.NodeID;
                    ViewState["NodeID"] = mNodeId;
                }

                return ValidationHelper.GetInteger(ViewState["NodeID"], mNodeId);
            }
            set
            {
                ViewState["NodeID"] = value;
                mNodeId = value;
            }
        }


        /// <summary>
        /// Community group ID
        /// </summary>
        public int GroupID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["GroupID"], mGroupId);
            }
            set
            {
                mGroupId = value;
                ViewState["GroupID"] = value;
            }
        }


        /// <summary>
        /// Parent node ID. Indicates parent node for document insertion.
        /// </summary>
        public int ParentNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Returns parent node for new document insertion.
        /// </summary>
        public TreeNode ParentNode
        {
            get
            {
                if (mParentNode == null)
                {
                    switch (Mode)
                    {
                        case FormModeEnum.Insert:
                        case FormModeEnum.InsertNewCultureVersion:
                            // Use parent node ID directly
                            if (ParentNodeID > 0)
                            {
                                mParentNode = DocumentHelper.GetDocument(ParentNodeID, TreeProvider.ALL_CULTURES, Tree);
                            }
                            break;

                        default:
                            // Use the given node ID
                            mParentNode = DocumentHelper.GetDocument(Node.NodeParentID, TreeProvider.ALL_CULTURES, Tree);
                            break;
                    }
                }

                return mParentNode;
            }
            set
            {
                mParentNode = value;
            }
        }


        /// <summary>
        /// Document ID. Has higher priority than Node ID. Identifies edited document.
        /// </summary>
        public int DocumentID
        {
            get
            {
                // Ensure document ID
                if ((mDocumentId == 0) && (CurrentDocument != null))
                {
                    mDocumentId = CurrentDocument.DocumentID;
                    ViewState["DocumentID"] = mDocumentId;
                }

                return ValidationHelper.GetInteger(ViewState["DocumentID"], mDocumentId);
            }
            set
            {
                ViewState["DocumentID"] = value;
                mDocumentId = value;
            }
        }


        /// <summary>
        /// Class ID of the document type of new document.
        /// </summary>
        public int NewNodeClassID
        {
            get
            {
                return mNewNodeClassID;
            }
            set
            {
                mNewNodeClassID = value;
                if (mNewNodeClassID > 0)
                {
                    mNewNodeClass = DataClassInfoProvider.GetDataClassInfo(mNewNodeClassID);
                    if (mNewNodeClass != null)
                    {
                        mNewNodeClassName = mNewNodeClass.ClassName;
                    }
                }
            }
        }


        /// <summary>
        /// Class name of the document type of new document.
        /// </summary>
        public string NewNodeClassName
        {
            get
            {
                return mNewNodeClassName;
            }
            set
            {
                mNewNodeClassName = value;
                if (!string.IsNullOrEmpty(mNewNodeClassName))
                {
                    mNewNodeClass = DataClassInfoProvider.GetDataClassInfo(mNewNodeClassName);
                    if (mNewNodeClass != null)
                    {
                        mNewNodeClassID = mNewNodeClass.ClassID;
                    }
                }
            }
        }


        /// <summary>
        /// Node class of the document type of new document.
        /// </summary>
        public DataClassInfo NewNodeClass
        {
            get
            {
                if ((mNewNodeClass == null) && (NewNodeClassID > 0))
                {
                    mNewNodeClass = DataClassInfoProvider.GetDataClassInfo(NewNodeClassID);
                }
                else if ((mNewNodeClass == null) && !string.IsNullOrEmpty(NewNodeClassName))
                {
                    mNewNodeClass = DataClassInfoProvider.GetDataClassInfo(NewNodeClassName);
                }

                if (mNewNodeClass == null)
                {
                    throw new NullReferenceException("[CMSDocumentManager.NodeClass]: Missing node class for new document.");
                }

                return mNewNodeClass;
            }
        }


        /// <summary>
        /// Node culture of new document.
        /// </summary>
        public string NewNodeCultureCode
        {
            get
            {
                return mNewNodeCultureCode ?? (mNewNodeCultureCode = CultureCode);
            }
            set
            {
                mNewNodeCultureCode = value;
            }
        }


        /// <summary>
        /// Indicates if java script functions for document management should be rendered.
        /// </summary>
        public bool RenderScript
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code of document
        /// </summary>
        public string CultureCode
        {
            get
            {
                string culture = ValidationHelper.GetString(ViewState["CultureCode"], mCultureCode);
                if (string.IsNullOrEmpty(culture))
                {
                    culture = LocalizationContext.PreferredCultureCode;
                    ViewState["CultureCode"] = culture;
                }
                mCultureCode = culture;

                return mCultureCode;
            }
            set
            {
                mCultureCode = value;
                ViewState["CultureCode"] = value;
            }
        }


        /// <summary>
        /// Current step ID
        /// </summary>
        protected int CurrentStepID
        {
            get
            {
                return ValidationHelper.GetInteger(CurrentStepArg.Value, 0);
            }
            set
            {
                CurrentStepArg.Value = value.ToString();
            }
        }


        /// <summary>
        /// Indicates if Save action is allowed.
        /// </summary>
        public bool AllowSave
        {
            get
            {
                return IsActionAllowed(ComponentEvents.SAVE);
            }
        }


        /// <summary>
        /// Indicates if workflow action is being processed
        /// </summary>
        public bool ProcessingAction
        {
            get
            {
                return (WorkflowManager.GetActionStatus(Node) == WorkflowHelper.ACTION_SATUS_RUNNING);
            }
        }


        /// <summary>
        /// Indicates if content should be refreshed (action is being processed)
        /// </summary>
        public bool RefreshActionContent
        {
            get
            {
                if (mRefreshActionContent == null)
                {
                    bool isActionStep = (Step != null) && Step.StepIsAction;
                    mRefreshActionContent = isActionStep && ProcessingAction;
                }

                return mRefreshActionContent.Value;
            }
        }


        /// <summary>
        /// Indicates if another document should be created after newly created.
        /// </summary>
        public bool CreateAnother
        {
            get
            {
                return ValidationHelper.GetBoolean(HiddenAnotherFlag.Value, false);
            }
        }


        /// <summary>
        /// Indicates whether dialog should be closed after action
        /// </summary>
        public bool CloseDialog
        {
            get
            {
                return ValidationHelper.GetBoolean(HiddenCloseFlag.Value, false);
            }
        }


        /// <summary>
        /// Action comment
        /// </summary>
        public string ActionComment
        {
            get
            {
                EnsureChildControls();
                return HTMLHelper.HTMLEncode(HiddenComment.Value);
            }
        }


        /// <summary>
        /// Returns true if the changes should be saved.
        /// </summary>
        public bool SaveChanges
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (!RegisterSaveChangesScript || (Context == null))
                {
                    return false;
                }

                return ValidationHelper.GetBoolean(HiddenSaveChanges.Value, false);
            }
        }


        /// <summary>
        /// Indicates if confirm dialog should be displayed if any change to the document was made
        /// </summary>
        public bool ConfirmChanges
        {
            get
            {
                if (!RegisterSaveChangesScript)
                {
                    return false;
                }

                // Save changes support
                string siteName = SiteName;
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                    case FormModeEnum.InsertNewCultureVersion:
                        break;

                    default:
                        siteName = (Node != null) ? Node.NodeSiteName : SiteName;
                        break;
                }

                return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSConfirmChanges");
            }
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        public override MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                return LocalMessagesPlaceHolder;
            }
        }


        /// <summary>
        /// Local messages placeholder
        /// </summary>
        public MessagesPlaceHolder LocalMessagesPlaceHolder
        {
            get
            {
                EnsureChildControls();
                return plcMess;
            }
            set
            {
                plcMess = value;
            }
        }


        /// <summary>
        /// Document info panel
        /// </summary>
        public CMSDocumentPanel DocumentPanel
        {
            get
            {
                return LocalDocumentPanel;
            }
        }


        /// <summary>
        /// Local document info panel
        /// </summary>
        public CMSDocumentPanel LocalDocumentPanel
        {
            get
            {
                EnsureChildControls();
                return pnlDocumentInfo;
            }
            set
            {
                pnlDocumentInfo = value;
            }
        }


        /// <summary>
        /// Hidden argument
        /// </summary>
        protected HiddenField HiddenArguments
        {
            get
            {
                EnsureChildControls();
                return hdnArgs;
            }
        }


        /// <summary>
        /// Hidden save another flag
        /// </summary>
        protected HiddenField HiddenAnotherFlag
        {
            get
            {
                EnsureChildControls();
                return hdnAnother;
            }
        }


        /// <summary>
        /// Hidden close flag
        /// </summary>
        protected HiddenField HiddenCloseFlag
        {
            get
            {
                EnsureChildControls();
                return hdnClose;
            }
        }


        /// <summary>
        /// Hidden current step flag
        /// </summary>
        protected HiddenField CurrentStepArg
        {
            get
            {
                EnsureChildControls();
                return hdnCurrStep;
            }
        }


        /// <summary>
        /// Hidden save changes flag
        /// </summary>
        protected HiddenField HiddenSaveChanges
        {
            get
            {
                EnsureChildControls();
                return hdnSaveChanges;
            }
        }


        /// <summary>
        /// Hidden content changed flag
        /// </summary>
        protected HiddenField HiddenContentChanged
        {
            get
            {
                EnsureChildControls();
                return hdnContentChanged;
            }
        }


        /// <summary>
        /// Hidden comment field
        /// </summary>
        protected HiddenField HiddenComment
        {
            get
            {
                EnsureChildControls();
                return hdnComment;
            }
        }


        /// <summary>
        /// Indicates if workflow actions should be displayed and handled
        /// </summary>
        public bool HandleWorkflow
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if DocumentHelper should be used to manage the document (Use FALSE if modifying non-versioned properties.)
        /// </summary>
        public bool UseDocumentHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether changes should be tracked for whole form or just for parts defined by data-tracksavechanges attribute.
        /// </summary>
        public bool UseFullFormSaveChanges
        {
            get;
            set;
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Indicates whether add comment dialog should be available.
        /// </summary>
        private bool AddComment
        {
            get
            {
                if (mAddComment == null)
                {
                    // There is nothing to comment when inserting new node or culture version
                    mAddComment = (Mode != FormModeEnum.Insert) && (Mode != FormModeEnum.InsertNewCultureVersion) &&
                                  (IsActionAllowed(DocumentComponentEvents.APPROVE) || IsActionAllowed(DocumentComponentEvents.REJECT) ||
                                   IsActionAllowed(DocumentComponentEvents.PUBLISH) || IsActionAllowed(DocumentComponentEvents.ARCHIVE) ||
                                   IsActionAllowed(DocumentComponentEvents.CHECKIN));
                }
                return mAddComment.Value;
            }
        }

        #endregion


        #region "Controls"

        /// <summary>
        /// Hidden field for post back arguments
        /// </summary>
        protected HiddenField hdnArgs = new HiddenField { ID = "hdnArgs", EnableViewState = false };

        /// <summary>
        /// Hidden field for save action argument
        /// </summary>
        protected HiddenField hdnAnother = new HiddenField { ID = "hdnAnother", EnableViewState = false };

        /// <summary>
        /// Hidden field for save &amp; close action argument
        /// </summary>
        protected HiddenField hdnClose = new HiddenField { ID = "hdnClose", EnableViewState = false };

        /// <summary>
        /// Hidden field for current step argument
        /// </summary>
        protected HiddenField hdnCurrStep = new HiddenField { ID = "hdnCurrStep", EnableViewState = true };

        /// <summary>
        /// Hidden field for comment argument
        /// </summary>
        protected HiddenField hdnComment = new HiddenField { ID = "hdnComment", EnableViewState = false };

        /// <summary>
        /// Hidden field for save changes flag
        /// </summary>
        protected HiddenField hdnSaveChanges = new HiddenField { ID = "hdnSaveChanges", EnableViewState = false };

        /// <summary>
        /// Hidden field for content changed flag
        /// </summary>
        protected HiddenField hdnContentChanged = new HiddenField { ID = "hdnContentChanged", EnableViewState = true };

        /// <summary>
        /// Messages placeholder
        /// </summary>
        protected MessagesPlaceHolder plcMess;

        /// <summary>
        /// Document info panel
        /// </summary>
        protected CMSDocumentPanel pnlDocumentInfo;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSDocumentManager()
        {
            UseFullFormSaveChanges = true;
            LoadDefaultValues = true;
            RegisterSaveChangesScript = true;
            HandleWorkflow = true;
            RenderScript = false;
            CheckPermissions = true;
            Mode = FormModeEnum.Update;
            UseDocumentHelper = true;
            DataConsistent = true;
            RedirectForNonExistingDocument = true;
            RegisterEvents = true;
        }

        #endregion


        #region "Page processing methods"

        /// <summary>
        /// CreateChildControls event handler.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (StopProcessing)
            {
                return;
            }

            if (Page == null)
            {
                throw new Exception("[CMSDocumentManager]: DocumentManager is not ensured. Set EnsureDocumentProperty property of a Page to true. Or missing managers placeholder.");
            }

            if (plcMess == null)
            {
                plcMess = new MessagesPlaceHolder { ID = "plcMess", ShortID = "pM", IsLiveSite = IsLiveSite };
                Controls.AddAt(0, plcMess);
            }

            // Add the hidden field for post back arguments
            Controls.Add(hdnArgs);
            Controls.Add(hdnAnother);
            Controls.Add(hdnClose);
            Controls.Add(hdnComment);
            Controls.Add(hdnCurrStep);
            if (RegisterSaveChangesScript)
            {
                Controls.Add(hdnSaveChanges);
                Controls.Add(hdnContentChanged);
            }

            // Document info panel
            if (pnlDocumentInfo == null)
            {
                pnlDocumentInfo = new CMSDocumentPanel { ID = "pnlDocumentInfo", ShortID = "pD" };
                Controls.AddAt(0, pnlDocumentInfo);
            }
        }


        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                return;
            }

            if (RegisterEvents)
            {
                // Register events
                if (Mode == FormModeEnum.Update)
                {
                    if (Node == null)
                    {
                        return;
                    }

                    RegisterComponentEvent(DocumentComponentEvents.PUBLISH, (actionArgument) => PublishDocument(ActionComment));
                    RegisterComponentEvent(DocumentComponentEvents.APPROVE, (actionArgument) => ApproveDocument(actionArgument, ActionComment));
                    RegisterComponentEvent(DocumentComponentEvents.REJECT, (actionArgument) => RejectDocument(actionArgument, ActionComment));
                    RegisterComponentEvent(DocumentComponentEvents.ARCHIVE, (actionArgument) => ArchiveDocument(actionArgument, ActionComment));
                    RegisterComponentEvent(DocumentComponentEvents.CHECKIN, (actionArgument) => CheckInDocument(ActionComment));
                    RegisterComponentEvent(DocumentComponentEvents.CHECKOUT, (actionArgument) => CheckOutDocument());
                    RegisterComponentEvent(DocumentComponentEvents.UNDO_CHECKOUT, (actionArgument) => UndoCheckOutDocument());
                    RegisterComponentEvent(DocumentComponentEvents.CREATE_VERSION, (actionArgument) => CreateNewVersion());
                }

                RegisterComponentEvent(ComponentEvents.SAVE, (actionArgument) => SaveDocument());
            }
        }


        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StopProcessing)
            {
                return;
            }

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CMSDocumentManager Control : " + ClientID + " ]");
                return;
            }

            RenderScripts();
        }


        /// <summary>
        /// Renders scripts
        /// </summary>
        public void RenderScripts()
        {
            StringBuilder sb = new StringBuilder();
            if (RenderScript)
            {
                bool setStepId = false;

                sb.Append("function CheckConsistency_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(this), "; } \n");
                if (IsActionAllowed(ComponentEvents.SAVE))
                {
                    sb.Append("function SaveDocument_", ClientID, "(createAnother) { if(createAnother) { ", GetSaveAnotherScript(), "}", GetSubmitScript(), ControlsHelper.GetPostBackEventReference(this, ComponentEvents.SAVE), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.APPROVE))
                {
                    sb.Append("function Approve_", ClientID, "(stepId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(stepId);", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.APPROVE), "; } \n");
                    setStepId = true;
                }
                if (IsActionAllowed(DocumentComponentEvents.REJECT))
                {
                    sb.Append("function Reject_", ClientID, "(historyId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(historyId);", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.REJECT), "; } \n");
                    setStepId = true;
                }
                if (IsActionAllowed(DocumentComponentEvents.PUBLISH))
                {
                    sb.Append("function Publish_", ClientID, "(comment) { SetComment_", ClientID, "(comment); ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.PUBLISH), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.ARCHIVE))
                {
                    sb.Append("function Archive_", ClientID, "(stepId, comment) { SetComment_", ClientID, "(comment); SetParam_", ClientID, "(stepId);", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.ARCHIVE), "; } \n");
                    setStepId = true;
                }
                if (IsActionAllowed(DocumentComponentEvents.CHECKOUT))
                {
                    sb.Append("function CheckOut_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.CHECKOUT), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.CHECKIN))
                {
                    sb.Append("function CheckIn_", ClientID, "(comment) { SetComment_", ClientID, "(comment); ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.CHECKIN), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.UNDO_CHECKOUT))
                {
                    sb.Append("function UndoCheckOut_", ClientID, "() { if(!confirm(" + ScriptHelper.GetString(ResHelper.GetString("EditMenu.UndoCheckOutConfirmation", ResourceCulture)) + ")) return false; ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.UNDO_CHECKOUT), "; } \n");
                }

                if (setStepId)
                {
                    sb.Append("function SetParam_", ClientID, "(param) { document.getElementById('", HiddenArguments.ClientID, "').value = param; } \n");
                }

                if (AddComment)
                {
                    string url = ApplicationUrlHelper.ResolveDialogUrl(IsLiveSite ? "~/CMSModules/Workflows/CMSPages/Comment.aspx" : "~/CMSModules/Workflows/Pages/Comment.aspx");
                    sb.Append("function AddComment_", ClientID, "(name, docId, menuId) { if((menuId == null) || (menuId == '')){ menuId = '", ClientID, "'; } modalDialog('", url, "?acname=' + name + '&documentId=' + docId + '&menuId=' + menuId, 'workflowComment', 770, 400, null, null, true); }\n");
                    sb.Append("function SetComment_", ClientID, "(comment) { document.getElementById('", HiddenComment.ClientID, "').value = comment; } \n");
                }
            }

            if (RegisterSaveChangesScript)
            {
                sb.AppendFormat("CMSContentManager.hdnSaveChangesId = '{0}';", HiddenSaveChanges.ClientID);
                sb.AppendFormat("CMSContentManager.hdnContentChangedId = '{0}';", HiddenContentChanged.ClientID);

                // Confirm changes message
                if (ConfirmChanges)
                {
                    sb.Append("CMSContentManager.confirmLeave=", ScriptHelper.GetString(ResHelper.GetString("Content.ConfirmLeave", ResourceCulture), true, false), "; \n");
                    sb.Append("CMSContentManager.confirmLeaveShort=", ScriptHelper.GetString(ResHelper.GetString("Content.ConfirmLeaveShort", ResourceCulture), true, false), "; \n");
                }
                else
                {
                    sb.Append("CMSContentManager.confirmChanges = false;");
                }
            }

            // Register the script
            if (sb.Length > 0)
            {
                ScriptHelper.RegisterStartupScript(Page, typeof(string), "DocumentManagement_" + ClientID, ScriptHelper.GetScript(sb.ToString()));
            }
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="argument">Action argument</param>
        /// <param name="comment">Action comment</param>
        public string GetJSFunction(string action, string argument, string comment)
        {
            return GetJSFunctionInternal(action, argument, comment);
        }


        /// <summary>
        /// Gets java-script function for save action
        /// </summary>
        /// <param name="createAnother">Create another flag</param>
        public string GetJSSaveFunction(string createAnother)
        {
            return GetJSFunctionInternal(ComponentEvents.SAVE, createAnother, null);
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="arg">Argument (optional)</param>
        /// <param name="comment">Action comment (optional)</param>
        private string GetJSFunctionInternal(string action, string arg, string comment)
        {
            string name = null;

            if (string.IsNullOrEmpty(comment))
            {
                comment = "''";
            }

            switch (action)
            {
                case ComponentEvents.SAVE:
                    name = "SaveDocument_{0}()";
                    break;

                case "CONS":
                    name = "CheckConsistency_{0}()";
                    break;

                case DocumentComponentEvents.APPROVE:
                    name = "Approve_{0}({2},{1})";
                    break;

                case DocumentComponentEvents.REJECT:
                    name = "Reject_{0}({2},{1})";
                    break;

                case DocumentComponentEvents.PUBLISH:
                    name = "Publish_{0}({1})";
                    break;

                case DocumentComponentEvents.ARCHIVE:
                    name = "Archive_{0}({2},{1})";
                    break;

                case DocumentComponentEvents.CHECKOUT:
                    name = "CheckOut_{0}()";
                    break;

                case DocumentComponentEvents.CHECKIN:
                    name = "CheckIn_{0}({1})";
                    break;

                case DocumentComponentEvents.UNDO_CHECKOUT:
                    name = "UndoCheckOut_{0}()";
                    break;

                // Special case
                case ComponentEvents.COMMENT:
                    // actionName|documentId|menuId
                    string[] args = arg.Split('|');
                    string menuId = (args.Length == 3) ? args[2] : "''";
                    return string.Format("AddComment_{0}({1},{2},{3})", ClientID, args[0], args[1], menuId);
            }

            if (String.IsNullOrEmpty(name))
            {
                return name;
            }

            return string.Format(name, ClientID, comment, arg, action);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (StopProcessing)
            {
                return;
            }

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            // Store step ID
            switch (Mode)
            {
                case FormModeEnum.Insert:
                case FormModeEnum.InsertNewCultureVersion:
                    break;

                default:
                    // Store step ID only for editing
                    CurrentStepID = (Step != null) ? Step.StepID : 0;
                    break;
            }

            RegisterScripts();
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Checks if given action is allowed
        /// </summary>
        public bool IsActionAllowed(string actionName)
        {
            // Node is not initialized
            if (Node == null)
            {
                return true;
            }

            // Initialize security properties
            string storageKey = string.Format("DocumentManager_{0}_{1}_{2}", Node.NodeID, Node.DocumentCulture, Mode);
            InitializeSecurityProperties(storageKey);

            // Check request stock helper
            string key = GetActionKey(actionName);
            if (RequestStockHelper.Contains(storageKey, key, false))
            {
                return ValidationHelper.GetBoolean(RequestStockHelper.GetItem(storageKey, key, false), false);
            }

            return false;
        }


        /// <summary>
        /// Gets action request key
        /// </summary>
        private string GetActionKey(string actionName)
        {
            return string.Join("_", actionName, HandleWorkflow.ToString());
        }


        /// <summary>
        /// Clears properties
        /// </summary>
        public override void ClearProperties()
        {
            if (Node != null)
            {
                string storageKey = string.Format("DocumentManager_{0}_{1}_{2}", Node.NodeID, Node.DocumentCulture, Mode);
                RequestStockHelper.DropStorage(storageKey, false);
            }

            // Clear next step info
            mNextSteps = null;

            // Clear workflow info
            mWorkflow = null;

            // Clear add comment flag
            mAddComment = null;
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Gets the default error message for check permission error
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override string GetDefaultCheckPermissionsError(SimpleDocumentManagerEventArgs args)
        {
            var errorMessage = ResHelper.GetString(args.Node.HasSKU ? "com.notauthorizedtoeditdocumentorproduct" : "cmsdesk.notauthorizedtoeditdocument", ResourceCulture);
            return String.Format(errorMessage, args.Node.NodeAliasPath);
        }


        /// <summary>
        /// Checks the default security for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override void CheckDefaultSecurity(SimpleDocumentManagerEventArgs args)
        {
            switch (Mode)
            {
                case FormModeEnum.Insert:
                    if (!DocumentSecurityHelper.IsAuthorizedToCreateNewDocument(ParentNode, NewNodeClass.ClassName, LocalizationContext.PreferredCultureCode, Tree.UserInfo))
                    {
                        args.IsValid = false;
                        args.ErrorMessage = ResHelper.GetString("accessdenied.notallowedtocreatedocument", ResourceCulture);
                    }
                    break;

                case FormModeEnum.InsertNewCultureVersion:
                    if (!DocumentSecurityHelper.IsAuthorizedToCreateNewDocument(ParentNode, Node.NodeClassName, LocalizationContext.PreferredCultureCode, Tree.UserInfo))
                    {
                        args.IsValid = false;
                        args.ErrorMessage = ResHelper.GetString("accessdenied.notallowedtocreatenewcultureversion", ResourceCulture);
                    }
                    break;

                case FormModeEnum.Update:
                    if (!Node.CheckPermissions(PermissionsEnum.Modify, Node.NodeSiteName, Tree.UserInfo))
                    {
                        args.IsValid = false;
                        args.ErrorMessage = GetDefaultCheckPermissionsError(args);
                    }
                    break;
            }
        }


        /// <summary>
        /// Initializes security properties
        /// </summary>
        /// <param name="storageKey">Storage key for request</param>
        private void InitializeSecurityProperties(string storageKey)
        {
            // Properties already initialized
            if (RequestStockHelper.GetStorage(storageKey, false, false) != null)
            {
                return;
            }

            bool allowApprove = false;
            bool allowReject = false;
            bool allowCheckin = false;
            bool allowCheckout = false;
            bool allowUndoCheckout = false;
            bool allowArchive = false;
            bool allowCreateNewVersion = false;
            bool allowApplyWorkflow = false;

            bool allowSave = IsAuthorized(false);

            if (allowSave)
            {
                WorkflowInfo wi = Workflow;

                if (Mode == FormModeEnum.Update)
                {
                    // If workflow not null, process the workflow information
                    if (wi != null)
                    {
                        // Get current step info
                        WorkflowStepInfo si = Step;
                        if (HandleWorkflow)
                        {
                            bool autoPublishChanges = wi.WorkflowAutoPublishChanges;
                            bool useCheckInCheckOut = !AutoCheck;
                            bool canApprove = WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Approve);

                            if (!canApprove)
                            {
                                allowSave = false;
                            }

                            // Check-in, Check-out
                            if (useCheckInCheckOut)
                            {
                                if (!Node.IsCheckedOut)
                                {
                                    // If not checked out, add allow check-out
                                    allowSave = false;
                                    allowCheckout = canApprove;
                                }
                            }

                            if (Node.IsCheckedOut)
                            {
                                // If checked out by current user, allow check-in
                                int checkedOutBy = Node.DocumentCheckedOutByUserID;
                                if (checkedOutBy == Tree.UserInfo.UserID)
                                {
                                    allowUndoCheckout = true;
                                    allowCheckin = true;
                                }
                                else
                                {
                                    bool checkinAll = UserInfoProvider.IsAuthorizedPerResource("CMS.Content", "CheckInAll", Node.NodeSiteName, Tree.UserInfo);
                                    allowCheckin = checkinAll;
                                    allowUndoCheckout = checkinAll;
                                    allowSave = false;
                                }
                            }

                            if (si != null)
                            {
                                // Document approval
                                if (!Node.IsCheckedOut)
                                {
                                    // When document is under edit or custom step, user can approve and changes are not automatically published, allow approve
                                    if (si.StepIsEdit || !si.StepIsDefault || !wi.IsBasic)
                                    {
                                        if (!autoPublishChanges && canApprove)
                                        {
                                            allowApprove = true;
                                        }
                                    }

                                    // Reject action
                                    bool canReject = WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Reject);
                                    // When step allows reject, user can approve and changes are not automatically published, allow reject
                                    if (si.StepAllowReject && !autoPublishChanges && canReject)
                                    {
                                        allowReject = true;
                                    }

                                    // Archive action
                                    bool canArchive = WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Archive);
                                    if ((!si.StepIsArchived || !wi.IsBasic) && canArchive)
                                    {
                                        allowArchive = true;
                                    }
                                }

                                // Disable reject button for publish step if there is no version available
                                if (si.StepIsPublished && (Node.DocumentCheckedOutVersionHistoryID <= 0))
                                {
                                    allowReject = false;
                                }

                                if (si.StepIsAction)
                                {
                                    // Action is processing
                                    if (ProcessingAction)
                                    {
                                        allowApprove = false;
                                        allowArchive = false;
                                        allowReject = false;

                                    }
                                    else
                                    {
                                        // There are no next steps
                                        if (NextSteps.Count == 0)
                                        {
                                            // Hide approve and archive buttons
                                            allowApprove = false;
                                            allowArchive = false;
                                        }
                                    }

                                    // Disable actions if in action step
                                    allowSave = false;
                                    allowCheckin = false;
                                    allowCheckout = false;
                                    allowUndoCheckout = false;
                                }

                                // Disable save for advanced workflow in published or archived step
                                if (!wi.IsBasic && (si.StepIsPublished || si.StepIsArchived))
                                {
                                    allowSave = false;
                                    allowCheckin = false;
                                    allowCheckout = false;
                                    allowUndoCheckout = false;
                                    allowCreateNewVersion = true;
                                }
                            }
                        }
                        else
                        {
                            allowSave = (si == null) || (!si.StepIsAction && !ProcessingAction);
                        }
                    }
                    else
                    {
                        allowApplyWorkflow = WorkflowManager.CanUserManageWorkflow(CurrentUser, Node.NodeSiteName);
                    }
                }
            }


            // Add results to the request
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.SAVE), allowSave, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.APPROVE), allowApprove, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.PUBLISH), allowApprove, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.REJECT), allowReject, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.CHECKIN), allowCheckin, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.CHECKOUT), allowCheckout, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.UNDO_CHECKOUT), allowUndoCheckout, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.ARCHIVE), allowArchive, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.CREATE_VERSION), allowCreateNewVersion, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(DocumentComponentEvents.APPLY_WORKFLOW), allowApplyWorkflow, false);
        }


        /// <summary>
        /// Checks document limitations and permissions
        /// </summary>
        /// <param name="showMessage">Indicates if info message should be shown</param>
        private bool IsAuthorized(bool showMessage)
        {
            bool authorized = true;

            switch (Mode)
            {
                case FormModeEnum.Insert:
                    // Insert new document
                    {
                        // Check allowed document type
                        if (!DocumentHelper.IsDocumentTypeAllowed(ParentNode, NewNodeClassID))
                        {
                            authorized = false;
                            if (showMessage)
                            {
                                ShowError(ResHelper.GetString("Content.ChildClassNotAllowed", ResourceCulture));
                            }
                        }

                        // Check license limitations
                        CheckLicenseLimitations(Mode, ref authorized);

                        // Check prerequisites
                        if (ParentNodeID <= 0)
                        {
                            authorized = false;
                            if (showMessage)
                            {
                                ShowError(ResHelper.GetString("newpage.invalidparentnode", ResourceCulture));
                            }
                        }

                        // Check permissions
                        SimpleDocumentManagerEventArgs args = new SimpleDocumentManagerEventArgs(ParentNode, Mode);
                        if (!RaiseCheckPermissions(args))
                        {
                            authorized = false;
                            if (showMessage)
                            {
                                ShowError(args.ErrorMessage);
                            }
                        }
                    }
                    break;

                case FormModeEnum.InsertNewCultureVersion:
                    // Insert as new culture version
                    {
                        // Check license limitations
                        CheckLicenseLimitations(Mode, ref authorized);

                        // Check permissions
                        SimpleDocumentManagerEventArgs args = new SimpleDocumentManagerEventArgs(ParentNode, Mode);
                        if (!RaiseCheckPermissions(args))
                        {
                            authorized = false;
                            if (showMessage)
                            {
                                ShowError(args.ErrorMessage);
                            }
                        }
                    }
                    break;

                case FormModeEnum.Update:
                    // Update mode
                    {
                        // Check permissions
                        SimpleDocumentManagerEventArgs args = new SimpleDocumentManagerEventArgs(Node, Mode);
                        if (!RaiseCheckPermissions(args))
                        {
                            authorized = false;
                            if (showMessage)
                            {
                                ShowError(args.ErrorMessage);
                            }
                        }
                    }
                    break;
            }

            return authorized;
        }

        #endregion


        #region "Consistency methods"


        /// <summary>
        /// Checks the default consistency for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override void CheckDefaultConsistency(SimpleDocumentManagerEventArgs args)
        {
            if (Node != null)
            {
                // Store step ID
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                    case FormModeEnum.InsertNewCultureVersion:
                        break;

                    default:
                        {
                            WorkflowStepInfo step = WorkflowManager.GetStepInfo(Node);
                            int nodeStepId = (step != null) ? step.StepID : 0;

                            // Check integrity
                            if (nodeStepId != CurrentStepID)
                            {
                                args.IsValid = false;
                                ShowWarning(GetString("ContentEdit.ConsistencyCheck"));

                                if (RegisterSaveChangesScript && SaveChanges)
                                {
                                    // Register changes to save data on next action
                                    ScriptHelper.RegisterStartupScript(Page, typeof(string), "DocumentManagementChanges_" + ClientID, ScriptHelper.GetScript("Changed();"));
                                }
                            }
                        }
                        break;
                }
            }
            else
            {
                // Document is not initialized, do not continue
                args.IsValid = false;
            }
        }

        #endregion


        #region "Action handling"

        /// <summary>
        /// Raises the post back event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            // Check consistency
            if (!RaiseCheckConsistency(new SimpleDocumentManagerEventArgs(Node, Mode, eventArgument)))
            {
                return;
            }

            if (IsActionAllowed(eventArgument))
            {
                switch (eventArgument)
                {
                    case ComponentEvents.SAVE:
                        SaveDocument();
                        break;

                    case DocumentComponentEvents.CHECKIN:
                        CheckInDocument(ActionComment);
                        break;

                    case DocumentComponentEvents.APPROVE:
                        {
                            int stepId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            ApproveDocument(stepId, ActionComment);
                        }
                        break;

                    case DocumentComponentEvents.REJECT:
                        {
                            int historyId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            RejectDocument(historyId, ActionComment);
                        }
                        break;

                    case DocumentComponentEvents.PUBLISH:
                        PublishDocument(ActionComment);
                        break;

                    case DocumentComponentEvents.ARCHIVE:
                        {
                            int stepId = ValidationHelper.GetInteger(HiddenArguments.Value, 0);
                            ArchiveDocument(stepId, ActionComment);
                        }
                        break;

                    case DocumentComponentEvents.CREATE_VERSION:
                        CreateNewVersion();
                        break;

                    case DocumentComponentEvents.CHECKOUT:
                        CheckOutDocument();
                        break;

                    case DocumentComponentEvents.UNDO_CHECKOUT:
                        UndoCheckOutDocument();
                        break;
                }
            }

            // Clear comment
            ClearComment();
        }


        /// <summary>
        /// Updates the current document
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used to update the document</param>
        public void UpdateDocument(bool useDocumentHelper)
        {
            UpdateDocumentInternal(useDocumentHelper);

            if (!useDocumentHelper)
            {
                ProcessAdditionalActions();
            }
        }


        /// <summary>
        /// Saves document.
        /// </summary>
        public bool SaveDocument()
        {
            return SaveDocument(null);
        }


        /// <summary>
        /// Saves document. Content changed flag is cleared if document was saved.
        /// </summary>
        /// <param name="actionContext">Context of the action within which was the document save called.</param>
        protected bool SaveDocument(string actionContext)
        {
            var saved = SaveDocumentInternal(actionContext);

            // Keep content changed flag if save wasn't performed and changed should be saved
            SetContentChanged(!saved && SaveChanges);

            return saved;
        }


        /// <summary>
        /// Processes additional document actions (task logging, search index task creation...). Substitutes the actions from DocumentHelper if not used.
        /// </summary>
        protected void ProcessAdditionalActions()
        {
            // Get task type
            TaskTypeEnum taskType;

            switch (Mode)
            {
                case FormModeEnum.Insert:
                case FormModeEnum.InsertNewCultureVersion:
                    taskType = TaskTypeEnum.CreateDocument;
                    break;

                default:
                    taskType = TaskTypeEnum.UpdateDocument;
                    break;
            }

            // Log synchronization task
            DocumentSynchronizationHelper.LogDocumentChange(Node, taskType, Tree);

            // Update search index for
            if (Node.PublishedVersionExists && SearchIndexInfoProvider.SearchEnabled)
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, Node.GetSearchID(), Node.DocumentID);
            }
        }


        /// <summary>
        /// Updates the current document
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used to update the document</param>
        protected void UpdateDocumentInternal(bool useDocumentHelper)
        {
            // Update the document
            Node.Update(useDocumentHelper);

            ShowConfirmation(ResHelper.GetString("General.ChangesSaved", ResourceCulture));
        }


        /// <summary>
        /// Saves document.
        /// </summary>
        /// <param name="actionContext">Context of the action within which was the document save called.</param>
        protected bool SaveDocumentInternal(string actionContext)
        {
            if (Node == null)
            {
                return false;
            }

            // Check limitations
            if (!IsAuthorized(true))
            {
                return false;
            }

            // Prepare arguments
            var originalStep = WorkflowManager.GetStepInfo(Node);
            var workflow = originalStep != null ? Workflow : null;
            DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, ComponentEvents.SAVE)
            {
                Workflow = workflow,
                SaveActionContext = actionContext,
                UseDocumentHelper = UseDocumentHelper
            };

            bool isWorkflowFinished = false;
            bool documentCreated = false;
            bool useParentNodeGroupId = Tree.UseParentNodeGroupID;

            try
            {
                // Validate data
                if (RaiseValidateData(args))
                {
                    // Raise before action event
                    if (!RaiseBeforeAction(args))
                    {
                        return false;
                    }

                    // Store current mode - it can be changed in events
                    FormModeEnum mode = Mode;

                    // Handle modes
                    switch (mode)
                    {
                        case FormModeEnum.Insert:
                            // Insert the document
                            if (Node != null)
                            {
                                Tree.UseParentNodeGroupID = (GroupID > 0);

                                // Set document group
                                if (GroupID > 0)
                                {
                                    Node.SetValue("NodeGroupID", GroupID);
                                }

                                // Try to get data to save
                                if (!RaiseSaveData(args))
                                {
                                    return false;
                                }

                                if (args.UpdateDocument)
                                {
                                    // Ensure consistency
                                    EnsureDocumentsConsistency();

                                    Node.Insert(ParentNode, args.UseDocumentHelper);
                                }

                                documentCreated = true;

                                // Clear already evaluated properties (document data context has changed)
                                ClearProperties();

                                // Register java-script to refresh UI
                                RegisterRefreshScript(!CreateAnother);
                            }
                            break;

                        case FormModeEnum.Update:
                            {
                                // If not using check-in/check-out, check out automatically
                                if (AutoCheck && args.UseDocumentHelper)
                                {
                                    // Check out - do not log separate event to event log; do not create search task, consequent update creates it
                                    using (new CMSActionContext { LogEvents = false, CreateSearchTask = !args.UpdateDocument })
                                    {
                                        var nextStep = VersionManager.CheckOut(Node, Node.IsPublished, true);
                                        isWorkflowFinished = IsWorkflowFinished(nextStep);
                                    }
                                }

                                // Try to get data to save
                                if (!RaiseSaveData(args))
                                {
                                    return false;
                                }

                                if (args.UpdateDocument)
                                {
                                    UpdateDocumentInternal(args.UseDocumentHelper);
                                }

                                // Clear already evaluated properties (document data context has changed)
                                ClearProperties();

                                // Register java-script to refresh UI
                                RegisterRefreshScript(CloseDialog);
                            }
                            break;

                        case FormModeEnum.InsertNewCultureVersion:
                            {
                                // Clear workflow information
                                DocumentHelper.ClearWorkflowInformation(Node);

                                Tree.UseParentNodeGroupID = (GroupID > 0);

                                // Try to get data to save
                                if (!RaiseSaveData(args))
                                {
                                    return false;
                                }

                                if (args.UpdateDocument)
                                {
                                    // Ensure consistency
                                    EnsureDocumentsConsistency();

                                    Node.InsertAsNewCultureVersion(CultureCode, args.UseDocumentHelper);
                                }
                                documentCreated = true;

                                // Clear already evaluated properties (document data context has changed)
                                ClearProperties();

                                // Register java-script to refresh UI
                                RegisterRefreshScript(true);
                            }
                            break;
                    }

                    // Ensure additional actions
                    if (!args.UseDocumentHelper)
                    {
                        ProcessAdditionalActions();
                    }

                    // Check in the document
                    if (AutoCheck && args.UseDocumentHelper)
                    {
                        if (mode == FormModeEnum.Update)
                        {
                            // Check if workflow hasn't been finished
                            if (!isWorkflowFinished && (Node.DocumentWorkflowStepID != 0))
                            {
                                // Check in - do not log separate event to event log
                                using (new CMSActionContext { LogEvents = false })
                                {
                                    VersionManager.CheckIn(Node, null);
                                }
                            }
                        }
                        else
                        {
                            // Automatically publish
                            // Check if allowed 'Automatically publish changes'
                            if (Workflow.WorkflowAutoPublishChanges)
                            {
                                Node.MoveToPublishedStep();
                            }
                        }
                    }

                    // Raise after action event
                    args.Workflow = Workflow;
                    args.WorkflowFinished = isWorkflowFinished;
                    if (!RaiseAfterAction(args))
                    {
                        return false;
                    }

                    if (mode == FormModeEnum.Update)
                    {
                        ShowConfirmation(ResHelper.GetString("General.ChangesSaved", ResourceCulture));
                    }
                }
                else
                {
                    if ((MessagesPlaceHolder == null) || string.IsNullOrEmpty(MessagesPlaceHolder.ErrorText))
                    {
                        ShowError(ResHelper.GetString("General.SaveError", ResourceCulture));
                    }

                    return false;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Delete document if created
                switch (Mode)
                {
                    case FormModeEnum.Insert:
                    case FormModeEnum.InsertNewCultureVersion:
                        // Delete the document only if it was created
                        if (documentCreated && (Node.DocumentID > 0))
                        {
                            DocumentHelper.DeleteDocument(Node, Tree, false, true);
                        }
                        break;
                }

                RaiseSaveFailed(args);

                string message = ResHelper.GetString("General.ErrorDuringSave", ResourceCulture);
                if ((MessagesPlaceHolder == null) || string.IsNullOrEmpty(MessagesPlaceHolder.ErrorText))
                {
                    ShowError(message);
                }

                // Log error
                EventLogProvider.LogException("Content", "SaveDocument", ex, Node.NodeSiteID, message);

                return false;
            }
            finally
            {
                // Restore settings
                Tree.UseParentNodeGroupID = useParentNodeGroupId;
            }

            return true;
        }


        /// <summary>
        /// Ensures documents consistency (Automatic blog post hierarchy etc.)
        /// </summary>
        public void EnsureDocumentsConsistency()
        {
            switch (Mode)
            {
                case FormModeEnum.Insert:
                case FormModeEnum.InsertNewCultureVersion:
                    {
                        // Ensure BlogMonth 
                        if (string.Equals(Node.NodeClassName, "cms.blogpost", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ParentNode = DocumentHelper.EnsureBlogPostHierarchy(Node, ParentNode, Tree);
                            // Update parent node ID
                            if (ParentNode != null)
                            {
                                ParentNodeID = ParentNode.NodeID;
                            }
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Creates new document version. (Moves document to edit step.)
        /// </summary>
        public bool CreateNewVersion()
        {
            if (Node == null)
            {
                return false;
            }

            // Check limitations
            if (!IsAuthorized(true))
            {
                return false;
            }

            // Prepare arguments
            WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
            DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.CREATE_VERSION)
            {
                Workflow = Workflow,
                UseDocumentHelper = UseDocumentHelper
            };

            try
            {
                // Validate data
                if (RaiseValidateData(args))
                {
                    // Raise before action event
                    if (!RaiseBeforeAction(args))
                    {
                        return false;
                    }

                    WorkflowStepInfo targetStep = null;

                    if (Mode == FormModeEnum.Update)
                    {
                        // Create new minor version for archived and published step
                        if ((originalStep != null) && (originalStep.StepIsArchived || originalStep.StepIsPublished))
                        {
                            targetStep = VersionManager.CreateNewVersion(Node);
                        }
                        else
                        {
                            targetStep = WorkflowManager.MoveToFirstStep(Node);
                        }
                    }

                    // Raise after action event
                    args.WorkflowFinished = IsWorkflowFinished(targetStep);
                    if (!RaiseAfterAction(args))
                    {
                        return false;
                    }

                    ClearContentChanged();

                    var message = ResHelper.GetString((targetStep != null) ? "Document.VersionCreated" : "WorfklowProperties.WorkflowFinished", ResourceCulture);

                    ShowConfirmation(message);
                }
                else
                {
                    if ((MessagesPlaceHolder == null) || string.IsNullOrEmpty(MessagesPlaceHolder.ErrorText))
                    {
                        ShowError(ResHelper.GetString("General.SaveError", ResourceCulture));
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                RaiseSaveFailed(args);

                string message = ResHelper.GetString("General.ErrorDuringSave", ResourceCulture);
                if ((MessagesPlaceHolder == null) || string.IsNullOrEmpty(MessagesPlaceHolder.ErrorText))
                {
                    ShowError(message);
                }

                // Log error
                EventLogProvider.LogException("Content", "CreateNewVersion", ex, Node.NodeSiteID, message);
            }

            return true;
        }


        /// <summary>
        /// Approves document.
        /// </summary>
        /// <param name="stepId">Workflow step ID (optional)</param>
        /// <param name="comment">Comment</param>
        public void ApproveDocument(int stepId, string comment)
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.APPROVE);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Approve)))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.APPROVE)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Save the changes first
                if (AutoCheck)
                {
                    if (SaveChanges)
                    {
                        if (!SaveDocument(DocumentComponentEvents.APPROVE))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // No version exists, create one
                        if (Node.DocumentCheckedOutVersionHistoryID == 0)
                        {
                            // Check out the document
                            var step = VersionManager.CheckOut(Node, Node.IsPublished, true);
                            if (step != null)
                            {
                                // Check in the document
                                VersionManager.CheckIn(Node, null);
                            }
                        }

                        // Try to load data
                        if (!RaiseLoadData(args))
                        {
                            return;
                        }
                    }
                }

                // Approve the document - Go to next workflow step
                // Get next step
                WorkflowStepInfo nextStep;

                // Approve the document
                if (stepId == 0)
                {
                    nextStep = WorkflowManager.MoveToNextStep(Node, comment);
                }
                else
                {
                    WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                    nextStep = WorkflowManager.MoveToSpecificNextStep(Node, s, comment);
                }

                // Raise after action event
                args.WorkflowFinished = IsWorkflowFinished(nextStep);
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                ClearContentChanged();

                // Ensure correct message is displayed
                if (nextStep != null)
                {
                    if (nextStep.StepIsPublished)
                    {
                        AddConfirmation(ResHelper.GetString("workflowstep.customtopublished", ResourceCulture));
                    }
                    else if (args.OriginalStep.StepIsEdit && !nextStep.StepIsPublished)
                    {
                        AddConfirmation(ResHelper.GetString("workflowstep.edittocustom", ResourceCulture));
                    }
                    else if (!args.OriginalStep.StepIsEdit && !nextStep.StepIsPublished && !nextStep.StepIsArchived)
                    {
                        AddConfirmation(ResHelper.GetString("workflowstep.customtocustom", ResourceCulture));
                    }
                    else
                    {
                        AddConfirmation(ResHelper.GetString("ContentEdit.WasApproved", ResourceCulture));
                    }
                }
                else
                {
                    AddConfirmation(ResHelper.GetString("WorfklowProperties.WorkflowFinished", ResourceCulture));
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);

                // Log error
                EventLogProvider.LogException("Content", "Approve", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Publishes document
        /// </summary>
        /// <param name="comment">Comment</param>
        public void PublishDocument(string comment)
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.PUBLISH);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Publish)))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.PUBLISH)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Save the changes first
                if (AutoCheck)
                {
                    if (SaveChanges)
                    {
                        if (!SaveDocument(DocumentComponentEvents.PUBLISH))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // No version exists, create one
                        if (Node.DocumentCheckedOutVersionHistoryID == 0)
                        {
                            // Check out the document
                            var step = VersionManager.CheckOut(Node, Node.IsPublished, true);
                            if (step != null)
                            {
                                // Check in the document
                                VersionManager.CheckIn(Node, null);
                            }
                        }

                        // Try to load data
                        if (!RaiseLoadData(args))
                        {
                            return;
                        }
                    }
                }

                // Publish the document - Go to published workflow step
                // Get next step
                var nextStep = WorkflowManager.PublishDocument(Node, comment);

                // Raise after action event
                args.WorkflowFinished = IsWorkflowFinished(nextStep);
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                ClearContentChanged();

                // Ensure correct message is displayed
                if (nextStep != null)
                {
                    if (nextStep.StepIsPublished)
                    {
                        AddConfirmation(ResHelper.GetString("workflowstep.customtopublished", ResourceCulture));
                    }
                    else
                    {
                        AddConfirmation(ResHelper.GetString("ContentEdit.WasApproved", ResourceCulture));
                        ShowWarning(ResHelper.GetString("ContentEdit.PublishWasApproved", ResourceCulture));
                    }
                }
                else
                {
                    if (Workflow == null)
                    {
                        AddConfirmation(ResHelper.GetString("WorfklowProperties.WorkflowFinished", ResourceCulture));
                    }
                    else
                    {
                        AddConfirmation(ResHelper.GetString("ContentEdit.WasApproved", ResourceCulture));
                        ShowWarning(ResHelper.GetString("ContentEdit.PublishWasApproved", ResourceCulture));
                    }
                }
            }
            catch (PermissionException ex)
            {
                AddConfirmation(ResHelper.GetString("ContentEdit.WasApproved", ResourceCulture));
                ShowWarning(ResHelper.GetString("ContentEdit.PublishWasApproved", ResourceCulture));
                EventLogProvider.LogException("Content", "Publish", ex, Node.NodeSiteID);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "Publish", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Archives document
        /// </summary>
        /// <param name="stepId">Workflow step ID (optional)</param>
        /// <param name="comment">Comment</param>
        public void ArchiveDocument(int stepId, string comment)
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.ARCHIVE);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Archive)))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.ARCHIVE)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Save the changes first
                if (AutoCheck)
                {
                    if (SaveChanges)
                    {
                        if (!SaveDocument(DocumentComponentEvents.ARCHIVE))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // Try to load data
                        if (!RaiseLoadData(args))
                        {
                            return;
                        }
                    }
                }

                WorkflowStepInfo nextStep;

                // Archive the document
                if (stepId == 0)
                {
                    nextStep = WorkflowManager.ArchiveDocument(Node, comment);
                }
                else
                {
                    WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepId);
                    if (!s.StepIsArchived)
                    {
                        throw new WorkflowException("[CMSDocumentManager.ArchiveDocument]: Target step is not archived step.");
                    }
                    nextStep = WorkflowManager.MoveToSpecificNextStep(Node, s, comment);
                }

                // Raise after action event
                args.WorkflowFinished = IsWorkflowFinished(nextStep);
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                ClearContentChanged();

                // Ensure correct message is displayed
                var message = ResHelper.GetString((nextStep != null) ? "workflowstep.archived" : "WorfklowProperties.WorkflowFinished", ResourceCulture);

                AddConfirmation(message);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "Archive", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Rejects document
        /// </summary>
        /// <param name="comment">Comment</param>
        public void RejectDocument(string comment)
        {
            RejectDocument(0, comment);
        }


        /// <summary>
        /// Rejects document
        /// </summary>
        /// <param name="historyId">Workflow history ID (optional)</param>
        /// <param name="comment">Comment</param>
        public void RejectDocument(int historyId, string comment)
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.REJECT);
            if (CheckPermissions && (!RaiseCheckPermissions(permArgs) || !WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Reject)))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.REJECT)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Save the document first
                if (AutoCheck)
                {
                    if (SaveChanges)
                    {
                        if (!SaveDocument(DocumentComponentEvents.REJECT))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // Try to load data
                        if (!RaiseLoadData(args))
                        {
                            return;
                        }
                    }
                }

                // Reject the document
                if (historyId == 0)
                {
                    WorkflowManager.MoveToPreviousStep(Node, comment);
                }
                else
                {
                    var history = WorkflowHistoryInfoProvider.GetWorkflowHistoryInfo(historyId);
                    if (history != null)
                    {
                        // Use step clone
                        WorkflowStepInfo s = WorkflowStepInfoProvider.GetWorkflowStepInfo(history.StepID).Clone();
                        s.RelatedHistoryID = historyId;
                        WorkflowManager.MoveToPreviousSpecificStep(Node, s, comment);
                    }
                    else
                    {
                        throw new NullReferenceException("[CMSDocumentManager.RejectDocument]: Missing workflow history.");
                    }
                }

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                ClearContentChanged();

                AddConfirmation(ResHelper.GetString("ContentEdit.WasRejected", ResourceCulture));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "Reject", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Checks-in document
        /// </summary>
        /// <param name="comment">Comment</param>
        public void CheckInDocument(string comment)
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.CHECKIN);
            if (CheckPermissions && !RaiseCheckPermissions(permArgs))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.CHECKIN)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Save the document first
                if (SaveChanges)
                {
                    if (!SaveDocument(DocumentComponentEvents.CHECKIN))
                    {
                        return;
                    }
                }
                else
                {
                    // Try to load data
                    if (!RaiseLoadData(args))
                    {
                        return;
                    }
                }

                // Check in the document        
                VersionManager.CheckIn(Node, null, comment);

                // Raise after action event
                if (!RaiseAfterAction(args))
                {
                    return;
                }

                ClearContentChanged();

                AddConfirmation(ResHelper.GetString("ContentEdit.WasCheckedIn", ResourceCulture));
            }
            catch (WorkflowException)
            {
                AddError(ResHelper.GetString("EditContent.DocumentCannotCheckIn", ResourceCulture));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "Checkin", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Check-outs document
        /// </summary>
        public void CheckOutDocument()
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.CHECKOUT);
            if (CheckPermissions && !RaiseCheckPermissions(permArgs))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            try
            {
                // Prepare arguments
                var originalStep = WorkflowManager.GetStepInfo(Node);
                var args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.CHECKOUT)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Ensure version
                VersionManager.EnsureVersion(Node, Node.IsPublished);

                // Check out the document
                var nextStep = VersionManager.CheckOut(Node);
                args.WorkflowFinished = IsWorkflowFinished(nextStep);
                
                // Raise after action event
                RaiseAfterAction(args);

                ClearContentChanged();
            }
            catch (WorkflowException)
            {
                AddError(ResHelper.GetString("EditContent.DocumentCannotCheckOut", ResourceCulture));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "Checkout", ex, Node.NodeSiteID, message);
            }
        }


        private static bool IsWorkflowFinished(WorkflowStepInfo nextStep)
        {
            return nextStep == null;
        }


        /// <summary>
        /// Undoes check-out document
        /// </summary>
        public void UndoCheckOutDocument()
        {
            if (Node == null)
            {
                return;
            }

            // Check modify permissions
            SimpleDocumentManagerEventArgs permArgs = new SimpleDocumentManagerEventArgs(Node, Mode, DocumentComponentEvents.UNDO_CHECKOUT);
            if (CheckPermissions && !RaiseCheckPermissions(permArgs))
            {
                AddError(permArgs.ErrorMessage);
                return;
            }

            // Check out the document
            try
            {
                // Prepare arguments
                WorkflowStepInfo originalStep = WorkflowManager.GetStepInfo(Node);
                DocumentManagerEventArgs args = new DocumentManagerEventArgs(Node, originalStep, Mode, DocumentComponentEvents.UNDO_CHECKOUT)
                {
                    Workflow = Workflow,
                    UseDocumentHelper = UseDocumentHelper
                };

                // Raise before action event
                if (!RaiseBeforeAction(args))
                {
                    return;
                }

                // Undo check out
                VersionManager.UndoCheckOut(Node);

                // Raise after action event
                RaiseAfterAction(args);

                ClearContentChanged();
            }
            catch (WorkflowException)
            {
                AddError(ResHelper.GetString("EditContent.DocumentCannotUndoCheckOut", ResourceCulture));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = ResHelper.GetString("doc.ErrorDuringAction", ResourceCulture);
                AddError(message);
                EventLogProvider.LogException("Content", "UndoCheckout", ex, Node.NodeSiteID, message);
            }
        }


        /// <summary>
        /// Clears current node.
        /// </summary>
        public void ClearNode()
        {
            ClearProperties();

            if (IsLiveSite)
            {
                mNode = null;
            }
            else
            {
                DocumentContext.EditedDocument = null;
            }
            mSourceNode = null;
        }


        /// <summary>
        /// Check license limitations for document or language version creation
        /// </summary>
        private void CheckLicenseLimitations(FormModeEnum mode, ref bool authorized)
        {
            // Get class name
            string className = (mode == FormModeEnum.Insert) ? NewNodeClass.ClassName : Node.ClassName;

            if (string.Equals(className, "cms.blog", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Blogs, ObjectActionEnum.Insert))
                {
                    authorized = false;
                    AddError(string.Format(ResHelper.GetString("cmsdesk.bloglicenselimits", ResourceCulture), ""));
                }
            }

            if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Documents, ObjectActionEnum.Insert))
            {
                authorized = false;
                AddError(string.Format(ResHelper.GetString("cmsdesk.documentslicenselimits", ResourceCulture), ""));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the reload page attributes
        /// </summary>
        /// <param name="reloadImmediately">Indicates whether wopener should be reloaded immediately</param>
        public void RegisterRefreshScript(bool reloadImmediately)
        {
            if (SetRefreshFlag)
            {
                TreeNode node = Node;
                if (node != null)
                {
                    // Indicate that the parent page should be reloaded when the dialog is closed
                    string reloadScript = "window.refreshPageOnClose = true;";

                    if (SetRedirectPageFlag)
                    {
                        // Redirect the parent page to the new document
                        string url = UrlResolver.ResolveUrl(DocumentURLProvider.GetUrl(node));
                        // Include culture to correct redirect
                        url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, CultureCode);
                        reloadScript += " window.reloadPageUrl =" + ScriptHelper.GetString(url) + ";";
                    }

                    reloadScript += " if (wopener.PassSavedNodeId) { wopener.PassSavedNodeId(" + node.NodeID + "); } ";

                    if (reloadImmediately)
                    {
                        if (!DeviceContext.CurrentDevice.IsMobile())
                        {
                            // Reload the parent page immediately
                            reloadScript += " if (wopener.RefreshWOpener) { wopener.RefreshWOpener(window); } ";
                        }
                        else
                        {
                            // Reload the parent page immediately and close the current dialog window for mobile devices (mobile devices open modal dialogs in a new tab)
                            reloadScript += " CloseDialog(); ";
                        }
                    }

                    ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), "ReloadDialogPage", ScriptHelper.GetScript(reloadScript));
                }
            }
        }


        /// <summary>
        /// Indicates if local messages placeholder is used
        /// </summary>
        public bool HasLocalMessagesPlaceHolder()
        {
            return (plcMess != null);
        }


        /// <summary>
        /// Shows default document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        public void ShowDocumentInfo(bool includeWorkflowInfo)
        {
            ShowDocumentInfo(includeWorkflowInfo, null);
        }


        /// <summary>
        /// Shows default document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        /// <param name="message">Additional message</param>
        public void ShowDocumentInfo(bool includeWorkflowInfo, string message)
        {
            // Return explicitly set info message
            if (!string.IsNullOrEmpty(DocumentInfo))
            {
                return;
            }

            // Return info only for update mode
            if ((Mode != FormModeEnum.Update) || (Node == null))
            {
                return;
            }

            string documentInfo = GetDocumentInfo(includeWorkflowInfo);
            DocumentInfo = AddText(documentInfo, message);
        }


        /// <summary>
        /// Gets the workflow for current document
        /// </summary>
        private BaseInfo GetWorkflow()
        {
            if (Node != null)
            {
                // Get workflow info
                return Node.GetWorkflow();
            }

            return null;
        }


        /// <summary>
        /// Gets default document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        public string GetDocumentInfo(bool includeWorkflowInfo)
        {
            string documentInfo = null;

            bool authorized = true;

            // Check permissions
            SimpleDocumentManagerEventArgs args = new SimpleDocumentManagerEventArgs(Node, Mode);
            if (!RaiseCheckPermissions(args))
            {
                authorized = false;
                documentInfo = args.ErrorMessage;
            }

            // If authorized
            if (authorized)
            {
                // If workflow not null, process the workflow information to display the items
                if (Workflow != null)
                {
                    bool isActionStep = (Step != null) && Step.StepIsAction;
                    if (includeWorkflowInfo)
                    {
                        bool autoPublishChanges = Workflow.WorkflowAutoPublishChanges;
                        bool useCheckInCheckOut = !AutoCheck;
                        bool canApprove = WorkflowManager.CheckStepPermissions(Node, WorkflowActionEnum.Approve);

                        if (!isActionStep)
                        {
                            // Check-in, Check-out information
                            if (!Node.IsCheckedOut)
                            {
                                // Show information only if used and user can approve
                                if (useCheckInCheckOut && canApprove)
                                {
                                    // New version must be created to allow editing for advanced workflow
                                    if (!Workflow.IsBasic && (Step != null) && (Step.StepIsPublished || Step.StepIsArchived))
                                    {
                                        documentInfo = ResHelper.GetString("EditContent.DocumentNewVersion", ResourceCulture);
                                    }
                                    else
                                    {
                                        // If not checked out, add the check-out information
                                        documentInfo = ResHelper.GetString("EditContent.DocumentCheckedIn", ResourceCulture);
                                    }
                                }
                            }
                            else
                            {
                                // If checked out by current user, show information
                                int checkedOutBy = Node.DocumentCheckedOutByUserID;
                                if (checkedOutBy == Tree.UserInfo.UserID)
                                {
                                    documentInfo = ResHelper.GetString("EditContent.DocumentCheckedOut", ResourceCulture);
                                }
                                // Checked out by another user
                                else
                                {
                                    UserInfo user = UserInfoProvider.GetUserInfo(checkedOutBy);
                                    string userName = (user != null) ? HTMLHelper.HTMLEncode(user.GetFormattedUserName(IsLiveSite)) : "";
                                    documentInfo = String.Format(ResHelper.GetString("EditContent.DocumentCheckedOutByAnother", ResourceCulture), userName);
                                }
                            }
                        }

                        // Not authorized to approve
                        if (!canApprove)
                        {
                            documentInfo = AddText(documentInfo, ResHelper.GetString("EditContent.NotAuthorizedToApprove", ResourceCulture));
                        }

                        // If workflow isn't auto publish or step name isn't 'published' or 'check-in/check-out' is allowed then show current step name
                        if ((Step != null) && (!autoPublishChanges || !Step.StepIsPublished || useCheckInCheckOut))
                        {
                            documentInfo = AddText(documentInfo, String.Format(ResHelper.GetString("EditContent.CurrentStepInfo", ResourceCulture), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(Step.StepDisplayName)), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(Workflow.WorkflowDisplayName))));
                        }
                    }

                    // Display action step message
                    if (isActionStep)
                    {
                        if (!ProcessingAction && (NextSteps.Count > 0))
                        {
                            // Display warning message
                            documentInfo = AddText(documentInfo, ResHelper.GetString("workflow.actionwarning", ResourceCulture));
                        }
                    }
                }
            }

            return documentInfo;
        }


        /// <summary>
        /// Gets script for save another action
        /// </summary>
        public string GetSaveAnotherScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var elm = document.getElementById('", HiddenAnotherFlag.ClientID, "'); if(elm!=null){ elm.value = '1';}");
            return sb.ToString();
        }


        /// <summary>
        /// Gets script for save &amp; close action
        /// </summary>
        public string GetSaveAndCloseScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var elm = document.getElementById('", HiddenCloseFlag.ClientID, "'); if(elm!=null){ elm.value = '1';}");
            return sb.ToString();
        }


        /// <summary>
        /// Gets allow submit script for save changes support
        /// </summary>
        /// <returns>Allow submit script for save changes support</returns>
        public string GetAllowSubmitScript()
        {
            return "if(window.CMSContentManager) { CMSContentManager.allowSubmit = true; }";
        }


        /// <summary>
        /// Gets submit script for save changes support
        /// </summary>
        /// <returns>Submit script for save changes support</returns>
        public string GetSubmitScript()
        {
            return "if(window.CMSContentManager) { CMSContentManager.submitAction(); } ";
        }


        /// <summary>
        /// Clears content changed flag
        /// </summary>
        public void ClearContentChanged()
        {
            SetContentChanged(false);
        }

        #endregion


        #region "Private methods"

        private void SetContentChanged(bool changed)
        {
            hdnContentChanged.Value = changed ? "true" : "";
        }


        /// <summary>
        /// Registers component event.
        /// </summary>
        /// <param name="eventType">Component event type.</param>
        /// <param name="action">Document action delegate.</param>
        private void RegisterComponentEvent(string eventType, Action<int> action)
        {
            // Register event
            ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, eventType, (s, args) =>
            {
                // Check consistency
                if (!RaiseCheckConsistency(new SimpleDocumentManagerEventArgs(Node, Mode, eventType)))
                {
                    return;
                }

                if (IsActionAllowed(eventType))
                {
                    // Get action argument
                    var com = args as CommandEventArgs;
                    int actionArgument = ValidationHelper.GetInteger((com != null) ? com.CommandArgument : HiddenArguments.Value, 0);

                    // Fire action
                    action(actionArgument);
                }

                // Clear comment
                ClearComment();
            });
        }


        /// <summary>
        /// Registers javascript scripts
        /// </summary>
        private void RegisterScripts()
        {
            // Register scripts
            if (RegisterSaveChangesScript)
            {
                ScriptHelper.RegisterSaveChanges(Page, UseFullFormSaveChanges);
            }

            if (AddComment)
            {
                ScriptHelper.RegisterDialogScript(Page);
            }

            if (!IsLiveSite)
            {
                ScriptHelper.RegisterPageLoadedEvent(Page);
            }

            ScriptHelper.RegisterCompletePageScript(Page);
        }


        /// <summary>
        /// Clears current comment
        /// </summary>
        private void ClearComment()
        {
            hdnComment.Value = "";
        }


        /// <summary>
        /// Gets the document based on manager settings.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        private TreeNode GetDocument(string cultureCode = null)
        {
            if (String.IsNullOrEmpty(cultureCode))
            {
                cultureCode = CultureCode;
            }

            if (!UseDocumentHelper)
            {
                // In case of update non-versioned data field don't use document helper and get published version
                return Tree.SelectSingleNode(NodeID, cultureCode);
            }

            return DocumentHelper.GetDocument(NodeID, cultureCode, Tree);
        }

        #endregion
    }
}