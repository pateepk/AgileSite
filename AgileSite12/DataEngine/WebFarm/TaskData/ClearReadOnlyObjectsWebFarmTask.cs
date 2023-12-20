using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to clear all read only objects.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearReadOnlyObjectsWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ModuleManager.ClearReadOnlyObjects(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            ModuleManager.ClearReadOnlyObjects(false);
        }
    }
}
