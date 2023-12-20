using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Thrown when trying to create a SharePoint file which already exists.
    /// </summary>
    public class SharePointFileAlreadyExistsException : Exception
    {
        /// <summary>
        /// Thrown when trying to create a SharePoint file which already exists.
        /// </summary>
        public SharePointFileAlreadyExistsException()
        {
        }


        /// <summary>
        /// Thrown when trying to create a SharePoint file which already exists.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointFileAlreadyExistsException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Thrown when trying to create a SharePoint file which already exists.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointFileAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
