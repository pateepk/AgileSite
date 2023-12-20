using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    using SyncAction = Func<LogObjectChangeSettings, IEnumerable<ISynchronizationTask>>;

    /// <summary>
    /// Synchronization action implementation
    /// </summary>
    public class SynchronizationAction
    {
        /// <summary>
        /// Condition which must be met in order to execute action
        /// </summary>
        public Func<LogObjectChangeSettings, bool> Condition 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Action which should be executed
        /// </summary>
        public SyncAction Execute 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="execute">Method executed to perform the action</param>
        /// <param name="condition">Condition under which the action is executed</param>
        public SynchronizationAction(SyncAction execute, Func<LogObjectChangeSettings, bool> condition)
        {
            Execute = execute;
            Condition = condition;
        }
        
        
        /// <summary>
        /// Executes the given action
        /// </summary>
        /// <param name="settings">Action settings</param>
        public IEnumerable<ISynchronizationTask> ExecuteAction(LogObjectChangeSettings settings)
        {
            return Execute(settings);
        }


        /// <summary>
        /// Returns true if the action should be executed
        /// </summary>
        /// <param name="settings">Action settings</param>
        public bool CanBeExecuted(LogObjectChangeSettings settings)
        {
            return Condition == null || Condition(settings);
        }
    }
}
