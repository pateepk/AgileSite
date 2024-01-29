using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for controls which provide external edit action
    /// </summary>
    public abstract class ExternalEditControl : CMSAdminControl
    {
        #region "Variables"

        /// <summary>
        /// Edit icon
        /// </summary>
        protected CMSGridActionButton btnEdit = null;


        /// <summary>
        /// Edit label
        /// </summary>
        protected Label lblEdit = null;


        /// <summary>
        /// WbDav control type.
        /// </summary>
        protected FileTypeEnum mControlType = FileTypeEnum.Unknown;

        private string mSiteName;
        private string mRefreshScript;
        private string mGroupName;
        private int? mGroupID;
        private string mCssClass = "external-edit-icon";
        private bool mCheckPermission = true;
        private bool mPerformFullPostback = true;
        private bool mEnabledResult = true;

        private string mMediaLibraryName;

        private Guid mMetaFileGUID = Guid.Empty;
        private TreeProvider mTreeProvider;
        private TreeNode mTreeNode;
        private WorkflowManager mWorkflowManager;
        private GeneralizedInfo mGroupInfo;
        private GeneralizedInfo mMediaLibraryInfo;
        private TreeNode mGroupNode;
        private SiteInfo mSiteInfo;
        private WorkflowInfo mWorkflowInfo;
        private WorkflowScopeInfo mWorkflowScopeInfo;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Gets group info.
        /// </summary>
        protected GeneralizedInfo GroupInfo
        {
            get
            {
                if ((mGroupInfo == null) && (GroupID > 0))
                {
                    mGroupInfo = ModuleCommands.CommunityGetGroupInfo(GroupID);
                }

                return mGroupInfo;
            }
        }


        /// <summary>
        /// Gets media library info.
        /// </summary>
        protected GeneralizedInfo MediaLibraryInfo
        {
            get
            {
                if ((mMediaLibraryInfo == null) && (MediaFileLibraryID > 0))
                {
                    mMediaLibraryInfo = ModuleCommands.MediaLibraryGetMediaLibraryInfo(MediaFileLibraryID);
                }

                return mMediaLibraryInfo;
            }
        }


        /// <summary>
        /// Group parent node.
        /// </summary>
        protected TreeNode GroupNode
        {
            get
            {
                if ((mGroupNode == null) && (GroupInfo != null))
                {
                    Guid nodeGuid = ValidationHelper.GetGuid(GroupInfo.GetValue("GroupNodeGUID"), Guid.Empty);
                    if (nodeGuid != Guid.Empty)
                    {
                        mGroupNode = TreeProvider.SelectSingleNode(nodeGuid, NodeCultureCode, SiteName);
                    }
                }

                return mGroupNode;
            }
        }


        /// <summary>
        /// Gets site info.
        /// </summary>
        protected SiteInfo SiteInfo
        {
            get
            {
                return mSiteInfo ?? (mSiteInfo = SiteInfoProvider.GetSiteInfo(SiteName));
            }
        }


        /// <summary>
        /// Gets CSS class for icon consisting of CssClass and icon related classes.
        /// </summary>
        protected string CompleteCssClass
        {
            get
            {
                return CssClass + " icon-arrow-right-rect" + (Enabled && EnabledResult ? "" : " icon-disabled");
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                if (string.IsNullOrEmpty(mSiteName))
                {
                    mSiteName = SiteContext.CurrentSiteName;
                }

                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Gets type of control (attachment, media, metafile...).
        /// </summary>
        public FileTypeEnum ControlType
        {
            get
            {
                return mControlType;
            }
        }


        /// <summary>
        /// Gets or sets state enable.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["Enabled"], true);
            }
            set
            {
                ViewState["Enabled"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of file for editing.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets Group ID.
        /// </summary>
        public int GroupID
        {
            get
            {
                // For media files get group ID from media library    
                if (mGroupID == null)
                {
                    mGroupID = (MediaLibraryInfo != null) ? ValidationHelper.GetInteger(MediaLibraryInfo.GetValue("LibraryGroupID"), 0) : 0;
                }
                return mGroupID.Value;
            }
            set
            {
                mGroupID = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of community group name.
        /// </summary>
        public string GroupName
        {
            get
            {
                if (string.IsNullOrEmpty(mGroupName) && (GroupInfo != null))
                {
                    // For attachment or media file get group name
                    if ((ControlType == FileTypeEnum.Attachment) || (ControlType == FileTypeEnum.MediaFile))
                    {
                        mGroupName = ValidationHelper.GetString(GroupInfo.GetValue("GroupName"), null);
                    }
                }

                return mGroupName;
            }
            set
            {
                mGroupName = value;
            }
        }


        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        public string CssClass
        {
            get
            {
                return mCssClass;
            }
            set
            {
                mCssClass = value;
            }
        }


        /// <summary>
        /// Gets or sets refresh script.
        /// </summary>
        public string RefreshScript
        {
            get
            {
                if (mRefreshScript == null)
                {
                    mRefreshScript = "if (window.SubmitAction) {window.SubmitAction();} ";
                    string functionName = string.IsNullOrEmpty(FormID) ? "RefreshForm" : FormID + "_RefreshForm";
                    mRefreshScript += "if (window." + functionName + ") {window." + functionName + "();} ";

                    if (TreeNode != null)
                    {
                        mRefreshScript += "if (window.RefreshTree) {window.RefreshTree(" + TreeNode.NodeParentID + ", " + TreeNode.NodeID + ");}";
                    }
                }
                return mRefreshScript;
            }
            set
            {
                mRefreshScript = value;
            }
        }


        /// <summary>
        /// Client ID of the parent form.
        /// </summary>
        public string FormID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the text of a caption label. If empty, no caption is rendered.
        /// </summary>
        public string LabelText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the css class of caption label.
        /// </summary>
        public string LabelCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the permissions should be checked.
        /// </summary>
        public bool CheckPermission
        {
            get
            {
                return mCheckPermission;
            }
            set
            {
                mCheckPermission = value;
            }
        }


        /// <summary>
        /// Determines whether control should perform full postback.
        /// </summary>
        public bool PerformFullPostback
        {
            get
            {
                return mPerformFullPostback;
            }
            set
            {
                mPerformFullPostback = value;
            }
        }


        /// <summary>
        /// Returns TRUE if the icon is enabled.
        /// </summary>
        public bool EnabledResult
        {
            get
            {
                return mEnabledResult;
            }
            private set
            {
                mEnabledResult = value;
            }
        }

        #endregion


        #region "Attachment specific properties"

        /// <summary>
        /// Gets or sets node alias path.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets document culture.
        /// </summary>
        public string NodeCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets attachment field name.
        /// </summary>
        public string AttachmentFieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets document.
        /// </summary>
        protected TreeNode TreeNode
        {
            get
            {
                return mTreeNode ?? (mTreeNode = DocumentHelper.GetDocument(SiteName, NodeAliasPath, NodeCultureCode, false, null, null, null, TreeProvider.ALL_LEVELS, false, null, TreeProvider));
            }
        }


        /// <summary>
        /// Gets tree provider.
        /// </summary>
        protected TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider(MembershipContext.AuthenticatedUser));
            }
        }


        /// <summary>
        /// Gets workflow manager.
        /// </summary>
        protected WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Gets workflow info.
        /// </summary>
        protected WorkflowInfo WorkflowInfo
        {
            get
            {
                return mWorkflowInfo ?? (mWorkflowInfo = WorkflowManager.GetNodeWorkflow(TreeNode));
            }
        }


        /// <summary>
        /// Gets node workflow scope.
        /// </summary>
        protected WorkflowScopeInfo WorkflowScopeInfo
        {
            get
            {
                return mWorkflowScopeInfo ?? (mWorkflowScopeInfo = WorkflowManager.GetNodeWorkflowScope(TreeNode));
            }
        }

        #endregion


        #region "Media file specific properties"

        /// <summary>
        /// Gets or sets media library name.
        /// </summary>
        public string MediaLibraryName
        {
            get
            {
                if (MediaLibraryInfo != null)
                {
                    mMediaLibraryName = ValidationHelper.GetString(MediaLibraryInfo.GetValue("LibraryName"), null);
                }

                return mMediaLibraryName;
            }
        }


        /// <summary>
        /// Gets or sets media library ID.
        /// </summary>
        public int MediaFileLibraryID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets media file path.
        /// </summary>
        public string MediaFilePath
        {
            get;
            set;
        }

        #endregion


        #region "Meta file specific properties"

        /// <summary>
        /// Gets or sets the GUID of meta file for editing.
        /// </summary>
        public Guid MetaFileGUID
        {
            get
            {
                return mMetaFileGUID;
            }
            set
            {
                mMetaFileGUID = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Control PreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (PerformFullPostback)
            {
                ControlsHelper.RegisterPostbackControl(this);
            }

            ReloadData();
            CheckEnable();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the URL of file for editing.
        /// </summary>
        /// <returns>URL of file for editing.</returns>
        protected abstract string GetUrl();


        /// <summary>
        /// Reload controls data.
        /// </summary>
        /// <param name="forceReload">Indicates if controls</param>
        public override void ReloadData(bool forceReload)
        {
            base.ReloadData(forceReload);

            string toolTip = ResHelper.GetString("externaledit.editdocument.tooltip");

            var script = GetEditScript();
            if (!String.IsNullOrEmpty(script))
            {
                // Add refresh script if the document uses worklow and check-in/check-out isn't allowed and auto publish changes isn't allowed
                if (ProcessRefreshScript())
                {
                    script += RefreshScript;
                }

                script += " return false;";
            }


            btnEdit = new CMSGridActionButton
            {
                IconCssClass = "icon-arrow-right-rect",
                ToolTip = toolTip,
                OnClientClick = script,
                Enabled = Enabled && EnabledResult
            };


            Controls.Clear();
            Controls.Add(btnEdit);

            // Add label
            if (!string.IsNullOrEmpty(LabelText))
            {
                LiteralControl ltlSeparator = new LiteralControl("&nbsp;");
                ltlSeparator.ID = "ltlSeparator";
                ltlSeparator.EnableViewState = false;

                lblEdit = new Label();
                lblEdit.ID = "lblExtEdit";
                lblEdit.CssClass = LabelCssClass;
                lblEdit.Text = LabelText;
                lblEdit.ToolTip = toolTip;

                if (Enabled)
                {
                    lblEdit.Attributes["onclick"] = script;
                }

                // Add controls to collection
                Controls.Add(ltlSeparator);
                Controls.Add(lblEdit);
            }

            if (forceReload)
            {
                CheckEnable();
            }
        }


        /// <summary>
        /// Gets the editing action script
        /// </summary>
        protected abstract string GetEditScript();


        /// <summary>
        /// Check if the refresh script should be processed.
        /// </summary>
        protected bool ProcessRefreshScript()
        {
            if (ControlType == FileTypeEnum.Attachment)
            {
                // Try to get workflow only for attachment control
                if (WorkflowInfo == null)
                {
                    return false;
                }

                // Refresh only for published or archived step
                bool process = (TreeNode.IsInPublishStep || TreeNode.IsArchived);

                // Check workflow change
                bool workflowChanged = false;
                if (WorkflowScopeInfo != null)
                {
                    workflowChanged = (WorkflowInfo.WorkflowID != WorkflowScopeInfo.ScopeWorkflowID);
                }
                // Check if document uses workflow and check-out/check-in is disabled or document lost workflow
                process &= ((!WorkflowInfo.UseCheckInCheckOut(SiteName) && !WorkflowInfo.WorkflowAutoPublishChanges) || workflowChanged || (WorkflowScopeInfo == null));

                return process;
            }

            return false;
        }


        /// <summary>
        /// Checks whether the button should be enabled or not.
        /// </summary>
        protected virtual void CheckEnable()
        {
            if (!Enabled)
            {
                DisableButton();
            }
        }


        /// <summary>
        /// Performs actions necessary for disabling button.
        /// </summary>
        protected void DisableButton()
        {
            EnabledResult = false;
            btnEdit.Enabled = false;
        }


        /// <summary>
        /// Disables the button with the given error message
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        protected void DisableButton(string errorMessage)
        {
            DisableButton();

            btnEdit.ToolTip = errorMessage;

            if (lblEdit != null)
            {
                lblEdit.ToolTip = errorMessage;
                lblEdit.Attributes["onclick"] = "return false;";
                lblEdit.Style.Add("cursor", "default");
            }
        }


        /// <summary>
        /// Initializes edit control.
        /// </summary>
        /// <param name="data">Data row with data on related media file</param>
        /// <param name="liveSite">If true, the control is loaded in live site context</param>
        /// <param name="treeNode">Attachment parent document</param>
        public void InitFrom(IDataContainer data, bool liveSite, TreeNode treeNode)
        {
            switch (ControlType)
            {
                case FileTypeEnum.MediaFile:
                    {
                        // Media file
                        MediaFileLibraryID = ValidationHelper.GetInteger(data.GetValue("FileLibraryID"), 0);
                        MediaFilePath = ValidationHelper.GetString(data.GetValue("FilePath"), null);
                        Enabled = true;
                    }
                    break;

                case FileTypeEnum.Attachment:
                    {
                        // Document attachment
                        Enabled = (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(treeNode, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Allowed);

                        string className = ValidationHelper.GetString(data.GetValue("ClassName"), "");

                        if (className.EqualsCSafe(SystemDocumentTypes.File, true))
                        {
                            FileName = ValidationHelper.GetString(data.GetValue("AttachmentName"), null);
                            AttachmentFieldName = "FileAttachment";
                            NodeAliasPath = ValidationHelper.GetString(data.GetValue("NodeAliasPath"), null);
                            NodeCultureCode = ValidationHelper.GetString(data.GetValue("DocumentCulture"), null);
                        }
                        else
                        {
                            FileName = ValidationHelper.GetString(data.GetValue("AttachmentName"), null);
                            NodeAliasPath = treeNode.NodeAliasPath;
                            NodeCultureCode = treeNode.DocumentCulture;
                        }

                        // Set Group ID for live site
                        GroupID = liveSite ? treeNode.GetValue("NodeGroupID", 0) : 0;
                    }
                    break;

                case FileTypeEnum.MetaFile:
                    {
                        // Meta file
                        MetaFileGUID = ValidationHelper.GetGuid(data.GetValue("MetaFileGUID"), Guid.Empty);
                        FileName = ValidationHelper.GetString(data.GetValue("MetaFileName"), null);
                        Enabled = true;
                    }
                    break;
            }

            IsLiveSite = liveSite;
            Visible = true;
        }

        #endregion
    }
}