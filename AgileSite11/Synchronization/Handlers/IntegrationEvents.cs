using System;

namespace CMS.Synchronization
{
    /// <summary>
    /// Integration events
    /// </summary>
    public static class IntegrationEvents
    {
        #region "Events"

        /// <summary>
        /// Fires on the server when the integration bus logs the task
        /// </summary>
        public static IntegrationTaskHandler LogInternalTask = new IntegrationTaskHandler { Name = "IntegrationEvents.LogInternalTask" };


        /// <summary>
        /// Fires on the server when the integration bus logs the external task
        /// </summary>
        public static IntegrationTaskHandler LogExternalTask = new IntegrationTaskHandler { Name = "IntegrationEvents.LogExternalTask" };

        #endregion
    }
}