using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk insert event arguments.
    /// </summary>
    public sealed class BulkInsertEventArgs : CMSEventArgs
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
        /// Inserted objects.
        /// </summary>
        /// <remarks>Objects in the collection are not complete (ID is not set) and are not intended for additional processing</remarks>
        public IEnumerable<IInfo> InsertedObjects
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkInsertEventArgs"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use constructor with parameters instead. ")]
        public BulkInsertEventArgs()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkInsertEventArgs"/>.
        /// </summary>
        /// <param name="typeInfo">Processed type info</param>
        /// <param name="objects">Inserted objects</param>
        public BulkInsertEventArgs(ObjectTypeInfo typeInfo, IEnumerable<IInfo> objects)
        {
            TypeInfo = typeInfo;
            InsertedObjects = objects;
        }
    }
}
