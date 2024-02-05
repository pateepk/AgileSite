using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object TypeInfo event arguments
    /// </summary>
    internal class ObjectTypeInfoEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Processed type info
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the check. If true objects specified by <see cref="TypeInfo"/> will be processed.
        /// </summary>
        public bool Result
        {
            get;
            set;
        }
    }
}
