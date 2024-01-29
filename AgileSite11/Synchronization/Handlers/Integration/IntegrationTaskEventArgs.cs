using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Integration task event arguments
    /// </summary>
    public class IntegrationTaskEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Integration task
        /// </summary>
        public IntegrationTaskInfo Task 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Task object
        /// </summary>
        public BaseInfo Object
        {
            get;
            set;
        }
    }
}