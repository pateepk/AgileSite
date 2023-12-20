using CMS.Core;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Clears Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearFormResolversWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="FormEngineWebUIResolvers.ClearResolvers(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            FormEngineWebUIResolvers.ClearResolvers(false);
        }
    }
}
