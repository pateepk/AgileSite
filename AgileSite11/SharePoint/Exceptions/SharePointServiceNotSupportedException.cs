using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Indicates the desired SharePoint service does not have an implementation.
    /// </summary>
    public class SharePointServiceNotSupportedException : Exception
    {
        /// <summary>
        /// Indicates the desired SharePoint service does not have an implementation.
        /// </summary>
        public SharePointServiceNotSupportedException()
        {
        }


        /// <summary>
        /// Indicates the desired SharePoint service does not have an implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointServiceNotSupportedException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Indicates the desired SharePoint service does not have an implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointServiceNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
