using CMS.Helpers;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Web farm synchronization for the route table
    /// </summary>
    internal static class RouteTableSynchronization
    {
        /// <summary>
        /// Initializes the tasks for the route table synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<DropAllRoutesWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RefreshPageRoutesWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RefreshAliasRoutesWebFarmTask>(true);
        }
    }
}
