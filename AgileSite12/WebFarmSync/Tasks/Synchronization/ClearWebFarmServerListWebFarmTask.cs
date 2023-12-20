using CMS.Core;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm task used to clear the list of web farm servers.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearWebFarmServerListWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="WebFarmContext.Clear(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            WebFarmContext.Clear(false);
        }
    }
}
