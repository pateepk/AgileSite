namespace CMS.DocumentEngine
{
    /// <summary>
    /// Workflow events
    /// </summary>
    public static class WorkflowEvents
    {
        #region "Locking"

        /// <summary>
        /// Fires when the document is checked out
        /// </summary>
        public static WorkflowHandler CheckOut = new WorkflowHandler { Name = "WorkflowEvents.CheckOut" };


        /// <summary>
        /// Fires when the document is checked in
        /// </summary>
        public static WorkflowHandler CheckIn = new WorkflowHandler { Name = "WorkflowEvents.CheckIn" };


        /// <summary>
        /// Fires when the undo checkout is performed
        /// </summary>
        public static WorkflowHandler UndoCheckOut = new WorkflowHandler { Name = "WorkflowEvents.UndoCheckOut" };

        #endregion


        #region "Workflow"

        /// <summary>
        /// Fires when the document is approved (moved to next step)
        /// </summary>
        public static WorkflowHandler Approve = new WorkflowHandler { Name = "WorkflowEvents.Approve" };


        /// <summary>
        /// Fires when the document is rejected (moved to previous step)
        /// </summary>
        public static WorkflowHandler Reject = new WorkflowHandler { Name = "WorkflowEvents.Reject" };


        /// <summary>
        /// Fires when a version of the document is published
        /// </summary>
        public static WorkflowHandler Publish = new WorkflowHandler { Name = "WorkflowEvents.Publish" };


        /// <summary>
        /// Fires when a version of the document is archived
        /// </summary>
        public static WorkflowHandler Archive = new WorkflowHandler { Name = "WorkflowEvents.Archive" };

        #endregion


        #region "Versions"

        /// <summary>
        /// Fires when a version of the document is saved (during DocumentHelper.UpdateDocument under workflow etc.)
        /// </summary>
        public static WorkflowHandler SaveVersion = new WorkflowHandler { Name = "WorkflowEvents.SaveVersion" };


        /// <summary>
        /// Fires when a new attachment is added to the document or and attachment is updated (during DocumentHelper.AddAtachment under workflow etc.)
        /// </summary>
        public static WorkflowHandler SaveAttachmentVersion = new WorkflowHandler { Name = "WorkflowEvents.SaveAttachmentVersion" };


        /// <summary>
        /// Fires when an attachment is removed from he document (during DocumentHelper.DeleteAtachment under workflow etc.)
        /// </summary>
        public static WorkflowHandler RemoveAttachmentVersion = new WorkflowHandler { Name = "WorkflowEvents.RemoveAttachmentVersion" };

        #endregion


        #region "Workflow action"
        
        /// <summary>
        /// Fires when a workflow action is executed
        /// </summary>
        public static WorkflowActionHandler Action = new WorkflowActionHandler { Name = "WorkflowEvents.Action" };

        #endregion
    }
}