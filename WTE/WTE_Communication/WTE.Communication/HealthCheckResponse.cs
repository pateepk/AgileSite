using System;

namespace WTE.Communication
{
    /// <summary>
    /// Class for holding Health check response
    /// </summary>
    public class HealthCheckResponse : CommunicationResponse
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public HealthCheckResponse()
            : base()
        {
        }

        /// <summary>
        /// Construct response with type and message
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        public HealthCheckResponse(ResponseCodeType p_type, string p_message)
            : this(p_type, p_message, null)
        {
        }

        /// <summary>
        /// Create Response with code and message and additional data
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        /// <param name="p_additionalData"></param>
        public HealthCheckResponse(ResponseCodeType p_type, string p_message, Object p_additionalData)
            : base(p_type, p_message, p_additionalData)
        {
        }
    }
}