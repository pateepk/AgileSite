using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Synchronization event arguments
    /// </summary>
    public class StagingSynchronizationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Task type
        /// </summary>
        public TaskTypeEnum TaskType 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Task data
        /// </summary>
        public DataSet TaskData 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Task binary data
        /// </summary>
        public DataSet TaskBinaryData 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Indicates if current task was handled by event handler.
        /// </summary>
        public bool TaskHandled
        {
            get;
            set;
        }


        /// <summary>
        /// SyncManager instance.
        /// </summary>
        public ISyncManager SyncManager
        {
            get;
            set;
        }
    }
}