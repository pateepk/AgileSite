using CMS.Core;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Web farm task for restarting external service.
    /// </summary>
    internal class RestartServiceWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Name of the service definition to be restarted.
        /// </summary>
        public string ServiceDefinitionName { get; set; }


        /// <summary>
        /// Restarts given windows service(s).
        /// </summary>
        /// <remarks>If no <see cref="ServiceDefinitionName"/> given, all services are restarted.</remarks>
        public override void ExecuteTask()
        {
            WinServiceHelper.RestartService(ServiceDefinitionName, false);
        }
    }
}
