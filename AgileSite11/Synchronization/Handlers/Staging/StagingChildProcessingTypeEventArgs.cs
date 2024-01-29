using CMS.Base;
using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Event arguments for getting object synchronization processing type
    /// </summary>
    public class StagingChildProcessingTypeEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Indicates how object should be processed
        /// </summary>
        public IncludeToParentEnum ProcessingType
        {
            get;
            set;
        }


        /// <summary>
        /// Currently processed object type
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of parent object of the processed object
        /// </summary>
        public string ParentObjectType
        {
            get;
            set;
        }
    }
}
