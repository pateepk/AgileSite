using System;

using CMS.Core;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to update attachment of documents.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateAttachmentWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which is file assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file to be updated.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Gets or sets name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets extension of the file to be updated.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates if files in destination folder with mask '[guid]*.*' should be deleted.
        /// </summary>
        public bool DeleteOldFiles { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="UpdateAttachmentWebFarmTask"/>.
        /// </summary>
        public UpdateAttachmentWebFarmTask()
        {
            TaskTarget = "FileUpload";
        }


        /// <summary>
        /// Returns true if the synchronization of the attachments is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return DocumentSynchronization.SynchronizeAttachments;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AttachmentBinaryHelper.SaveFileToDisk(string, string, string, string, BinaryData, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Save the binary data to the disk
            if (TaskBinaryData.Data != null)
            {
                AttachmentBinaryHelper.SaveFileToDisk(SiteName, FileGuid.ToString(), FileName, FileExtension, TaskBinaryData, DeleteOldFiles, true);
            }
            else
            {
                // Drop the cache dependencies
                CacheHelper.TouchKey($"attachment|{FileGuid}", false, false);
            }
        }
    }
}
