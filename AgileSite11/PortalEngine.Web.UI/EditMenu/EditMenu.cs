using System;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Base class for document edit menu control.
    /// </summary>
    public abstract class EditMenu : BaseEditMenu
    {
        #region "Button display options"

        /// <summary>
        /// Show the Save button.
        /// </summary>
        public bool ShowSave
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Delete button.
        /// </summary>
        public bool ShowDelete
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Apply workflow button.
        /// </summary>
        public bool ShowApplyWorkflow
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Check Out button.
        /// </summary>
        public bool ShowCheckOut
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Check In button.
        /// </summary>
        public bool ShowCheckIn
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Undo CheckOut button.
        /// </summary>
        public bool ShowUndoCheckOut
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Approve button.
        /// </summary>
        public bool ShowApprove
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Archive button.
        /// </summary>
        public bool ShowArchive
        {
            get;
            set;
        }


        /// <summary>
        /// Forces to show the Archive button (backward compatibility for basic workflow on Properties->Workflow tab).
        /// </summary>
        public bool ForceArchive
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Reject button.
        /// </summary>
        public bool ShowReject
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Submit To Approval button.
        /// </summary>
        public bool ShowSubmitToApproval
        {
            get;
            set;
        }


        /// <summary>
        /// Show the Properties button.
        /// </summary>
        public bool ShowProperties
        {
            get;
            set;
        }


        /// <summary>
        /// Show spell check button.
        /// </summary>
        public bool ShowSpellCheck
        {
            get;
            set;
        }


        /// <summary>
        /// If true, create another button can be displayed.
        /// </summary>
        public bool ShowCreateAnother
        {
            get;
            set;
        }


        /// <summary>
        /// If true, save and close button can be displayed
        /// </summary>
        public bool ShowSaveAndClose
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Node ID.
        /// </summary>
        public virtual int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Document node.
        /// </summary>
        public virtual TreeNode Node
        {
            get;
            protected set;
        }


        /// <summary>
        /// Document culture code.
        /// </summary>
        public virtual string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if workflow actions should be displayed and handled
        /// </summary>
        public virtual bool HandleWorkflow
        {
            get;
            set;
        }


        /// <summary>
        /// Name of validation group where action buttons belong to.
        /// </summary>
        public string ActionsValidationGroup
        {
            get;
            set;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Event raised when the ReloadMenu action is called.
        /// </summary>
        public event EventHandler OnBeforeReloadMenu;

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the OnBeforeReloadMenu when the ReloadMenu action is called.
        /// </summary>
        protected virtual void RaiseOnBeforeReloadMenu()
        {
            if (OnBeforeReloadMenu != null)
            {
                OnBeforeReloadMenu(this, null);
            }
        }

        #endregion
    }
}