using System.Collections.Generic;

using CMS.Base;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Event argument wrapper object for <see cref="ContentStagingTaskCollection.TasksCollected"/> event.
    /// </summary>
    public class TasksCollectedEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Staging tasks.
        /// </summary>
        public IReadOnlyList<StagingTask> Tasks
        {
            get;
            set;
        }
    }
}
