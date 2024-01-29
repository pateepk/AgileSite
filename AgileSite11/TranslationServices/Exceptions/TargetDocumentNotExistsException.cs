using CMS.Base;
using System;
using System.Runtime.Serialization;

namespace CMS.TranslationServices
{

    /// <summary>
    /// Target document doesn't exist exception.
    /// </summary>
    [Serializable]
    public class TargetDocumentNotExistsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDocumentNotExistsException"/> class with default values.
        /// </summary>
        public TargetDocumentNotExistsException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDocumentNotExistsException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TargetDocumentNotExistsException(string message) 
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDocumentNotExistsException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public TargetDocumentNotExistsException(string message, Exception inner) 
            : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDocumentNotExistsException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected TargetDocumentNotExistsException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
