using CMS.Core;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Web farm task used to drop all routes from the table.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DropAllRoutesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CMSDocumentRouteHelper.DropAllRoutes(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            CMSDocumentRouteHelper.DropAllRoutes(logWebFarmTask: false);
        }
    }
}
