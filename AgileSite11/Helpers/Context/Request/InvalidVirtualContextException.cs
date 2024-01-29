using System;
using System.Runtime.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Thrown when path processed by <see cref="VirtualContext"/> is invalid
    /// </summary>
    [Serializable]
    public sealed class InvalidVirtualContextException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVirtualContextException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public InvalidVirtualContextException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVirtualContextException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference</exception>
        private InvalidVirtualContextException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}