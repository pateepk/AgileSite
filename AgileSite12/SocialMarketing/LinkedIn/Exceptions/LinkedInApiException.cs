using System;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Thrown when protocol error in communication with LinkedIn API occurs.
    /// </summary>
    /// <remarks>
    /// The exception is ment for situations when the error condition arised at LinkedIn server, thus the <see cref="HttpStatus"/> and <see cref="ErrorCode"/> should always be available.
    /// Should not be used in situations when the root cause is local.
    /// </remarks>
    public class LinkedInApiException : Exception
    {
        /// <summary>
        /// HTTP status code which was present in the API's HTTP response.
        /// </summary>
        public int HttpStatus
        {
            get;
            protected set;
        }


        /// <summary>
        /// Error code of the LinkedIn API response. Originates from the API response body.
        /// </summary>
        public int ErrorCode
        {
            get;
            protected set;
        }


        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API occurs.
        /// </summary>
        public LinkedInApiException()
        {
        }


        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        public LinkedInApiException(string message)
            : base(message)
        {
        }


        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public LinkedInApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Thrown when protocol error in communication with LinkedIn API occurs.
        /// </summary>
        /// <param name="httpStatus">HTTP code of LinkedIn API response</param>
        /// <param name="errorCode">Error code of LinkedIn API response (originates from the API response body)</param>
        /// <param name="message">Exception message</param>
        public LinkedInApiException(int httpStatus, int errorCode, string message)
            : base (message)
        {
            HttpStatus = httpStatus;
            ErrorCode = errorCode;
        }
    }
}
