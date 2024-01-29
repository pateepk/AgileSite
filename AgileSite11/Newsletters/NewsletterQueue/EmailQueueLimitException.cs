using System;
using System.Runtime.Serialization;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Represents errors that occur during the email generating once the provided license does not cover <see cref="FeatureEnum.FullContactManagement"/> and number of generated emails exceeds the limit.
    /// </summary>
    [Serializable]
    public class EmailQueueLimitException : Exception
    {
        /// <summary>
        /// Gets the limit that was exceeded.
        /// </summary>
        public int Limit
        {
            get;
            private set;
        }


        /// <summary>
        /// Instatiates a new instance of the <see cref="EmailQueueLimitException" /> class.
        /// </summary>
        /// <param name="limit">Limit that was exceeded</param>
        public EmailQueueLimitException(int limit)
        {
            Limit = limit;
        }


        /// <summary>
        /// Instatiates a new instance of the <see cref="EmailQueueLimitException" /> class with a specified error message.
        /// </summary>
        /// <param name="limit">Limit that was exceeded</param>
        /// <param name="message">The message that describes the error</param>
        public EmailQueueLimitException(int limit, string message) : base(message)
        {
            Limit = limit;
        }


        /// <summary>
        /// Instatiates a new instance of the <see cref="EmailQueueLimitException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception..
        /// </summary>
        /// <param name="limit">Limit that was exceeded</param>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public EmailQueueLimitException(int limit, string message, Exception innerException) : base(message, innerException)
        {
            Limit = limit;
        }


        /// <summary>
        /// Instatiates a new instance of the <see cref="EmailQueueLimitException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="EmailQueueLimitException" /> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="EmailQueueLimitException" /> that contains contextual information about the source or destination</param>
        protected EmailQueueLimitException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
