using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Web farm task used to clear all custom table type infos.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearCustomTableTypeInfosWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CustomTableItemProvider.Clear"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            CustomTableItemProvider.Clear(false);
        }
    }
}
