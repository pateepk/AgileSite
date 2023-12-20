using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to remove classes.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveClassStructureInfoWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the name of the class to be removed.
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ClassStructureInfo.Remove(string, bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            ClassStructureInfo.Remove(ClassName, false);
        }
    }
}
