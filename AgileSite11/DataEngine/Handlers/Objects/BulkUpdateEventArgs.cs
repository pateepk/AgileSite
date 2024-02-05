using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk update event arguments.
    /// </summary>
    public class BulkUpdateEventArgs : CMSEventArgs
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
        /// Where condition for the bulk update.
        /// </summary>
        public IWhereCondition WhereCondition
        {
            get;
            private set;
        }


        /// <summary>
        /// Names of the columns that are changed by the bulk update. Any object's column can be changed if this collection is not set or empty.
        /// </summary>
        public IEnumerable<string> ChangedColumns
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use constructor with parameters instead. ")]
        public BulkUpdateEventArgs()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeInfo">Processed type info</param>
        /// <param name="whereCondition">Where condition for the bulk update</param>
        /// <param name="changedColumns">Names of the columns that are changed by the bulk update. Any object's column can be changed if this collection is not set or empty.</param>
        public BulkUpdateEventArgs(ObjectTypeInfo typeInfo, IWhereCondition whereCondition, IEnumerable<string> changedColumns = null)
        {
            TypeInfo = typeInfo;
            WhereCondition = whereCondition;
            if (changedColumns != null)
            {
                ChangedColumns = new HashSet<string>(changedColumns, StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}
