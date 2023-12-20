using CMS.Base;
using CMS.Core;
using CMS.FormEngine;
using CMS.IO;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm task used to update files attached to a form.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateBizFormFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the name of the site the form is assigned to.
        /// Null if <see cref="IsTempFile"/> is true.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets the name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets the flag indicating whether file is a temporary file. False by default.
        /// </summary>
        public bool IsTempFile { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBizFormFileWebFarmTask"/> class.
        /// </summary>
        public UpdateBizFormFileWebFarmTask()
        {
            TaskTarget = "updatebizformfile";
        }


        /// <summary>
        /// Returns true if the synchronization of the BizForm files is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return FormSynchronization.SynchronizeBizFormFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="StorageHelper.SaveFileToDisk(string, BinaryData, bool)"/> method upon bizform file given by <see cref="SiteName"/> and <see cref="FileName"/>.
        /// </summary>
        public override void ExecuteTask()
        {
            string filesFolderPath;
            if (IsTempFile)
            {
                filesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();
            }
            else
            {
                filesFolderPath = FormHelper.GetBizFormFilesFolderPath(SiteName);
            }

            string filePath = Path.Combine(filesFolderPath, FileName);

            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);
            StorageHelper.SaveFileToDisk(filePath, TaskBinaryData);
        }
    }
}
