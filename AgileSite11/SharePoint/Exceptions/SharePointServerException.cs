using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Thrown when error at SharePoint server occurs.
    /// </summary>
    public class SharePointServerException : Exception
    {
        /// <summary>
        /// Thrown when error at SharePoint server occurs.
        /// </summary>
        public SharePointServerException()
        {
        }


        /// <summary>
        /// Thrown when error at SharePoint server occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointServerException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Thrown when error at SharePoint server occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
