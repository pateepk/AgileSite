using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to clear all class structure infos.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearClassStructureInfosWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ClassStructureInfo.Clear(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            ClassStructureInfo.Clear(false);
        }
    }
}
