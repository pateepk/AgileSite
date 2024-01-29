using System;

using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Abstract control for document attachments control.
    /// </summary>
    public abstract class AttachmentsControl : FormEngineUserControl, IInputControl
    {
        #region "Constants"

        /// <summary>
        /// Maximal length of attachment name
        /// </summary>
        public const int ATTACHMENT_NAME_LIMIT = 90;

        #endregion


        #region "Variables"

        private Guid mGroupGUID = Guid.Empty;
        private Guid mFormGUID = Guid.Empty;
        private string mGUIDColumnName;
        private bool mAllowChangeOrder = true;
        private bool mAllowEditing = true;
        private bool mAllowDelete = true;
        private bool mAllowPaging = true;
        private string mPageSize = "0";
        private int mDefaultPageSize = 25;
        private int mResizeToWidth;
        private int mResizeToHeight;
        private int mResizeToMaxSideSize;
        private string mAllowedExtensions;
        private bool mHideActions;
        private object mValue;
        private Guid mInnerAttachmentGUID = Guid.Empty;
        private bool mCheckPermissions = true;
        private string mInnerLoadingDivClass = "NewAttachmentLoading";
        private string mNodeOriginalSiteName;
        private TreeNode mNode;
        private bool? mFullRefresh;

        #endregion


        #region "Properties"

        /// <summary>
        /// Site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                return DocumentManager.SiteName;
            }
            set
            {
                DocumentManager.SiteName = value;
            }
        }
        
        
        /// <summary>
        /// Site name of the original node (if linked).
        /// </summary>
        protected string OriginalNodeSiteName
        {
            get
            {
                if ((mNodeOriginalSiteName == null) && (Node != null))
                {
                    mNodeOriginalSiteName = SiteInfoProvider.GetSiteName(Node.OriginalNodeSiteID);
                }
                return mNodeOriginalSiteName;
            }
        }


        /// <summary>
        /// Indicates if automatic check-in/check-out should be used
        /// </summary>
        protected bool AutoCheck
        {
            get
            {
                return DocumentManager.AutoCheck;
            }
        }


        /// <summary>
        /// Gets or sets tree provider.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return DocumentManager.Tree;
            }
            set
            {
                DocumentManager.Tree = value;
            }
        }


        /// <summary>
        /// Gets tree node.
        /// </summary>
        public TreeNode Node
        {
            get
            {
                if (mNode != null)
                {
                    return mNode;
                }

                mNode = DocumentManager.Node;
                if (mNode == null)
                {
                    return null;
                }

                // Always work with original node
                mNode = TreeProvider.GetOriginalNode(mNode);

                if (UsesWorkflow)
                {
                    mNode = DocumentHelper.GetDocument(mNode, TreeProvider);
                }

                return mNode;
            }
        }


        /// <summary>
        /// Gets version manager.
        /// </summary>
        protected WorkflowManager WorkflowManager
        {
            get
            {
                return DocumentManager.WorkflowManager;
            }
        }


        /// <summary>
        /// Value of the control.
        /// </summary>
        public override object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Indicates if the permissions should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Indicates whether grouped attachments should be displayed.
        /// </summary>
        public virtual Guid GroupGUID
        {
            get
            {
                return mGroupGUID;
            }
            set
            {
                mGroupGUID = value;
            }
        }


        /// <summary>
        /// Name of document attachment column.
        /// </summary>
        public virtual string GUIDColumnName
        {
            get
            {
                return mGUIDColumnName;
            }
            set
            {
                mGUIDColumnName = value;
            }
        }


        /// <summary>
        /// Indicates if user is allowed to change the order of the attachments.
        /// </summary>
        public virtual bool AllowChangeOrder
        {
            get
            {
                return mAllowChangeOrder;
            }
            set
            {
                mAllowChangeOrder = value;
            }
        }


        /// <summary>
        /// Indicates if user can edit the attachments.
        /// </summary>
        public virtual bool AllowEditing
        {
            get
            {
                return mAllowEditing;
            }
            set
            {
                mAllowEditing = value;
            }
        }


        /// <summary>
        /// Indicates if user can delete the attachments.
        /// </summary>
        public virtual bool AllowDelete
        {
            get
            {
                return mAllowDelete;
            }
            set
            {
                mAllowDelete = value;
            }
        }


        /// <summary>
        /// Indicates if paging is allowed.
        /// </summary>
        public virtual bool AllowPaging
        {
            get
            {
                return mAllowPaging;
            }
            set
            {
                mAllowPaging = value;
            }
        }


        /// <summary>
        /// Defines size of the page for paging.
        /// </summary>
        public virtual string PageSize
        {
            get
            {
                return mPageSize;
            }
            set
            {
                mPageSize = value;
            }
        }


        /// <summary>
        /// Default page size.
        /// </summary>
        public virtual int DefaultPageSize
        {
            get
            {
                return mDefaultPageSize;
            }
            set
            {
                mDefaultPageSize = value;
            }
        }


        /// <summary>
        /// Width of the uploaded image.
        /// </summary>
        public virtual int ResizeToWidth
        {
            get
            {
                return mResizeToWidth;
            }
            set
            {
                mResizeToWidth = value;
            }
        }


        /// <summary>
        /// Height of the uploaded image.
        /// </summary>
        public virtual int ResizeToHeight
        {
            get
            {
                return mResizeToHeight;
            }
            set
            {
                mResizeToHeight = value;
            }
        }


        /// <summary>
        /// Maximal side size of the uploaded image.
        /// </summary>
        public virtual int ResizeToMaxSideSize
        {
            get
            {
                return mResizeToMaxSideSize;
            }
            set
            {
                mResizeToMaxSideSize = value;
            }
        }


        /// <summary>
        /// List of allowed extensions.
        /// </summary>
        public virtual string AllowedExtensions
        {
            get
            {
                return mAllowedExtensions;
            }
            set
            {
                mAllowedExtensions = value;
            }
        }


        /// <summary>
        /// Specifies the node of document for which the attachments should be displayed.
        /// </summary>
        public virtual int NodeID
        {
            get
            {
                return DocumentManager.NodeID;
            }
            set
            {
                DocumentManager.NodeID = value;
            }
        }


        /// <summary>
        /// Specifies the document for which the attachments should be displayed.
        /// </summary>
        public virtual int DocumentID
        {
            get
            {
                return DocumentManager.DocumentID;
            }
            set
            {
                DocumentManager.DocumentID = value;
            }
        }


        /// <summary>
        /// Specifies the node parent node.
        /// </summary>
        public virtual int NodeParentNodeID
        {
            get
            {
                return DocumentManager.ParentNodeID;
            }
            set
            {
                DocumentManager.ParentNodeID = value;
            }
        }


        /// <summary>
        /// Specifies the node class name.
        /// </summary>
        public virtual string NodeClassName
        {
            get
            {
                return DocumentManager.NewNodeClassName;
            }
            set
            {
                DocumentManager.NewNodeClassName = value;
            }
        }


        /// <summary>
        /// Indicates if the actions should be hidden.
        /// </summary>
        public virtual bool HideActions
        {
            get
            {
                return mHideActions;
            }
            set
            {
                mHideActions = value;
            }
        }


        /// <summary>
        /// Defines the form GUID; indicates that the temporary attachment will be handled.
        /// </summary>
        public virtual Guid FormGUID
        {
            get
            {
                return mFormGUID;
            }
            set
            {
                mFormGUID = value;
            }
        }


        /// <summary>
        /// Inner attachment GUID (GUID of temporary attachment created for new culture version).
        /// </summary>
        public virtual Guid InnerAttachmentGUID
        {
            get
            {
                return mInnerAttachmentGUID;
            }
            set
            {
                mInnerAttachmentGUID = value;
            }
        }


        /// <summary>
        /// CSS class of the new attachment loading element.
        /// </summary>
        public string InnerLoadingDivClass
        {
            get
            {
                return mInnerLoadingDivClass;
            }
            set
            {
                mInnerLoadingDivClass = value;
            }
        }


        /// <summary>
        /// If true, control does not process the data.
        /// </summary>
        public new virtual bool StopProcessing
        {
            get
            {
                return base.StopProcessing;
            }

            set
            {
                base.StopProcessing = value;
            }
        }


        /// <summary>
        /// Indicates if workflow is used.
        /// </summary>
        protected bool UsesWorkflow
        {
            get
            {
                if (Node != null)
                {
                    // Check if the document uses workflow
                    return (Node.GetWorkflow() != null);
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates if full refresh is needed after upload. Can be used first on OnPreRender event because accessing to this property sooner may assign incorrect value.
        /// </summary>
        protected bool FullRefresh
        {
            get
            {
                if (!mFullRefresh.HasValue)
                {
                    mFullRefresh = CheckFullRefresh();
                }

                return mFullRefresh.Value;
            }
        }


        #endregion


        #region "Events"

        /// <summary>
        /// On check permissions event.
        /// </summary>
        public event CheckPermissionsEventHandler OnCheckPermissions;


        /// <summary>
        /// Raises when the current file requests to be deleted.
        /// </summary>
        public event EventHandler OnDeleteFile;


        /// <summary>
        /// Raises when a new file is given to be uploaded.
        /// </summary>
        public event EventHandler OnUploadFile;


        /// <summary>
        /// Raises the OnCheckPermissions event, returns true when event was fired.
        /// </summary>
        /// <param name="permissionType">Type of the permission to check</param>
        /// <param name="sender">Sender object</param>
        public bool RaiseOnCheckPermissions(string permissionType, AttachmentsControl sender)
        {
            if (OnCheckPermissions != null)
            {
                OnCheckPermissions(permissionType, sender);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Delete file event handler.
        /// </summary>
        public void RaiseDeleteFile(object sender, EventArgs e)
        {
            if (OnDeleteFile != null)
            {
                OnDeleteFile(this, e);
            }
        }


        /// <summary>
        /// Upload file event handler.
        /// </summary>
        public void RaiseUploadFile(object sender, EventArgs e)
        {
            if (OnUploadFile != null)
            {
                OnUploadFile(this, e);
            }
        }

        #endregion


        #region "Delegates"

        /// <summary>
        /// Delegate of event fired when permissions should be checked.
        /// </summary>
        /// <param name="permissionType">Type of a permission to check</param>
        /// <param name="sender">Sender</param>
        public delegate void CheckPermissionsEventHandler(string permissionType, AttachmentsControl sender);

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates if the control contains some data.
        /// </summary>
        public virtual bool HasData()
        {
            return false;
        }


        /// <summary>
        /// Reloads data of the control.
        /// </summary>
        public virtual void ReloadData()
        {
        }


        /// <summary>
        /// Reloads data of the control.
        /// </summary>
        /// <param name="forceReload">Indicates if data reload should be forced</param>
        public virtual void ReloadData(bool forceReload)
        {
        }


        /// <summary>
        /// Checks if full refresh is needed.
        /// </summary>
        private bool CheckFullRefresh()
        {
            if (!DocumentManager.AutoCheck)
            {
                return false;
            }

            // Check if full refresh should be performed after upload
            var step = DocumentManager.Step;
            return (step != null) && (step.StepIsPublished || step.StepIsArchived);
        }

        #endregion
    }
}