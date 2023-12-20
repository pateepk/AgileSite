using System;

using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Web farm task used to update forum attachments.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateForumAttachmentWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which forum is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets name of the file attachment to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets extension of the file attachment to be updated.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file attachment to be updated.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="UpdateForumAttachmentWebFarmTask"/>.
        /// </summary>
        public UpdateForumAttachmentWebFarmTask()
        {
            TaskTarget = "ForumAttachmentUpdate";
        }


        /// <summary>
        /// Returns true if the synchronization of the forum attachments is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return ForumsSynchronization.SynchronizeForumAttachments;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ForumAttachmentInfoProvider.SaveAttachmentFileToDisk(string, string, string, string, BinaryData, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Save the binary data to the disk
            if (TaskBinaryData.Data != null)
            {
                ForumAttachmentInfoProvider.SaveAttachmentFileToDisk(SiteName, FileGuid.ToString(), FileName, FileExtension, TaskBinaryData.Data, true);
            }
            else
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteName);
                if (si != null)
                {
                    // Drop the cache dependencies
                    CacheHelper.TouchKey($"forumattachment|{FileGuid}|{si.SiteID}");
                }
            }
        }
    }
}
