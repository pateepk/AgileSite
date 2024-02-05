using System;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Thrown when delete operation of LinkedIn content is not executed completely.
    /// </summary>
    public class LinkedInPartialDeleteException : Exception
    {
        /// <summary>
        ///  Thrown when delete operation of LinkedIn content is not executed completely.
        /// </summary>
        public LinkedInPartialDeleteException()
        {
        }


        /// <summary>
        ///  Thrown when delete operation of LinkedIn content is not executed completely.
        /// </summary>
        /// <param name="message">Exception message</param>
        public LinkedInPartialDeleteException(string message)
            : base(message)
        {
        }


        /// <summary>
        ///  Thrown when delete operation of LinkedIn content is not executed completely.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public LinkedInPartialDeleteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
