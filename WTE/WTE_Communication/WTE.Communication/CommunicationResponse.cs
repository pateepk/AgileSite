using System;

namespace WTE.Communication
{
    /// <summary>
    /// class for holding the result of a communication request
    /// </summary>
    public class CommunicationResponse
    {
        public enum ResponseCodeType
        {
            Success, ///< No errors
            SuccessWithWarnings, ///< No errors but warnings
            Error ///< and error occurred
        }

        private ResponseCodeType m_code = ResponseCodeType.Success;
        private String m_message = String.Empty;
        private Object m_additionalData = null;

        /// <summary>
        /// The response code
        ///  0 - success
        ///  1 - success with warnings
        ///  2 - error
        /// </summary>
        public ResponseCodeType Code
        {
            get { return m_code; }
            set { m_code = value; }
        }

        /// <summary>
        /// Warning/error description
        /// </summary>
        public String Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Any additional data
        /// </summary>
        public Object AdditionalData
        {
            get
            {
                return m_additionalData;
            }
            set
            {
                m_additionalData = value;
            }
        }

        /// <summary>
        /// Successful response constructor
        /// </summary>
        public CommunicationResponse()
        {
        }

        /// <summary>
        /// Create Response with code and message
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        public CommunicationResponse(ResponseCodeType p_type, string p_message)
            : this(p_type, p_message, null)
        {
        }

        /// <summary>
        /// Create Response with code and message and additional data
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_message"></param>
        /// <param name="p_additionalData"></param>
        public CommunicationResponse(ResponseCodeType p_type, string p_message, Object p_additionalData)
            : base()
        {
            m_code = p_type;
            m_message = p_message;
            m_additionalData = p_additionalData;
        }
    }
}