using CMS.Helpers;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Synchronization helper class for the windows service module.
    /// </summary>
    internal static class WinServiceSynchronization
    {
        /// <summary>
        /// Initializes the tasks for windows service synchronization.
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<DeleteServiceWebFarmTask>();
            WebFarmHelper.RegisterTask<RestartServiceWebFarmTask>();
        }
    }
}
