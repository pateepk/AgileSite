using System;
using System.Runtime.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// Thrown when application initialization failed
    /// </summary>
    [Serializable]
    public class ApplicationInitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInitException"/> class with default values.
        /// </summary>
        public ApplicationInitException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInitException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ApplicationInitException(string message, Exception inner = null)
          : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInitException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ApplicationInitException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
