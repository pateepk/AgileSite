using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CMS.EventLog
{
    /// <summary>
    /// Thrown when recursion is detected while logging an event.
    /// </summary>
    [Serializable]
    internal sealed class EventLoggingRecursionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLoggingRecursionException"/> class with default values.
        /// </summary>
        public EventLoggingRecursionException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLoggingRecursionException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EventLoggingRecursionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLoggingRecursionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public EventLoggingRecursionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLoggingRecursionException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        private EventLoggingRecursionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
