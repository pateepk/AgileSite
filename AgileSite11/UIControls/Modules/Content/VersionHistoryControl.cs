using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.WorkflowEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base history version control.
    /// </summary>
    public abstract class VersionHistoryControl : CMSUserControl
    {
        #region "Variables"

        private TreeProvider mTreeProvider;
        private WorkflowStepInfo mWorkflowStepInfo;
        private WorkflowManager mWorkflowManager;
        private VersionManager mVersionManager;
        private TreeNode mNode;

        #endregion


        #region "Properties"

        /// <summary>
        /// Identifier of edited node.
        /// </summary>
        public int NodeID
        {
            get
            {
                int mNodeID = ValidationHelper.GetInteger(ViewState["NodeID"], 0);
                if (mNodeID == 0)
                {
                    mNodeID = QueryHelper.GetInteger("nodeid", 0);
                }
                return mNodeID;
            }
            set
            {
                ViewState["NodeID"] = value;
            }
        }


        /// <summary>
        /// Indicates if returned nodes should be combined with appropriate nodes of default culture in case they are not localized. It applies only if you're using multilingual support. The default value is false.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["CombineWithDefaultCulture"], TreeProvider.CombineWithDefaultCulture);
            }
            set
            {
                ViewState["CombineWithDefaultCulture"] = value;
            }
        }


        /// <summary>
        /// Culture of document.
        /// </summary>
        public string DocumentCulture
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DocumentCulture"], LocalizationContext.PreferredCultureCode);
            }
            set
            {
                ViewState["DocumentCulture"] = value;
            }
        }


        /// <summary>
        /// Currently edited node.
        /// </summary>
        public virtual TreeNode Node
        {
            get
            {
                return mNode ?? (mNode = DocumentHelper.GetDocument(NodeID, DocumentCulture, CombineWithDefaultCulture, TreeProvider));
            }
            set
            {
                mNode = value;
                if (mNode != null)
                {
                    // Refresh parameters to get updated document 
                    NodeID = mNode.NodeID;
                    DocumentCulture = mNode.DocumentCulture;
                }
            }
        }


        /// <summary>
        /// Tree provider.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider(CurrentUser));
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Workflow manager.
        /// </summary>
        public WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(TreeProvider));
            }
            set
            {
                mWorkflowManager = value;
            }
        }


        /// <summary>
        /// Returns workflow step information of current node.
        /// </summary>
        public WorkflowStepInfo WorkflowStepInfo
        {
            get
            {
                if ((mWorkflowStepInfo == null) && (Node != null))
                {
                    mWorkflowStepInfo = WorkflowManager.GetStepInfo(Node);
                }

                return mWorkflowStepInfo;
            }
            set
            {
                mWorkflowStepInfo = value;
            }
        }


        /// <summary>
        /// Version manager.
        /// </summary>
        public VersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = VersionManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Determines whether current user is allowed to destroy current node.
        /// </summary>
        public bool CanDestroy
        {
            get
            {
                return CurrentUser.IsAuthorizedPerDocument(Node, NodePermissionsEnum.Destroy) == AuthorizationResultEnum.Allowed;
            }
        }


        /// <summary>
        /// Determines whether current user is allowed to modify given node.
        /// </summary>
        public bool CanModify
        {
            get
            {
                return CurrentUser.IsAuthorizedPerDocument(Node, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Allowed;
            }
        }


        /// <summary>
        /// Returns identifier of user for whom the node is currently checked out.
        /// </summary>
        public int CheckedOutByUserID
        {
            get
            {
                return Node != null ? Node.DocumentCheckedOutByUserID : 0;
            }
        }


        /// <summary>
        /// Determines whether given node is checked out by another user.
        /// </summary>
        public bool CheckedOutByAnotherUser
        {
            get
            {
                return (CheckedOutByUserID != 0) && (CheckedOutByUserID != CurrentUser.UserID);
            }
        }


        /// <summary>
        /// Determines whether user has permission 'CheckInAll'.
        /// </summary>
        public bool CanCheckIn
        {
            get
            {
                return CurrentUser.IsAuthorizedPerResource("CMS.Content", "CheckInAll");
            }
        }

        #endregion
    }
}