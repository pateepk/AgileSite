using System;
using System.Runtime.Serialization;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Thrown when a document has a culture which is not allowed on the site.
    /// </summary>
    [Serializable]
    public class DocumentCultureNotAllowedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCultureNotAllowedException"/> class with default values.
        /// </summary>
        public DocumentCultureNotAllowedException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCultureNotAllowedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DocumentCultureNotAllowedException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCultureNotAllowedException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DocumentCultureNotAllowedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCultureNotAllowedException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DocumentCultureNotAllowedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
