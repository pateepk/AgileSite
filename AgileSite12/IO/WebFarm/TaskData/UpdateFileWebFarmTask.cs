using System;

using CMS.Core;

namespace CMS.IO
{
    /// <summary>
    /// Web farm task used to update files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets path of the file to be updated.
        /// </summary>
        public string Path { get; set; }


        /// <summary>
        /// Returns true if the synchronization of the physical files is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return CoreServices.WebFarm.SynchronizePhysicalFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="File.WriteAllBytes(string, byte[])"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            if (String.IsNullOrEmpty(Path) || (TaskBinaryData.Data == null))
            {
                return;
            }

            var filePath = StorageHelper.GetFullFilePhysicalPath(Path, StorageTasks.WebFarmApplicationPhysicalPath);
            DirectoryHelper.EnsureDiskPath(filePath, StorageTasks.WebFarmApplicationPhysicalPath);
            File.WriteAllBytes(filePath, TaskBinaryData.Data);
        }
    }
}
