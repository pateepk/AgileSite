using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Staging event arguments
    /// </summary>
    public class StagingLogTaskEventArgs : StagingTaskEventArgs
    {
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