using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Thrown when data class requires a table update that can not be performed.
    /// </summary>
    [Serializable]
    internal class DataClassTableUpdateException : Exception
    {
        /// <summary>
        /// Gets the data class that required the table update.
        /// </summary>
        public string DataClassName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets name of the table that could not be updated.
        /// </summary>
        public string TableName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a new instance of <see cref="DataClassTableUpdateException"/>.
        /// </summary>
        protected DataClassTableUpdateException()
        {
        }


        /// <summary>
        /// Creates a new instance of <see cref="DataClassTableUpdateException"/>.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="dataClassName">The data class which required the update.</param>
        /// <param name="tableName">Name of the table that could not be updated.</param>
        /// <param name="inner">Inner exception.</param>
        public DataClassTableUpdateException(string message, string dataClassName, string tableName, Exception inner = null)
            : base(message, inner)
        {
            DataClassName = dataClassName;
            TableName = tableName;
        }


        /// <summary>
        /// Creates a new instance of <see cref="DataClassTableUpdateException"/>.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected DataClassTableUpdateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            DataClassName = info.GetString("DataClassName");
            TableName = info.GetString("TableName");
        }


        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("DataClassName", DataClassName);
            info.AddValue("TableName", TableName);
        }
    }
}
