using CMS.Core;

namespace CMS.Forums
{
    /// <summary>
    /// Web farm task used to delete forum attachments.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteForumAttachmentWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which forum is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets name of the file attachment which will be deleted.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates whether directory will be deleted.
        /// </summary>
        public bool DeleteDirectory { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteForumAttachmentWebFarmTask"/>.
        /// </summary>
        public DeleteForumAttachmentWebFarmTask()
        {
            TaskTarget = "ForumAttachmentDelete";
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the forum attachments is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return ForumsSynchronization.SynchronizeForumAttachments && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ForumAttachmentInfoProvider.DeleteAttachmentFile(string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            ForumAttachmentInfoProvider.DeleteAttachmentFile(SiteName, FileName, DeleteDirectory, true);
        }
    }
}
