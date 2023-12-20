using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to delete meta files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteMetaFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site to which file is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets name of the file which will be deleted.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates whether delete specified directory.
        /// </summary>
        public bool DeleteDirectory { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteMetaFileWebFarmTask"/>.
        /// </summary>
        public DeleteMetaFileWebFarmTask()
        {
            TaskTarget = "FileDelete";
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the meta files is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return DataTasks.SynchronizeMetaFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MetaFileInfoProvider.DeleteFile(string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MetaFileInfoProvider.DeleteFile(SiteName, FileName, DeleteDirectory, true);
        }
    }
}
