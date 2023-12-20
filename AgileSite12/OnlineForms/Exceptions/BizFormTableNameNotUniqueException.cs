using System;
using System.Runtime.Serialization;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Thrown when biz form table name is not unique.
    /// </summary>
    [Serializable]
    public class BizFormTableNameNotUniqueException : BizFormException
    {
        private readonly string mTableName;


        /// <summary>
        /// Gets the table name.
        /// </summary>
        public virtual string TableName
        {
            get
            {
                return mTableName;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with default values.
        /// </summary>
        public BizFormTableNameNotUniqueException()
          : base()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BizFormTableNameNotUniqueException(string message)
          : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with a specified error message and a table name that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="tableName">The table name that is the cause of this exception.</param>
        public BizFormTableNameNotUniqueException(string message, string tableName)
          : base(message)
        {
            mTableName = tableName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public BizFormTableNameNotUniqueException(string message, Exception inner)
          : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with a specified error message, a table name and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="tableName">The table name that is the cause of this exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public BizFormTableNameNotUniqueException(string message, string tableName, Exception inner)
          : base(message, inner)
        {
            mTableName = tableName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormTableNameNotUniqueException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected BizFormTableNameNotUniqueException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            mTableName = info.GetString(nameof(TableName));
        }


        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The S<see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="info"/> is null.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);

            info.AddValue(nameof(TableName), mTableName, typeof(String));
        }
    }
}
