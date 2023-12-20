using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Web farm task used to clear custom tables hash count values.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    internal class ClearCustomTableLicensesCountWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CustomTableItemProvider.ClearLicensesCount"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            CustomTableItemProvider.ClearLicensesCount(false);
        }
    }
}
