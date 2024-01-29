using System;
using System.Runtime.Serialization;

namespace CMS.Core
{
    /// <summary>
    /// Thrown when registration of a service fails.
    /// </summary>
    [Serializable]
    public class ServiceRegistrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException"/> class with default values.
        /// </summary>
        public ServiceRegistrationException()
            : base()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ServiceRegistrationException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ServiceRegistrationException(string message, Exception inner)
            : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ServiceRegistrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
