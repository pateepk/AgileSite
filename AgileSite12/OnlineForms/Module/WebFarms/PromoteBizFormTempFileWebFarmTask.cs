using CMS.Base;
using CMS.Core;
using CMS.FormEngine;
using CMS.IO;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm task used to promote temporary files attached to a form.
    /// </summary>
    public class PromoteBizFormTempFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the name of the site the form is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets the name of the temporary file to be promoted.
        /// </summary>
        public string TempFileName { get; set; }


        /// <summary>
        /// Gets or sets the name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Returns true if the synchronization of the BizForm files is allowed.
        /// </summary>
        /// <returns></returns>
        public override bool ConditionMethod()
        {
            return FormSynchronization.SynchronizeBizFormFiles;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PromoteBizFormTempFileWebFarmTask"/> class.
        /// </summary>
        public PromoteBizFormTempFileWebFarmTask()
        {
            TaskTarget = "promotebizformtempfile";
        }


        /// <summary>
        /// Processes the web farm task by moving the temporary file identified by <see cref="TempFileName"/> to a new location specified by <see cref="FileName"/>.
        /// </summary>
        public override void ExecuteTask()
        {
            string filesFolderPath = FormHelper.GetBizFormFilesFolderPath(SiteName);
            string tempFilesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();

            string filePath = Path.Combine(filesFolderPath, FileName);
            string tempFilePath = Path.Combine(tempFilesFolderPath, TempFileName);

            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);
            File.Move(tempFilePath, filePath);
        }
    }
}
