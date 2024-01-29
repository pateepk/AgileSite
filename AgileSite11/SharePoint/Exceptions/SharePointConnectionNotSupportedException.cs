using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Indicates that given SharePoint connection is not suitable for SharePoint service implementation.
    /// </summary>
    public class SharePointConnectionNotSupportedException : Exception
    {
        /// <summary>
        /// Indicates that given SharePoint connection is not suitable for SharePoint service implementation.
        /// </summary>
        public SharePointConnectionNotSupportedException()
        {
        }


        /// <summary>
        /// Indicates that given SharePoint connection is not suitable for SharePoint service implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointConnectionNotSupportedException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Indicates that given SharePoint connection is not suitable for SharePoint service implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointConnectionNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
