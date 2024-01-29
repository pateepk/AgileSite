using System;

namespace WTE.Communication
{
    public class FaxResponse : CommunicationResponse
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FaxResponse()
            : base()
        {
        }

        /// <summary>
        /// Construct Response with type and message
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        public FaxResponse(ResponseCodeType p_type, string p_message)
            : this(p_type, p_message, null)
        {
        }

        /// <summary>
        /// Create Response with code and message and additional data
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        /// <param name="p_additionalData"></param>
        public FaxResponse(ResponseCodeType p_type, string p_message, Object p_additionalData)
            : base(p_type, p_message, p_additionalData)
        {
        }
    }
}