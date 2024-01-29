using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Workflow event arguments
    /// </summary>
    public class WorkflowEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Tree provider
        /// </summary>
        public TreeProvider TreeProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Edited version of the document
        /// </summary>
        public TreeNode Document
        {
            get;
            set;
        }


        /// <summary>
        /// Published version of the document, available only for Published handler in after event
        /// </summary>
        public TreeNode PublishedDocument
        {
            get;
            set;
        }
        

        /// <summary>
        /// Document attachment, used where it makes sense: Save attachment version
        /// </summary>
        public DocumentAttachment Attachment
        {
            get;
            set;
        }


        /// <summary>
        /// Previous workflow step, used where it makes sense: After approve, After reject, After archive
        /// </summary>
        public WorkflowStepInfo PreviousStep
        {
            get;
            set;
        }


        /// <summary>
        /// Version history info record, used where it makes sense: Save version
        /// </summary>
        public VersionHistoryInfo VersionHistory
        {
            get;
            set;
        }


        /// <summary>
        /// Version number, used where it makes sense: Save version
        /// </summary>
        public string VersionNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Version comment, used where it makes sense: Save version
        /// </summary>
        public string VersionComment
        {
            get;
            set;
        }
    }
}