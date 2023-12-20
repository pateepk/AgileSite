using CMS.Core;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Web farm task for deleting external service.
    /// </summary>
    internal class DeleteServiceWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Name of the service definition to be deleted.
        /// </summary>
        public string ServiceDefinitionName { get; set; }


        /// <summary>
        /// Deletes service restart file.
        /// </summary>
        /// <remarks>If no <see cref="ServiceDefinitionName"/> given, all services are deleted.</remarks>
        public override void ExecuteTask()
        {
            WinServiceHelper.DeleteServiceFile(ServiceDefinitionName, false);
        }
    }
}
