using System;

using CMS.Base;
using CMS.Core;

namespace CMS.IO
{
    /// <summary>
    /// Web farm task used to delete folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteFolderWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets path of the folder to be deleted.
        /// </summary>
        public string Path { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteFolderWebFarmTask"/>.
        /// </summary>
        public DeleteFolderWebFarmTask()
        {
            TaskTarget = "deletefolder";
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the physical files is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return CoreServices.WebFarm.SynchronizePhysicalFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="Directory.Delete(string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            if (String.IsNullOrEmpty(Path))
            {
                return;
            }

            // Get directory path for current application
            string directoryPath = StorageHelper.GetFullFolderPhysicalPath(Path, StorageTasks.WebFarmApplicationPhysicalPath);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
