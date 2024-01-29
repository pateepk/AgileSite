using System;

namespace WTE.Communication
{
    /// <summary>
    /// class for holding the result of an email send
    /// </summary>
    public class EmailResponse : CommunicationResponse
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public EmailResponse()
            : base()
        {
        }

        /// <summary>
        /// Construct Response with type and message
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        public EmailResponse(ResponseCodeType p_type, string p_message)
            : this(p_type, p_message, null)
        {
        }

        /// <summary>
        /// Create Response with code and message and additional data
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        /// <param name="p_additionalData"></param>
        public EmailResponse(ResponseCodeType p_type, string p_message, Object p_additionalData)
            : base(p_type, p_message, p_additionalData)
        {
        }
    }
}