using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Staging handler
    /// </summary>
    public class StagingLogTaskHandler : AdvancedHandler<StagingLogTaskHandler, StagingLogTaskEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="task">Task object</param>
        /// <param name="infoObj">Info object</param>
        public StagingLogTaskHandler StartEvent(StagingTaskInfo task, BaseInfo infoObj)
        {
            var e = new StagingLogTaskEventArgs()
            {
                Task = task,
                Object = infoObj
            };

            return StartEvent(e, true);
        }
    }
}