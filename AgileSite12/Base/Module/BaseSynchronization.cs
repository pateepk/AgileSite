using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Base module synchronization class (debug synchronization is registered here for example).
    /// </summary>
    internal class BaseSynchronization
    {
        /// <summary>
        /// Initializes the tasks for debug synchronization.
        /// </summary>
        public static void Init()
        {
            CoreServices.WebFarm.RegisterTask<ResetDebugSettingsWebFarmTask>(true);
        }
    }
}
