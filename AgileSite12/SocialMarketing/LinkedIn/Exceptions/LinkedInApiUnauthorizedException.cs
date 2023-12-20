using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Thrown when protocol error in communication with LinkedIn API has the root cause in not being authorized.
    /// The HTTP status code in such case is most certainly a 401.
    /// </summary>
    public class LinkedInApiUnauthorizedException : LinkedInApiException
    {
        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API has the root cause in not being authorized.
        /// </summary>
        public LinkedInApiUnauthorizedException()
        {
        }


        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API has the root cause in not being authorized.
        /// </summary>
        /// <param name="message">Exception message</param>
        public LinkedInApiUnauthorizedException(string message)
            : base(message)
        {
        }


        /// <summary>
        ///  Thrown when protocol error in communication with LinkedIn API has the root cause in not being authorized.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public LinkedInApiUnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Thrown when protocol error in communication with LinkedIn API has the root cause in not being authorized.
        /// </summary>
        /// <param name="httpStatus">HTTP code of LinkedIn API response</param>
        /// <param name="errorCode">Error code of LinkedIn API response (originates from the API response body)</param>
        /// <param name="message">Exception message</param>
        public LinkedInApiUnauthorizedException(int httpStatus, int errorCode, string message)
            : base(httpStatus, errorCode, message)
        {
        }
    }
}
