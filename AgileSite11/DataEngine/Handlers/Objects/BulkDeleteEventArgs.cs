using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk delete event arguments.
    /// </summary>
    public sealed class BulkDeleteEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Processed type info.
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Where condition for the bulk delete.
        /// </summary>
        public IWhereCondition WhereCondition
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkDeleteEventArgs"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use constructor with type info and where condition.")]
        public BulkDeleteEventArgs()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkDeleteEventArgs"/>.
        /// </summary>
        /// <param name="typeInfo">Processed type info</param>
        /// <param name="whereCondition">Where condition for the bulk update</param>
        public BulkDeleteEventArgs(ObjectTypeInfo typeInfo, IWhereCondition whereCondition)
        {
            TypeInfo = typeInfo;
            WhereCondition = whereCondition;
        }
    }
}
