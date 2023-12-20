using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Data classes event handling
    /// Rebuilds CI configuration when any data class is changed
    /// </summary>
    internal class ContinuousIntegrationRepositoryHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        internal static void Init()
        {
            ObjectEvents.Insert.After += (sender, objectEventArgs) => ResetCacheIfRequired(objectEventArgs);
            ObjectEvents.Delete.After += (sender, objectEventArgs) => ResetCacheIfRequired(objectEventArgs);
        }


        private static void ResetCacheIfRequired(ObjectEventArgs objectEventArgs)
        {
            if (!RepositoryActionContext.CurrentIsRestoreOperationRunning && ContinuousIntegrationHelper.IsObjectSerializationEnabled)
            {
                if (objectEventArgs.Object is DataClassInfo)
                {
                    ContinuousIntegrationEventHandling.RebuildConfiguration();
                }
            }
        }
    }
}
