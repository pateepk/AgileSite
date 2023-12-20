using CMS.ContinuousIntegration.Internal;
using CMS.Core;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Web farm task used to rebuild CI configuration.
    /// </summary>
    internal class RebuildConfigurationWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Rebuilds CI configuration.
        /// </summary>
        public override void ExecuteTask()
        {
            ContinuousIntegrationEventHandling.RebuildConfiguration(false);
        }
    }
}
