using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Staging handler
    /// </summary>
    public class StagingSynchronizationHandler : AdvancedHandler<StagingSynchronizationHandler, StagingSynchronizationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="objectType">Object type</param>
        /// <param name="taskData">Task data</param>
        /// <param name="taskBinaryData">Task binary data</param>
        /// <param name="syncManager">Instance of SyncManager</param>
        public StagingSynchronizationHandler StartEvent(TaskTypeEnum taskType, string objectType, DataSet taskData, DataSet taskBinaryData, ISyncManager syncManager)
        {
            var e = new StagingSynchronizationEventArgs()
            {
                TaskType = taskType,
                ObjectType = objectType,
                TaskData = taskData,
                TaskBinaryData = taskBinaryData,
                SyncManager = syncManager
            };

            return StartEvent(e, true);
        }
    }
}