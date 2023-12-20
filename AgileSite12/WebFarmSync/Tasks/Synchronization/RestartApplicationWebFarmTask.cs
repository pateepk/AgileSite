using CMS.Core;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm task used to restart application.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RestartApplicationWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RestartApplicationWebFarmTask"/>.
        /// </summary>
        public RestartApplicationWebFarmTask()
        {
            TaskTarget = "RestartApplication";
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SystemHelper.RestartApplication()"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            SystemHelper.RestartApplication();
        }
    }
}
