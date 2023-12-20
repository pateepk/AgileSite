using CMS.Core;
using CMS.FormEngine;
using CMS.IO;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm task used to delete bizform files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteBizFormFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which bizform is assigned to.
        /// Null if <see cref="IsTempFile"/> is true.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets name of the file to be deleted.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets the flag indicating whether file is a temporary file. False by default.
        /// </summary>
        public bool IsTempFile { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteBizFormFileWebFarmTask"/>.
        /// </summary>
        public DeleteBizFormFileWebFarmTask()
        {
            TaskTarget = "deletebizformfile";
        }


        /// <summary>
        /// Returns true whether the synchronization for deleting bizform files is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return FormSynchronization.SynchronizeBizFormFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="File.Delete(string)"/> method upon bizform file given by <see cref="SiteName"/> and <see cref="FileName"/>.
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

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
