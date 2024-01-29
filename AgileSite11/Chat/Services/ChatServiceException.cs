using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Exception used to pass status code and status message to the service.
    /// </summary>
    public class ChatServiceException : Exception
    {
        #region "Constructors"

        /// <summary>
        /// Constucts exception. Message is set to the default value associated with status code.
        /// </summary>
        /// <param name="statusCode">Code of the response</param>
        public ChatServiceException(ChatResponseStatusEnum statusCode) : this(statusCode, statusCode.ToStringValue())
        {
        }


        /// <summary>
        /// Constructs exception.
        /// </summary>
        /// <param name="statusCode">Code</param>
        /// <param name="statusMessage">Status message</param>
        public ChatServiceException(ChatResponseStatusEnum statusCode, string statusMessage) : base(statusMessage)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Status code of chat response.
        /// </summary>
        public ChatResponseStatusEnum StatusCode { get; set; }


        /// <summary>
        /// Message of chat response.
        /// </summary>
        public string StatusMessage { get; set; }

        #endregion
    }
}
