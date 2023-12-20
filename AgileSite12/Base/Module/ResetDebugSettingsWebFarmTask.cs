using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Web farm task used to reset debug settings.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ResetDebugSettingsWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="DebugHelper.ResetDebugSettings(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            DebugHelper.ResetDebugSettings(false);
        }
    }
}
