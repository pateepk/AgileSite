using System;

using CMS.Core;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to delete attachments.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteAttachmentWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which is file assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file to be deleted.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates whether directory will be deleted.
        /// </summary>
        public bool DeleteDirectory { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteAttachmentWebFarmTask"/>.
        /// </summary>
        public DeleteAttachmentWebFarmTask()
        {
            TaskTarget = "FileDelete";
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the attachments is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return DocumentSynchronization.SynchronizeAttachments && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AttachmentBinaryHelper.DeleteFile"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            AttachmentBinaryHelper.DeleteFile(FileGuid, SiteName, DeleteDirectory, true);
        }
    }
}
