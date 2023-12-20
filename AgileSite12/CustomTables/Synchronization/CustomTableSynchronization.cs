using CMS.Helpers;

namespace CMS.CustomTables
{
    /// <summary>
    /// Web farm synchronization for custom table items
    /// </summary>
    internal class CustomTableSynchronization
    {
        /// <summary>
        /// Initializes the tasks for custom table items synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<InvalidateCustomTableTypeInfoWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearCustomTableTypeInfosWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearCustomTableLicensesCountWebFarmTask>(true);
        }
    }
}
