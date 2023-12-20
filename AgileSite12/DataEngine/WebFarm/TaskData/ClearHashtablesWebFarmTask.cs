using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to clear hashtables.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearHashtablesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ModuleManager.ClearHashtables(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            ModuleManager.ClearHashtables(false);
        }
    }
}
