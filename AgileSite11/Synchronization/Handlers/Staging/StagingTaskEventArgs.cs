using System;

using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Event arguments for the staging task event
    /// </summary>
    public class StagingTaskEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Synchronization task
        /// </summary>
        public StagingTaskInfo Task 
        { 
            get; 
            set; 
        }
    }
}