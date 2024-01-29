using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Thrown when repository configuration is invalid.
    /// </summary>
    [Serializable]
    public class RepositoryConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryConfigurationException"/> class with default values.
        /// </summary>
        public RepositoryConfigurationException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryConfigurationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RepositoryConfigurationException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public RepositoryConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryConfigurationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected RepositoryConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
