using System;
using System.Runtime.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// Throws when system requests feature with insufficient license
    /// </summary>
    [Serializable]
    public class LicenseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseException"/> class with default values.
        /// </summary>
        public LicenseException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public LicenseException(string message, Exception inner = null)
            : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected LicenseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
