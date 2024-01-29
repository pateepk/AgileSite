using System;
using System.Runtime.Serialization;

using CMS.Core;


namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Thrown when object serialization fails.
    /// </summary>
    [Serializable]
    internal class ObjectSerializationException : SerializationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializationException"/> class with default values.
        /// </summary>
        public ObjectSerializationException()
        {
            Source = ModuleName.CONTINUOUSINTEGRATION;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ObjectSerializationException(string message)
            : base(message)
        {
            Source = ModuleName.CONTINUOUSINTEGRATION;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ObjectSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Source = ModuleName.CONTINUOUSINTEGRATION;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ObjectSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}