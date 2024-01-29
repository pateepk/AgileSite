using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Integration task handler
    /// </summary>
    public class IntegrationTaskHandler : AdvancedHandler<IntegrationTaskHandler, IntegrationTaskEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="task">Task object</param>
        /// <param name="infoObj">Task object</param>
        public IntegrationTaskHandler StartEvent(IntegrationTaskInfo task, BaseInfo infoObj)
        {
            var e = new IntegrationTaskEventArgs()
            {
                Task = task,
                Object = infoObj
            };

            return StartEvent(e, true);
        }
    }
}