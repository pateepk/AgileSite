using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Base module synchronization class (debug synchronization is registered here for example).
    /// </summary>
    internal class BaseSynchronization
    {
        #region "Properties"

        /// <summary>
        /// Task type for synchronization of reset debug method (when debug settings are changed, the debugs have to be reset).
        /// </summary>
        public const string ResetDebugSettingsTaskType = "RESETDEBUGSETTINGS";

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for debug synchronization.
        /// </summary>
        public static void Init()
        {
            var task = new WebFarmTask
            {
                Type = ResetDebugSettingsTaskType,
                Execute = (target, data, binaryData) => DebugHelper.ResetDebugSettings(false),
                IsMemoryTask = true
            };

            CoreServices.WebFarm.RegisterTask(task);
        }

        #endregion
    }
}
