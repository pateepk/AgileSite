using CMS.Core;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Web farm task used to refresh the route table.
    /// </summary>
    internal class RefreshAliasRoutesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the node identifier which routes will be refreshed.
        /// </summary>
        public int NodeID { get; set; }


        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CMSDocumentRouteHelper.RefreshAliasRoutes(int, string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            CMSDocumentRouteHelper.RefreshAliasRoutes(NodeID, SiteName, logWebFarmTask: false);
        }
    }
}
