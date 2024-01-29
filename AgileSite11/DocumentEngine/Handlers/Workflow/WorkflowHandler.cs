using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Workflow handler
    /// </summary>
    public class WorkflowHandler : AdvancedHandler<WorkflowHandler, WorkflowEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="tree">Tree provider</param>
        public WorkflowHandler StartEvent(TreeNode document, TreeProvider tree)
        {
            WorkflowEventArgs e = new WorkflowEventArgs
            {
                Document = document,
                TreeProvider = tree
            };

            return StartEvent(e, tree.UseCustomHandlers);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="attachment">Attachment</param>
        /// <param name="tree">Tree provider</param>
        public WorkflowHandler StartEvent(TreeNode document, DocumentAttachment attachment, TreeProvider tree)
        {
            var e = new WorkflowEventArgs
            {
                Document = document,
                Attachment = attachment,
                TreeProvider = tree
            };

            return StartEvent(e, tree.UseCustomHandlers);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="versionNumber">Version number</param>
        /// <param name="versionComment">Version comment</param>
        /// <param name="tree">Tree provider</param>
        public WorkflowHandler StartEvent(TreeNode document, ref string versionNumber, ref string versionComment, TreeProvider tree)
        {
            var e = new WorkflowEventArgs
            {
                Document = document,
                VersionNumber = versionNumber,
                VersionComment = versionComment,
                TreeProvider = tree
            };

            var h = StartEvent(e, tree.UseCustomHandlers);

            versionComment = e.VersionComment;
            versionNumber = e.VersionNumber;

            return h;
        }
    }
}