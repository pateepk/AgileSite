using System;

using CMS.Base;
using CMS.Core;

namespace CMS.IO
{
    /// <summary>
    /// Web farm task used to delete physical files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets path of the file to be deleted.
        /// </summary>
        public string Path { get; set; }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the physical files is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return CoreServices.WebFarm.SynchronizePhysicalFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="File.Delete(string)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            if (String.IsNullOrEmpty(Path))
            {
                return;
            }

            var filePath = StorageHelper.GetFullFilePhysicalPath(Path, StorageTasks.WebFarmApplicationPhysicalPath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
