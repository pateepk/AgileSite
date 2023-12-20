using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Thrown when the LinkedIn API throttle limit is reached.
    /// The HTTP status code in such case is most certainly a 403.
    /// </summary>
    public class LinkedInApiThrottleLimitException : LinkedInApiException
    {
        /// <summary>
        /// Thrown when the LinkedIn API throttle limit is reached.
        /// </summary>
        public LinkedInApiThrottleLimitException()
        {
        }


        /// <summary>
        /// Thrown when the LinkedIn API throttle limit is reached.
        /// </summary>
        /// <param name="message">Exception message</param>
        public LinkedInApiThrottleLimitException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Thrown when the LinkedIn API throttle limit is reached.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public LinkedInApiThrottleLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Thrown when the LinkedIn API throttle limit is reached.
        /// </summary>
        /// <param name="httpStatus">HTTP code of LinkedIn API response</param>
        /// <param name="errorCode">Error code of LinkedIn API response (originates from the API response body)</param>
        /// <param name="message">Exception message</param>
        public LinkedInApiThrottleLimitException(int httpStatus, int errorCode, string message)
            : base(httpStatus, errorCode, message)
        {

        }
    }
}
