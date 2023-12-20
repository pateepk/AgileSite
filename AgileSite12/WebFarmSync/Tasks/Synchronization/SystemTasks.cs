using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm synchronization for system
    /// </summary>
    internal class SystemTasks
    {
        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<ClearWebFarmServerListWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RestartApplicationWebFarmTask>(true);
        }
    }
}
