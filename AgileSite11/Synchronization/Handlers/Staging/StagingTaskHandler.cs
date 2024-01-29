using System;

using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Staging handler
    /// </summary>
    public class StagingTaskHandler : AdvancedHandler<StagingTaskHandler, StagingTaskEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="task">Task object</param>
        public StagingTaskHandler StartEvent(StagingTaskInfo task)
        {
            var e = new StagingTaskEventArgs()
            {
                Task = task
            };

            return StartEvent(e, true);
        }
    }
}