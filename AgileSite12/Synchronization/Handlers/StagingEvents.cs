using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Staging events
    /// </summary>
    public static class StagingEvents
    {
        #region "Events"

        /// <summary>
        /// Fires on the source server when the staging logs the task
        /// </summary>
        public static StagingLogTaskHandler LogTask = new StagingLogTaskHandler { Name = "StagingEvents.LogTask" };


        /// <summary>
        /// Fires on the source server when the staging synchronizes task or a bulk of tasks to the server
        /// </summary>
        public static AdvancedHandler Synchronize = new AdvancedHandler { Name = "StagingEvents.Synchronize" };


        /// <summary>
        /// Fires on the source server when the staging synchronizes task to a server
        /// </summary>
        public static StagingTaskHandler SynchronizeTask = new StagingTaskHandler { Name = "StagingEvents.SynchronizeTask" };


        /// <summary>
        /// Fires on the target server when the staging processes task
        /// </summary> 
        public static StagingSynchronizationHandler ProcessTask = new StagingSynchronizationHandler { Name = "StagingEvents.ProcessTask" };


        /// <summary>
        /// Fires on the target server when the staging updates child objects of created/update main object for each processed child object type
        /// </summary>
        public static StagingChildProcessingTypeHandler GetChildProcessingType = new StagingChildProcessingTypeHandler { Name = "StagingEvents.GetChildProcessingType" };
        
        #endregion
    }
}