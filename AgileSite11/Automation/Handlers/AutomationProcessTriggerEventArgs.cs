using System.Collections.Generic;

using CMS.Base;

namespace CMS.Automation
{
    /// <summary>
    /// Automation process trigger event arguments
    /// </summary>
    public class AutomationProcessTriggerEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Trigger options.
        /// </summary>
        public IEnumerable<TriggerOptions> Options
        {
            get;
            internal set;
        }


        /// <summary>
        /// Trigger info object.
        /// </summary>
        public ObjectWorkflowTriggerInfo TriggerInfo
        {
            get;
            internal set;
        }
    }
}