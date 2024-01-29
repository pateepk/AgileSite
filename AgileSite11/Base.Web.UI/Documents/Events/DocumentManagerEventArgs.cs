using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.WorkflowEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Manager event arguments
    /// </summary>
    public class DocumentManagerEventArgs : SimpleDocumentManagerEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Document workflow
        /// </summary>
        public WorkflowInfo Workflow
        {
            get;
            set;
        }


        /// <summary>
        /// Original document workflow step
        /// </summary>
        public WorkflowStepInfo OriginalStep
        {
            get;
            private set;
        }


        /// <summary>
        /// Current document workflow step
        /// </summary>
        public WorkflowStepInfo CurrentStep
        {
            get
            {
                if (Node != null)
                {
                    return Node.WorkflowStep;
                }

                return null;
            }
        }


        /// <summary>
        /// Indicates if the document should be updated after save data event
        /// </summary>
        public bool UpdateDocument
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
        /// Context of the save action. When the document was saved as a part of another action.
        /// </summary>
        public string SaveActionContext
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if workflow was finished (There is no more workflow applied for the document.)
        /// </summary>
        public bool WorkflowFinished
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="mode">Manager mode</param>
        /// <param name="action">Action name</param>
        public DocumentManagerEventArgs(TreeNode node, WorkflowStepInfo originalStep, FormModeEnum mode, string action)
            : base(node, mode)
        {
            IsValid = true;
            UpdateDocument = true;
            UseDocumentHelper = true;
            OriginalStep = originalStep;
            ActionName = action;
        }

        #endregion
    }
}
